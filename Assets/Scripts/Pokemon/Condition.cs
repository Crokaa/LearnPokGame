using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition : MonoBehaviour
{
    
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public ConditionID Id { get; set; }

    public Func<Pokemon, bool> OnBeforeTurn { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
    public Action<Pokemon> OnStart { get; set; }
    
}
