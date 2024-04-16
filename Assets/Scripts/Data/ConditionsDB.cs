using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ConditionsDB
{

    public static void Init()
    {

        foreach (var kvp in Conditions)
        {

            var conditionID = kvp.Key;
            var condition = kvp.Value;
            
            condition.Id = conditionID;
        }

    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned.",
                OnAfterTurn = (Pokemon pokemon) =>
                {

                    pokemon.UpdateHP(pokemon.MaxHp / 8 == 0 ? 1 : pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by its poisoning!");
                }
            }
        },

        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned.",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                        pokemon.UpdateHP(pokemon.MaxHp / 16 == 0 ? 1 : pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} was hurt by its burn!");
                }
            }
         },

         {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "is paralyzed, so it may be unable to move!",
                OnBeforeTurn = (Pokemon pokemon) =>
                {

                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} couldn't move because it's paralyzed!");
                        return false;
                    }

                    return true;
                }
            }
         },

         {
            ConditionID.frz,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "was frozen solid!",
                OnBeforeTurn = (Pokemon pokemon) =>
                {

                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is frozen solid!");
                        pokemon.CureStatus();
                        return false;
                    }

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer frozen!");
                    return false;
                }
            }
         },
         {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "fell asleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.StatusStime = Random.Range(1,4);
                }
                ,
                OnBeforeTurn = (Pokemon pokemon) =>
                {

                    if(pokemon.StatusStime <= 0) {

                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} just woke up!");
                        return true;

                    }

                    pokemon.StatusStime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is fast asleep!");
                    return false;
                }
            }
         },

         // Volatile status
         {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "became confused!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                }
                ,
                OnBeforeTurn = (Pokemon pokemon) =>
                {

                    if(pokemon.VolatileStatusTime <= 0) {

                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} snapped out of its confusion!");
                        return true;

                    }

                    pokemon.VolatileStatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused!");

                    if(Random.Range(1,4) == 1)
                        return true;

                    float a = (2 * pokemon.Level + 10) / 250f;
                    int damage = Mathf.FloorToInt(a * 40 * ((float)pokemon.Attack / pokemon.Defense) + 2);

                    pokemon.UpdateHP(damage);
                    pokemon.StatusChanges.Enqueue($"It hurt itself in its confusion!");
                    return false;
                }
            }
         }

    };

    public static float GetStatusBonus(Condition condition)
    {

        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.frz || condition.Id == ConditionID.slp)
            return 2f;
        else if (condition.Id == ConditionID.brn || condition.Id == ConditionID.par || condition.Id == ConditionID.psn)
            return 1.5f;
        else
            return 1f;
    }

}

public enum ConditionID
{
    none,
    psn,
    brn,
    slp,
    par,
    frz,
    confusion
}
