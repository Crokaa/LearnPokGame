using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{

    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    int currentSelected;
    RectTransform itemListRect;
    const int itemsInViewport = 8;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
    }

    void UpdateItemList()
    {

        slotUIList = new List<ItemSlotUI>();
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var itemSlot in inventory.Slots)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        currentSelected = 0;
        UpdateInventorySelection();
    }
    public void HandleUpdate(Action goBack)
    {
        int prevSelected = currentSelected;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentSelected++;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentSelected--;

        currentSelected = Mathf.Clamp(currentSelected, 0, inventory.Slots.Count-1);
        
        if (prevSelected != currentSelected)
            UpdateInventorySelection();

        if (Input.GetKeyDown(KeyCode.X))
        {
            goBack?.Invoke();
        }
    }

    void UpdateInventorySelection()
    {

        for (int i = 0; i < slotUIList.Count; i++)
        {

            if (i == currentSelected)
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

        var item = inventory.Slots[currentSelected].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text = item.Description;

        HandleScrolling();

    }

    private void HandleScrolling()
    {
        float scrollPos = Mathf.Clamp(currentSelected - itemsInViewport/2, 0, currentSelected) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);


        bool showUpArrow = currentSelected > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = currentSelected + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);

    }
}
