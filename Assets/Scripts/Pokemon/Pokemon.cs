using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class Pokemon
{

    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base
    {
        get
        {
            return _base;
        }
    }
    public int Level
    {
        get
        {
            return level;
        }
    }

    public List<Move> Moves { get; set; }
    public int HP { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoost { get; private set; }
    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public Condition Status { get; set; }
    public bool HpChanged { get; set; }

    public void Init()
    {
        //Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.MoveBase));

            if (Moves.Count >= 4)
                break;
        }
        
        CalculateStats();

        HP = MaxHp;
        ResetStatBoost();
    }

    void CalculateStats()
    {

        Stats = new Dictionary<Stat, int>
        {
            { Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5 },
            { Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5 },
            { Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5 }
        };

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
    }

    public void ResetStatBoost()
    {

        StatBoost = new Dictionary<Stat, int>
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 }
        };
    }

    public int GetStat(Stat stat)
    {

        int statVal = Stats[stat];

        int boost = StatBoost[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        else
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoost(List<StatBoost> statBoosts)
    {

        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoost[stat] = Mathf.Clamp(StatBoost[stat] + boost, -6, 6);

            switch (boost)
            {
                case 1:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
                    break;
                case > 1:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} sharply rose!");
                    break;
                case -1:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
                    break;
                case < -1:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} harshly fell!");
                    break;
            }
        }
    }

    public void SetStatus(ConditionID statusID)
    {

        Status = ConditionsDB.Conditions[statusID];
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }

    public void OnAfterTurn() {
        Status?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp { get; set; }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {

        float critical = 1f;
        if (Random.value * 100f <= 4.17)
            critical = 1.5f;

        float effectiveness = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Effectiveness = effectiveness,
            Critical = critical,
            Fainted = false
        };

        float attack = move.Base.Category == MoveCategory.Special ? attacker.SpAttack : attacker.Attack;
        float defense = move.Base.Category == MoveCategory.Special ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * effectiveness * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);
        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        Debug.Log($"previous HP - {HP}");
        HP = HP - damage < 0 ? 0 : HP - damage;
        HpChanged = true;
        Debug.Log($"After burn HP - {HP}");
    }

    public Move GetRandomMove()
    {

        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

}

public class DamageDetails
{

    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Effectiveness { get; set; }
}
