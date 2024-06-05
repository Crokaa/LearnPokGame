using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Pokeball Item")]
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier;

    public override bool Use(Pokemon target)
    {
        return true;
    }
}
