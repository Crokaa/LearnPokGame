using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Pokemon pokemon;
    [SerializeField] Dialog dialog;

    bool given = false;


    public IEnumerator GivePokemon(PlayerController player)
    {

        yield return DialogManager.Instance.ShowDialog(dialog);

        pokemon.Init();

        player.GetComponent<PokemonParty>().AddPokemon(pokemon);

        given = true;

        string pokemonText = $"{player.Name} has received a {pokemon.Base.Name}.";

        yield return DialogManager.Instance.ShowDialogText(pokemonText);

    }


    public bool CanGive()
    {
        return !given && pokemon != null;
    }

    public object CaptureState()
    {
        return given;
    }

    public void RestoreState(object state)
    {
        given = (bool) state;
    }
}
