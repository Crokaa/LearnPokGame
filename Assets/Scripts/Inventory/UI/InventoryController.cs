using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy, MoveToForget, ForgetDone }

public class InventoryController : MonoBehaviour
{

    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Text categoryText;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    int[] currSelectedonCategory;
    //int currentSelected = 0;
    int categorySelected = 0;
    RectTransform itemListRect;
    const int itemsInViewport = 8;
    MoveBase moveToLearn;
    InventoryUIState currentState;
    Action<ItemBase> onItemUsed;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
        currSelectedonCategory = new int[Inventory.ItemCategories.Count];

    }

    private void Start()
    {
        categoryText.text = Inventory.ItemCategories[categorySelected];
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        slotUIList = new List<ItemSlotUI>();
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var itemSlot in inventory.GetSlotByCategory(categorySelected))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateInventorySelection();
    }
    public void HandleUpdate(Action goBack, Action<ItemBase> onItemUsed = null)
    {

        this.onItemUsed = onItemUsed;

        if (currentState == InventoryUIState.ItemSelection)
        {
            int prevSelected = currSelectedonCategory[categorySelected];
            int prevCategory = categorySelected;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                currSelectedonCategory[categorySelected]++;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                currSelectedonCategory[categorySelected]--;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                categorySelected--;
                if (categorySelected < 0)
                    categorySelected = Inventory.ItemCategories.Count - 1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                categorySelected++;
                if (categorySelected > Inventory.ItemCategories.Count - 1)
                    categorySelected = 0;
            }

            currSelectedonCategory[categorySelected] = Mathf.Clamp(currSelectedonCategory[categorySelected], 0, inventory.GetSlotByCategory(categorySelected).Count
                - 1 > 0 ? inventory.GetSlotByCategory(categorySelected).Count - 1 : 0);


            if (prevCategory != categorySelected)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[categorySelected];
                UpdateItemList();
            }

            else if (prevSelected != currSelectedonCategory[categorySelected])
                UpdateInventorySelection();

            if (Input.GetKeyDown(KeyCode.Z))
                StartCoroutine(ItemSelected());

            else if (Input.GetKeyDown(KeyCode.X))
            {
                goBack?.Invoke();
            }
        }
        else if (currentState == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
                {
                    // Use item
                    currentState = InventoryUIState.Busy;
                    StartCoroutine(UseItem());
                };

            Action goBackPartyScreen = () =>
            {
                ClosePartySelection();
            };

            partyScreen.HandleUpdate(onSelected, goBackPartyScreen);
        }
        else if (currentState == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForget(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator OnMoveToForget(int moveIndex)
    {

        var pokemon = partyScreen.SelectedPokemon;
        currentState = InventoryUIState.Busy;

        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == PokemonBase.MaxNumMoves)
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} did not learn the move {moveToLearn.Name}");
        }
        else
        {
            var selectedMove = pokemon.Moves[moveIndex].Base;
            yield return PrintMoveLearned(selectedMove, moveToLearn, pokemon);
            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }
        moveToLearn = null;
        currentState = InventoryUIState.ForgetDone;
    }

    IEnumerator PrintMoveLearned(MoveBase selectedMove, MoveBase moveToLearn, Pokemon pokemon)
    {
        yield return DialogManager.Instance.ShowDialogText("One...two...and...ta-da!");
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} forgot {selectedMove.Name}...");
        yield return DialogManager.Instance.ShowDialogText($"And it learned {moveToLearn.Name} instead!");

    }

    void ResetSelection()
    {
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemDescription.text = "";
        itemIcon.sprite = null;
    }

    IEnumerator UseItem()
    {
        //currentState = InventoryUIState.Busy;

        yield return HandleTmItems();

        var item = inventory.UseItem(currSelectedonCategory[categorySelected], partyScreen.SelectedPokemon, categorySelected);
        if (item != null)
        {
            if (item is RecoveryItem)
                yield return DialogManager.Instance.ShowDialogText($"{item.OnShowMessage}");
                
            onItemUsed?.Invoke(item);
        }
        else 
        {
            if (categorySelected == (int) ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText("It won't have any effect.");
        }

        ClosePartySelection();

    }

    IEnumerator HandleTmItems()
    {

        var item = inventory.GetItem(currSelectedonCategory[categorySelected], categorySelected) as TmHmItem;

        if (item == null)
            yield break;

        var pokemon = partyScreen.SelectedPokemon;

        if (pokemon.HasMove(item.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} already knows {item.Move.Name}.");
            yield break;
        }

        if (!item.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} and {item.Move.Name} are not compatible.");
            yield return DialogManager.Instance.ShowDialogText($"{item.Move.Name} can't be learned.");
            yield break;
        }

        if (pokemon.Moves.Count < PokemonBase.MaxNumMoves)
        {
            pokemon.LearnMove(item.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} has learned {item.Move.Name}.");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is trying to learn {item.Move.Name}.");
            yield return DialogManager.Instance.ShowDialogText($"But it cannot learn more than {PokemonBase.MaxNumMoves} moves.");
            yield return ChooseMoveToForget(pokemon, item.Move);
            // State only used for this in specific so I don't have to change to any other that has an actual use
            yield return new WaitUntil(() => currentState == InventoryUIState.ForgetDone);
        }

    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        currentState = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText("Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        currentState = InventoryUIState.MoveToForget;
    }

    IEnumerator ItemSelected()
    {
        currentState = InventoryUIState.Busy;

        var item = inventory.GetItem(currSelectedonCategory[categorySelected], categorySelected);

        if (item == null)
        {
            currentState = InventoryUIState.ItemSelection;
            yield break;
        }

        if (GameController.Instance.State == GameState.Battle)
        {
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText("This item can't be used in battle.");
                currentState = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {

            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText("This item can't be used outside battle.");
                currentState = InventoryUIState.ItemSelection;
                yield break;
            }

        }

        if (categorySelected == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartySelection();

            if (item is TmHmItem tmHmItem)
                partyScreen.ShowIfTmUsable(tmHmItem);
        }
    }

    IEnumerator FadeInInventory(Fader fader)
    {

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);
    }

    IEnumerator FadeOutInventory(Fader fader)
    {
        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);
    }


    void OpenPartySelection()
    {
        partyScreen.ResetPartySelected();
        //var fader = FindObjectOfType<Fader>();
        //yield return FadeInInventory(fader);
        partyScreen.gameObject.SetActive(true);
        //yield return FadeOutInventory(fader);
        currentState = InventoryUIState.PartySelection;
    }

    void ClosePartySelection()
    {
        //var fader = FindObjectOfType<Fader>();
        //yield return FadeInInventory(fader);
        partyScreen.gameObject.SetActive(false);
        //yield return FadeOutInventory(fader);
        partyScreen.ResetPartySelected();
        partyScreen.ClearMemberSlotMessages();
        currentState = InventoryUIState.ItemSelection;
    }

    void UpdateInventorySelection()
    {

        var slots = inventory.GetSlotByCategory(categorySelected);

        currSelectedonCategory[categorySelected] = Mathf.Clamp(currSelectedonCategory[categorySelected], 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {

            if (i == currSelectedonCategory[categorySelected])
            {
                slotUIList[i].NameText.color = GlobalSettings.Instance.HighlightedColor;
                slotUIList[i].CountText.color = GlobalSettings.Instance.HighlightedColor;
            }
            else
            {
                slotUIList[i].NameText.color = Color.black;
                slotUIList[i].CountText.color = Color.black;
            }
        }

        if (slots.Count > 0)
        {
            var item = slots[currSelectedonCategory[categorySelected]].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;

            if (slotUIList.Count > itemsInViewport)
                HandleScrolling();
        }
    }

    void HandleScrolling()
    {
        float scrollPos = Mathf.Clamp(currSelectedonCategory[categorySelected] - itemsInViewport / 2, 0, currSelectedonCategory[categorySelected]) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = currSelectedonCategory[categorySelected] > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = currSelectedonCategory[categorySelected] + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);

    }
}
