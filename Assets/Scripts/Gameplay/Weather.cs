using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string NaturalStartMessage { get; set; }
    public string StartMessage { get; set; }
    public string RoundMessage { get; set; }
    public string LeaveMessage { get; set; }
    public string PreventWeatherMessage { get; set; }
    public WeatherID Id;
    public Func<Move, Pokemon, float> OnModifyDamage { get; set; }
    public Func<Move, Pokemon, string> ChangeEffectivenessMessage { get; set; }
    public Func<Pokemon, int> ResistOnWeather { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }

}
