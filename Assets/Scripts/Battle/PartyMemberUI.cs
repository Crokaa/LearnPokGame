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
    [SerializeField] Text messageText;
    Pokemon pokemon;

    public void Init(Pokemon pokemon)
    {
        this.pokemon = pokemon;
        UpdateData();
        SetMessageText("");

        this.pokemon.OnHpChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);

    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.Instance.HighlightedColor;

        else
            nameText.color = Color.black;

    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
