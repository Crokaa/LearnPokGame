using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBase move;

    public override string Description { get { return move.Description; } }
    public MoveBase Move { get { return move; } }

    public override bool Use(Pokemon target)
    {
        return target.HasMove(move);
    }
}
