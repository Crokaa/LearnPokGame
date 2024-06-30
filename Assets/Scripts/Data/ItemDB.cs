using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemDB
{

    static Dictionary<string, ItemBase> items;

    public static void Init()
    {

        items = new Dictionary<string, ItemBase>();

        var itemArray = Resources.LoadAll<ItemBase>("");

        foreach (var item in itemArray)
        {
            if (items.ContainsKey(item.Name))
            {
                Debug.LogError($"There are 2 items with the name {item.Name}");
                continue;
            }
            items.Add(item.Name, item);
        }
    }
    public static ItemBase GetItemByName(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.LogError($"No item found with the name {name}");
            return null;
        }

        return items[name];
    }
}