using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{

    public string Name { get; set; }
    public string Description { get; set; }
    public AbilityID Id { get; set; }
    public bool NegateWeather { get; set; } = false;
    public Func<float, float> OnApplyStab { get; set; }
    public Func<Move, PokemonType> ChangeType { get; set; }
    public Func<Move, Pokemon, float> MoveBoost { get; set; }
    public Action<Move, Pokemon, Pokemon> OnDamaged { get; set; }
    public Action<Pokemon, Pokemon> OnSwitchIn { get; set; }
    public Action<Pokemon> OnCriticalReceive { get; set; }
    public Func<Pokemon, bool> CanRun { get; set; }
    public Func<Move, Pokemon, Pokemon, bool> CanUseMove { get; set; }
    public Action<Pokemon, Pokemon> OnTurnEnd { get; set; }
    public Action<Pokemon, Pokemon> OnDropHalf { get;  set; }
    public Action<Dictionary<Stat, int>, Pokemon, Pokemon> OnBoost { get;  set; }
    //This will be used on both attacker and defender. The defender will win because it will be calculated after the attacker.
    public Func<float> CalculateCritical { get; set; }
     // Might not be used (I have OnDamaged already to compensate) On Knock out might be used if status conditions count on any ability
    //public Action<Pokemon> OnKnockOut { get; set; }

    // All of these are used because of Battle Bond, but this way I can create new Abilities that act the same way
    public Action BattleEnded { get; set; }
    

    static bool activated = false;

    public static void Activate()
    {
        activated = true;
    }

    public static void EndBattleActivate()
    {
        activated = false;
    }

    public static bool Activated()
    {
        return activated;
    }

}
