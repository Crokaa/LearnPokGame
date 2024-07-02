using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] protected string onUseMessage;
    string onShowMessage;

    public string Name { get { return name; } }
    public virtual string Description { get { return description; } }
    public Sprite Icon { get { return icon; } }
    public string OnShowMessage { get { return onShowMessage; } protected set { onShowMessage = value; } }

    public virtual bool Use(Pokemon target)
    {
        return false;
    }
}
