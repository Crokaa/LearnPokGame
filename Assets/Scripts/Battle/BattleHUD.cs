using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color parColor;


    Pokemon pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        this.pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>() {

            {ConditionID.psn, psnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.frz, frzColor},
            {ConditionID.brn, brnColor},
            {ConditionID.par, parColor}
        };

        SetStatusText();
        this.pokemon.OnStatusChanged += SetStatusText;
    }

    public void SetStatusText()
    {

        if (pokemon.Status is null)
            statusText.text = "";
        else
        {
            statusText.text = pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[pokemon.Status.Id];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)pokemon.HP / pokemon.MaxHp);
            pokemon.HpChanged = false;
        }
    }
}
