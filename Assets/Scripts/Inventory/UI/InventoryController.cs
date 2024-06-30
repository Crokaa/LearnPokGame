using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

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

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    int[] currSelectedonCategory;
    //int currentSelected = 0;
    int categorySelected = 0;
    RectTransform itemListRect;
    const int itemsInViewport = 8;
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
                ItemSelected();

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
                    StartCoroutine(UseItem());
                };

            Action goBackPartyScreen = () =>
            {
                ClosePartySelection();
            };

            partyScreen.HandleUpdate(onSelected, goBackPartyScreen);
        }
    }

    private void ResetSelection()
    {
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemDescription.text = "";
        itemIcon.sprite = null;
    }

    private IEnumerator UseItem()
    {
        currentState = InventoryUIState.Busy;

        var item = inventory.UseItem(currSelectedonCategory[categorySelected], partyScreen.SelectedPokemon, categorySelected);
        if (item != null)
        {

            if (item is not PokeballItem)
                yield return DialogManager.Instance.ShowDialogText($"{item.OnShowMessage}");

            onItemUsed?.Invoke(item);
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText("It won't have any effect.");
        }

        ClosePartySelection();

    }

    private void ItemSelected()
    {
        if (categorySelected == (int)ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartySelection();
        }
    }

    private void OpenPartySelection()
    {
        currentState = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    private void ClosePartySelection()
    {
        currentState = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
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

    private void HandleScrolling()
    {
        float scrollPos = Mathf.Clamp(currSelectedonCategory[categorySelected] - itemsInViewport / 2, 0, currSelectedonCategory[categorySelected]) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = currSelectedonCategory[categorySelected] > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = currSelectedonCategory[categorySelected] + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);

    }
}
