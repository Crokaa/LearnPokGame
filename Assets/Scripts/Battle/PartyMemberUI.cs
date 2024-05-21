using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    Color colorHightlighted;

    Pokemon pokemon;

    private void Start() {
        colorHightlighted = GlobalSettings.Instance.HighlightedColor;
    }

    public void SetData(Pokemon pokemon)
    {
        this.pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = colorHightlighted;

        else
            nameText.color = Color.black;

    }
}
