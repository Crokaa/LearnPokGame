using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] GameObject expBar;


    Pokemon pokemon;
    Dictionary<ConditionID, Color> statusColors;

    public void SetData(Pokemon pokemon)
    {
        this.pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        SetExp();

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

    public void SetLevel() {

        levelText.text = "Lvl " + pokemon.Level;
    }

    public IEnumerator UpdateHP()
    {
        if (pokemon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)pokemon.HP / pokemon.MaxHp);
            pokemon.HpChanged = false;
        }
    }

    public void SetExp()
    {

        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {

        if (expBar == null) yield break;

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {

        int currentLevelExp = pokemon.Base.GetExpForLevel(pokemon.Level);
        int nextLevelExp = pokemon.Base.GetExpForLevel(pokemon.Level + 1);

        return Mathf.Clamp01((float) (pokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp));
    }
}
