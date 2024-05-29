using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] string onUseMessage;

    public string Name { get { return name; } }
    public string Description { get { return description; } }
    public Sprite Icon { get { return icon; } }
    public string OnUseMessage { get { return onUseMessage; } protected set { onUseMessage = value; } }

    public virtual bool Use(Pokemon target) {
        return false;
    }
}
