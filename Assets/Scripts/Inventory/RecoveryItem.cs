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

        // Revive
        if (revive || maxRevive)
        {
            if (target.HP > 0)
                return false;

            if (revive)
                target.HealHP(target.MaxHp / 2);
            else
                target.HealHP(target.MaxHp);


            // I will leave it like this for now, in the future I might delete it because when a pokemon dies all its status leave.
            target.CureStatus();
            target.CureVolatileStatus();

            // This changes the message for the Item class but it's not really important as it will always change based on who and how much I heal
            OnUseMessage = $"{target.Base.Name}'s HP was restored by {target.HP} point(s).";

            return true;
        }

        // For all the conditions below the Pokemon needs to have at least 1HP
        if (target.HP <= 0)
            return false;

        // Restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (target.HP == target.MaxHp)
                return false;

            int beforeHeal = target.HP;

            if (restoreMaxHP)
                target.HealHP(target.MaxHp);
            else
                target.HealHP(hpAmount);


            int afterHeal = target.HP;

            // This changes the message for the Item class but it's not really important as it will always change based on who and how much I heal
            OnUseMessage = $"{target.Base.Name}'s HP was restored by {afterHeal - beforeHeal} point(s).";
        }

        // Restore PP
        if (restoreMaxPP || ppAmount > 0)
        {
            // For now I'll leave it like this even tho it is only for a specifc move and not all of them
            if(restoreMaxPP)
                target.Moves.ForEach(m => m.RecoverPP(m.Base.Pp));
            else
                target.Moves.ForEach(m => m.RecoverPP(ppAmount));
        }


        // Cure status
        if(recoverAllStatus || status != ConditionID.none) 
        {

            if(target.Status == null || target.VolatileStatus == null)
                return false;

            if(recoverAllStatus)
            {
                target.CureStatus();
                target.CureVolatileStatus();
            }

            if(target.Status.Id == status)
                target.CureStatus();
            else if(target.VolatileStatus.Id == status)
                target.CureVolatileStatus();
            else
                return false;

        }

        return true;
    }
}
