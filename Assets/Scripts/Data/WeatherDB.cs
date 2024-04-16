using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Build;
using UnityEngine;

public class WeatherDB : MonoBehaviour
{

    internal static void Init()
    {
        foreach (var kvp in Weathers)
        {

            var weatherID = kvp.Key;
            var weather = kvp.Value;

            weather.Id = weatherID;
        }
    }

    public static Dictionary<WeatherID, Weather> Weathers { get; set; } = new Dictionary<WeatherID, Weather>{

        {
            WeatherID.sun,
            new Weather ()
                {
                    Name = "Harsh sunlight",
                    NaturalStartMessage = "The sunlight is harsh!",
                    StartMessage = "The sunlight turned harsh!",
                    //This might be deleted or not depending on how the game goes
                    RoundMessage = "The sunlight is strong.",
                    LeaveMessage = "The harsh sunlight faded.",
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
                    NaturalStartMessage = "It's raining!",
                    StartMessage = "It started to rain!",
                    //This might be deleted or not depending on how the game goes
                    RoundMessage = "Rain continues to fall.",
                    LeaveMessage = "The rain stopped.",
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
                    NaturalStartMessage = "The sandstorm is raging!",
                    StartMessage = "A sandstorm kicked up!",
                    //This might be deleted or not depending on how the game goes
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
                        {
                            pokemon.UpdateHP(pokemon.MaxHp / 16 == 0 ? 1 : pokemon.MaxHp / 16);
                            pokemon.WeatherDamages.Enqueue($"{pokemon.Base.Name} is buffeted by the hail!");

                        }
                }

            }
        },
        {
            WeatherID.hail,
            new Weather ()
                {
                    Name = "Hail",
                    NaturalStartMessage = "It's hailing!",
                    StartMessage = "It started to hail!",
                    RoundMessage = "Hail continues to fall.",
                    LeaveMessage = "The hail stopped.",
                    OnAfterTurn = (Pokemon pokemon) => {

                    var type1 = pokemon.Base.Type1;
                    var type2 = pokemon.Base.Type2;

                        if (type1 != PokemonType.Ice || type2 != PokemonType.Ice)
                        {
                            pokemon.UpdateHP(pokemon.MaxHp / 16 == 0 ? 1 : pokemon.MaxHp / 16);
                            pokemon.WeatherDamages.Enqueue($"{pokemon.Base.Name} is buffeted by the hail!");
                        }
                    }

            }
        },
        {
            WeatherID.fog,
            new Weather ()
                {
                    Name = "Fog",
                    NaturalStartMessage = "The fog is deep..."
            }
        },
        {
            WeatherID.exsun,
            new Weather ()
                {
                    Name = "Extremely Harsh Sunlight",
                    StartMessage = "The sunlight turned extremely harsh!",
                    PreventWeatherMessage = "The extremely harsh sunlight was not lessened at all!",
                    MoveEffectivenessMessage = "The Water-type attack evaporated in the harsh sunlight!",
                    LeaveMessage = "The harsh sunlight faded.",
                    // I use this method because in take damage this applies an effectivenessChange that will show a message later
                    ChangeEffectiveness = (Move move, Pokemon target) => {
                        if (move.Base.Type == PokemonType.Fire)
                            return 1.5f ;
                        else if (move.Base.Type == PokemonType.Water)
                            return 0.0f;

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
                    PreventWeatherMessage = "There is no relief from this heavy rain!",
                    MoveEffectivenessMessage = "The Fire-type attack fizzled out in the heavy rain!",
                    LeaveMessage = "The heavy rain has lifted!",
                    // I use this method because in take damage this applies an effectivenessChange that will show a message later
                    ChangeEffectiveness = (Move move, Pokemon target) => {
                        if (move.Base.Type == PokemonType.Water)
                            return 1.5f ;
                        else if (move.Base.Type == PokemonType.Fire)
                            return 0.0f;

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
                    PreventWeatherMessage = "The mysterious strong winds blow on regardless!",
                    MoveEffectivenessMessage = "The mysterious strong winds weakened the attack!",
                    LeaveMessage = "The mysterious strong winds have dissipated!",
                    // This method makes sense here as the target needs to be a flying type.
                    ChangeEffectiveness = (Move move, Pokemon target) => {

                        var type1 = target.Base.Type1;
                        var type2 = target.Base.Type2;
                        if ((move.Base.Type == PokemonType.Electric || move.Base.Type == PokemonType.Ice || move.Base.Type == PokemonType.Rock) && (type1 == PokemonType.Flying || type2 == PokemonType.Flying))
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
