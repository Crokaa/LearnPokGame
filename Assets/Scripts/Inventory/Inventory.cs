using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public enum ItemCategory { Items, Pokeballs, TmHm, Berries, KeyItems }

public class Inventory : MonoBehaviour
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

    public ItemBase UseItem(int itemIndex, Pokemon pokemon, int currentCategory)
    {

        var currentSlots = GetSlotByCategory(currentCategory);

        var item = currentSlots[itemIndex].Item;
        bool used = item.Use(pokemon);
        if (used)
        {
            RemoveItem(item, currentCategory);
            return item;
        }

        return null;
    }

    private void RemoveItem(ItemBase item, int currentCategory)
    {

        var currentSlots = GetSlotByCategory(currentCategory);
        var itemSlot = currentSlots.First(slot => slot.Item == item);

        itemSlot.Count--;
        if (itemSlot.Count == 0)
            slots.Remove(itemSlot);

        OnUpdated?.Invoke();

    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item { get { return item; } }
    public int Count { get { return count; } set { count = value; } }

}
