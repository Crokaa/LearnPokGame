using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmHmItem : ItemBase
{
    [SerializeField] MoveBase move;
    [SerializeField] bool isHM;

    public override string Name { get { return base.Name + $" - {move.Name}"; } }
    public override string Description { get { return move.Description; } }
    public MoveBase Move { get { return move; } }
    public bool IsHM { get { return isHM; } }
    public override bool IsReusable => IsHM;
    public override bool CanUseInBattle { get { return false; } }

    public override bool Use(Pokemon target)
    {
        return target.HasMove(move);
    }

    public bool CanBeTaught(Pokemon pokemon)
    {
        return pokemon.Base.LearnByTm.Contains(move);
    }
}
