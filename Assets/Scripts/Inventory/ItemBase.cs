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

    public virtual string Name { get { return name; } }
    public virtual string Description { get { return description; } }
    public Sprite Icon { get { return icon; } }
    public string OnShowMessage { get { return onShowMessage; } protected set { onShowMessage = value; } }
    public virtual bool IsReusable { get { return false; } }
    public virtual bool CanUseInBattle { get { return true; } }
    public virtual bool CanUseOutsideBattle { get { return true; } }

    public virtual bool Use(Pokemon target)
    {
        return false;
    }
}
