using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather : MonoBehaviour
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string NaturalStartMessage { get; set; }
    public string StartMessage { get; set; }
    public string RoundMessage { get; set; }
    public string LeaveMessage { get; set; }
    public string PreventWeatherMessage { get; set; }
    public string MoveEffectivenessMessage { get; set; }
    public WeatherID Id;
    public Func<Move, Pokemon, float> ChangeEffectiveness { get; set; }
    public Func<Move, float> DuringMove { get; set; }
    public Func<Pokemon, int> ResistOnWeather { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }

}
