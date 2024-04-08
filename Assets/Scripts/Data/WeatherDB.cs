using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Build;
using UnityEngine;

public class WeatherDB : MonoBehaviour
{


    public static Dictionary<WeatherID, Weather> Conditions { get; set; } = new Dictionary<WeatherID, Weather>{

        {
            WeatherID.sun,
            new Weather ()
                {
                    Name = "Harsh sunlight",
                    StartMessage = "The sunlight turned harsh!",
                    RoundMessage = "The sunlight is harsh!",
                    DuringMove = (Move move) => {
                        if (move.Base.Type == PokemonType.Fire)
                            return 1.5f ;
                        else if (move.Base.Type == PokemonType.Water)
                            return 0.5f;

                        return 1f;
                    }

            }
        },
        {
            WeatherID.rain,
            new Weather ()
                {
                    Name = "Rain",
                    StartMessage = "It started to rain!",
                    RoundMessage = "It's raining!",
                    DuringMove = (Move move) => {
                        if (move.Base.Type == PokemonType.Fire)
                            return 0.5f ;
                        else if (move.Base.Type == PokemonType.Water)
                            return 1.5f;

                        return 1f;
                    }

            }
        },
        {
            WeatherID.sand,
            new Weather ()
                {
                    Name = "Sandstorm",
                    StartMessage = "A sandstorm kicked up!",
                    RoundMessage = "The sandstorm is raging!",
                    ResistOnWeather = (Pokemon pokemon) => {
                        if (pokemon.Base.Type1 == PokemonType.Rock || pokemon.Base.Type2 == PokemonType.Rock)
                            return Mathf.FloorToInt(pokemon.SpDefense * 1.5f);
                        return pokemon.SpDefense;
                    },
                    OnAfterTurn = (Pokemon pokemon) => {

                    var type1 = pokemon.Base.Type1;
                    var type2 = pokemon.Base.Type2;

                        if (type1 != PokemonType.Ground && type1 != PokemonType.Rock && type1 != PokemonType.Steel
                        && type2 != PokemonType.Ground && type2 != PokemonType.Rock && type2 != PokemonType.Steel)
                            pokemon.UpdateHP(pokemon.MaxHp / 16);
                    }

            }
        },
     };
}


public enum WeatherID
{

    none,
    sun,
    rain,
    sand,
    hail,
    fog,
    exsun,
    heavyrain,
    strongwind
}
