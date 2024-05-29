using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    public List<ItemSlot> Slots { get { return slots; } }
    public event Action OnUpdated;

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public ItemBase UseItem(int itemIndex, Pokemon pokemon)
    {
        var item = slots[itemIndex].Item;
        bool used = item.Use(pokemon);
        if (used)
        {
            RemoveItem(item);
            return item;
        }

        return null;
    }

    private void RemoveItem(ItemBase item)
    {
        var itemSlot = slots.First(slot => slot.Item == item);

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
