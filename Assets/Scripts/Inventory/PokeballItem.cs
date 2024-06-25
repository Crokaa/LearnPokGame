using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Pokeball Item")]
public class PokeballItem : ItemBase
{
    [SerializeField] float catchRateModifier;

    public float CatchRateModifier { get { return catchRateModifier; } }

    public override bool Use(Pokemon target)
    {
        // later on will be removed as I don't want my item to know the GameController and pokeballs won't even be able to be used
        // just like pokemon games the inventory will control what my item does (use, give, take, cancel)
        if(GameController.Instance.State == GameState.Battle)
            return true;

        return false;
    }
}
