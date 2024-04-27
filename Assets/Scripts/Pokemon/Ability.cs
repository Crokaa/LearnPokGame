using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    

    public string Name { get; set; }
    public string Description { get; set; }
    public Func<float, float> OnApplyStab { get; set; }
    public Func<Move, PokemonType> ChangeType { get; set; }
    public Func<Move, float> MoveBoost { get; set; }
    public AbilityID Id { get; set; }
}
