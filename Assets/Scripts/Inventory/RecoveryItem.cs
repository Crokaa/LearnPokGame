using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Recovery Item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon target)
    {

        if (hpAmount > 0)
        {
            if (target.HP == target.MaxHp)
                return false;

            int beforeHeal = target.HP;
            target.HealHP(hpAmount);
            int afterHeal = target.HP;
            // This changes the message for the Item class but it's not really important as it will always change based on who and how much I heal
            OnUseMessage = $"{target.Base.Name}'s HP was restored by {afterHeal - beforeHeal} point(s).";
            return true;
        }

        return false;
    }
}
