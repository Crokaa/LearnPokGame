using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public enum ItemCategory { Items, Pokeballs, TmHm, Berries, KeyItems }

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballs;
    [SerializeField] List<ItemSlot> tmshms;
    [SerializeField] List<ItemSlot> berries;
    [SerializeField] List<ItemSlot> keyitems;
    public event Action OnUpdated;
    public List<List<ItemSlot>> allSlots;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { slots, pokeballs, tmshms, berries, keyitems };
    }

    public static List<string> ItemCategories = new List<string> {
        "ITEMS", "POKÉBALLS", "TMs & HMs", "BERRIES", "KEY ITEMS"
        };

    public List<ItemSlot> GetSlotByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlots = GetSlotByCategory(categoryIndex);

        if (currentSlots.Count == 0)
            return null;

        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon pokemon, int currentCategory)
    {

        var item = GetItem(itemIndex, currentCategory);
        bool used = item.Use(pokemon);
        if (used)
        {
            if (!item.IsReusable)
                RemoveItem(item);
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {

        var categoryIndex = (int)GetItemCategory(item);

        var currSlots = GetSlotByCategory(categoryIndex);
        var itemSlot = currSlots.FirstOrDefault(itemSlot => itemSlot.Item == item);

        if (itemSlot != null)
            itemSlot.Count += count;
        
        else
            currSlots.Add(new ItemSlot(item, count));

        OnUpdated?.Invoke();
    }

    public ItemCategory GetItemCategory(ItemBase item)
    {
        if (item is RecoveryItem)
            return ItemCategory.Items;
        else if (item is PokeballItem)
            return ItemCategory.Pokeballs;
        else if (item is TmHmItem)
            return ItemCategory.TmHm;
        else if (item is BerryItem)
            return ItemCategory.Berries;
        else
            return ItemCategory.KeyItems;

    }

    public void RemoveItem(ItemBase item)
    {
        var currentCategory = (int)GetItemCategory(item);

        var currentSlots = GetSlotByCategory(currentCategory);
        var itemSlot = currentSlots.First(slot => slot.Item == item);

        itemSlot.Count--;
        if (itemSlot.Count == 0)
            currentSlots.Remove(itemSlot);

        OnUpdated?.Invoke();

    }

    public bool HasItem(ItemBase item)
    {
        var currentCategory = (int)GetItemCategory(item);

        var currentSlots = GetSlotByCategory(currentCategory);

        return currentSlots.Exists(slots => slots.Item == item); 
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData
        {
            slots = this.slots.Select(i => i.GetSaveItemSlot()).ToList(),
            pokeballs = this.pokeballs.Select(i => i.GetSaveItemSlot()).ToList(),
            tmshms = this.tmshms.Select(i => i.GetSaveItemSlot()).ToList(),
            berries = this.berries.Select(i => i.GetSaveItemSlot()).ToList(),
            keyitems = this.keyitems.Select(i => i.GetSaveItemSlot()).ToList(),
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (InventorySaveData)state;

        this.slots = saveData.slots.Select(i => new ItemSlot(i)).ToList();
        this.pokeballs = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        this.tmshms = saveData.tmshms.Select(i => new ItemSlot(i)).ToList();
        this.berries = saveData.berries.Select(i => new ItemSlot(i)).ToList();
        this.keyitems = saveData.keyitems.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>>() { slots, pokeballs, tmshms, berries, keyitems };

        OnUpdated?.Invoke();
    }

}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item { get { return item; } set { item = value; } }
    public int Count { get { return count; } set { count = value; } }

    public SaveItemSlot GetSaveItemSlot()
    {
        var saveData = new SaveItemSlot
        {
            name = item.name,
            count = Count
        };

        return saveData;
    }

    public ItemSlot(ItemBase item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public ItemSlot(SaveItemSlot saveData)
    {
        this.item = ItemDB.GetObjectByName(saveData.name);
        this.count = saveData.count;
    }

}

[Serializable]
public class SaveItemSlot
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<SaveItemSlot> slots;
    public List<SaveItemSlot> pokeballs;
    public List<SaveItemSlot> tmshms;
    public List<SaveItemSlot> berries;
    public List<SaveItemSlot> keyitems;
}
