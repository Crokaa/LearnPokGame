using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{

    [SerializeField] List<Pokemon> pokemons;
    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {
            return pokemons;
        }

        set
        {
            pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in pokemons)
        {

            pokemon.Init();
        }
    }

    public Pokemon GetHealthyPokemon()
    {

        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon newPokemon)
    {

        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated();
        }
        else
        {
            //TODO: Transfer to PC
        }
    }

    public IEnumerator CheckForEvolution()
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                yield return EvolutionManager.Instance.Evolve(pokemon, evolution);
            }
        }

        OnUpdated?.Invoke();
    }
}