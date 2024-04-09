using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Build;
using UnityEngine;

public class WeatherDB : MonoBehaviour
{

    public static Dictionary<WeatherID, Weather> Weathers { get; set; } = new Dictionary<WeatherID, Weather>{

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
        {
            WeatherID.hail,
            new Weather ()
                {
                    Name = "Hail",
                    StartMessage = "It started to hail!",
                    RoundMessage = "It's hailing!",
                    OnAfterTurn = (Pokemon pokemon) => {

                    var type1 = pokemon.Base.Type1;
                    var type2 = pokemon.Base.Type2;

                        if (type1 != PokemonType.Ice || type2 != PokemonType.Ice)
                            pokemon.UpdateHP(pokemon.MaxHp / 16);
                    }

            }
        },
        {
            WeatherID.fog,
            new Weather ()
                {
                    Name = "Fog",
                    RoundMessage = "The fog is deep..."
            }
        },
        {
            WeatherID.exsun,
            new Weather ()
                {
                    Name = "Extremely Harsh Sunlight",
                    StartMessage = "The sunlight turned extremely harsh!",
                    DuringMove = (Move move) => {
                        if (move.Base.Type == PokemonType.Fire)
                            return 1.5f ;
                        else if (move.Base.Type == PokemonType.Water)
                            return 0.0f; // this will need some change as it will show a message

                        return 1f;
                    }
            }
        },
        {
            WeatherID.heavyrain,
            new Weather ()
                {
                    Name = "Heavy Rain",
                    StartMessage = "A heavy rain began to fall!",
                    DuringMove = (Move move) => {
                        if (move.Base.Type == PokemonType.Water)
                            return 1.5f ;
                        else if (move.Base.Type == PokemonType.Fire)
                            return 0.0f; // this will need some change as it will show a message

                        return 1f;
                    }
            }
        },
        {
            WeatherID.strongwind,
            new Weather ()
                {
                    Name = "Strong winds",
                    StartMessage = "Mysterious strong winds are protecting Flying-type Pokémon!",
                    //this method will need a change for sure. I'll just leave it like this and remember to change it later on. 0.5f will be 1 and I'll receive the pokemon defending
                    DuringMove = (Move move) => {
                        if (move.Base.Type == PokemonType.Electric || move.Base.Type == PokemonType.Ice || move.Base.Type == PokemonType.Rock)
                            return 0.5f ;

                        return 1f;
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
