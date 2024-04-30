using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    public string Name { get; set; }
    public string Description { get; set; }
    public AbilityID Id { get; set; }
    public bool NegateWeather { get; set; }
    public Func<float, float> OnApplyStab { get; set; }
    public Func<Move, PokemonType> ChangeType { get; set; }
    public Func<Move, float> MoveBoost { get; set; }
    public Action<Move, Pokemon, Pokemon> OnDamaged { get; set; }
    public Action<Pokemon, Pokemon> OnSwitchIn {get; set;}
    public Action<Pokemon> OnCriticalReceive { get;  set; }
    public Func<Pokemon, bool> CanRun { get;  set; }
    public Func<Move, Pokemon, bool> CanUseMove;
    public Action<Pokemon> OnKnockOut { get;  set; }
}
