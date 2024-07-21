using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Pokeball Item")]
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier;

    public float CatchRateModifier { get { return catchRateModifier; } }
    public override bool CanUseOutsideBattle { get { return false; } }

    public override bool Use(Pokemon target)
    {
        return true;
    }
}
