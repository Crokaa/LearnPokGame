using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public static EvolutionManager Instance { get; private set; }
    public Action onStartEvolve;
    public Action onCompleteEvolve;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {


        evolutionUI.SetActive(true);
        pokemonImage.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is evolving.");

        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);


        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name} has evolved into a {pokemon.Base.Name}.");
        evolutionUI.SetActive(false);
    }
}
