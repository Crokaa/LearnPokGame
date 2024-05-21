using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    int selected = 0;
    //Party screen can be called from different states like ActionSeelction, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }
    public Pokemon SelectedPokemon { get { return pokemons[selected]; } }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void HandleUpdate(Action onSelected, Action goBack)
    {

        int prevSelected = selected;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selected < pokemons.Count - 1 && selected != 1 && selected != 3)
                ++selected;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selected > 0 && selected != 2 && selected != 4)
                --selected;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selected < pokemons.Count - 2)
                selected += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selected > 1)
                selected -= 2;
        }

        if (prevSelected != selected)
            UpdateMemberSelection(selected);

        if (Input.GetKeyDown(KeyCode.X))
        {
            goBack?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        for (int i = 0; i < memberSlots.Length; i++)
        {

            if (i < pokemons.Count)
            {
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        messageText.text = "Choose a Pokemon.";

        UpdateMemberSelection(selected);
    }

    public void UpdateMemberSelection(int selectedMember)
    {

        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void SetMessageText(string message)
    {

        messageText.text = message;
    }
}
