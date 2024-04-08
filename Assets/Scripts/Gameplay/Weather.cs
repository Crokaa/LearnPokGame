using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public string RoundMessage { get; set; }
    public Func<Move, float> BeforeMove { get; set; }
    public Func<Move, float> DuringMove { get; set; }
    public Func<Pokemon, int> ResistOnWeather { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }

}
