using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEditor.Build;
using UnityEngine;

[System.Serializable]
public class Pokemon
{

    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int level)
    {
        _base = pBase;
        this.level = level;

        Init();
    }

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
    public Move CurrentMove { get; set; }
    public int HP { get; set; }
    public int Exp { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Queue<string> StatusChanges { get; private set; }
    public Queue<string> WeatherDamages { get; private set; }
    public Condition Status { get; set; }
    public Ability Ability { get; set; }
    public int StatusStime { get; set; }
    public Condition VolatileStatus { get; set; }
    public int VolatileStatusTime { get; set; }
    public event Action OnStatusChanged;
    public event Action OnHpChanged;

    public void Init()
    {
        //Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.MoveBase));

            if (Moves.Count >= PokemonBase.MaxNumMoves)
                break;
        }

        Exp = Base.GetExpForLevel(Level);
        CalculateStats();


        StatusChanges = new Queue<string>();
        WeatherDamages = new Queue<string>();
        Ability = AbilitiesDB.Abilities[Base.Ability];

        HP = MaxHp;
        ResetStatBoost();

        Status = null;
        VolatileStatus = null;
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

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + Level + 10;
    }

    public void ResetStatBoost()
    {

        StatBoosts = new Dictionary<Stat, int>
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0},
            { Stat.Evasion, 0 }
        };
    }

    public int GetStat(Stat stat)
    {

        int statVal = Stats[stat];

        int boost = StatBoosts[stat];
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

            var prevBoost = StatBoosts[stat];
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (prevBoost == StatBoosts[stat])
            {
                if (boost < 0)
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} won't go any lower!");
                else
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} won't go any higher!");

                return;
            }

            switch (boost)
            {
                case 1:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
                    break;
                case 2:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} sharply rose!");
                    break;
                case >= 3:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} rose drastically!");
                    break;
                case -1:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
                    break;
                case -2:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} harshly fell!");
                    break;
                case <= -3:
                    StatusChanges.Enqueue($"{Base.Name}'s {stat} severely fell!");
                    break;
            }
        }
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public void SetStatus(ConditionID statusID)
    {
        if (Status is not null) return;
        Status = ConditionsDB.Conditions[statusID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");

        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID statusID)
    {
        if (VolatileStatus is not null) return;
        VolatileStatus = ConditionsDB.Conditions[statusID];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");

    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public bool OnBeforeTurn()
    {
        if (Status?.OnBeforeTurn != null)
        {

            if (!Status.OnBeforeTurn(this))
                return false;
        }
        if (VolatileStatus?.OnBeforeTurn != null)
        {
            if (!VolatileStatus.OnBeforeTurn(this))
                return false;
        }
        return true;
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

    public bool CheckForLevelUp()
    {

        if (Exp > Base.GetExpForLevel(level + 1))
        {
            level++;
            return true;
        }

        return false;
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

    public DamageDetails TakeDamage(Move move, Pokemon attacker, Weather weather)
    {

        float critical = 1f;
        if (UnityEngine.Random.value * 100f <= 4.17)
            critical = 1.5f;

        float effectiveness = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        float weatherMod = weather?.OnModifyDamage?.Invoke(move, this) ?? 1f;
        string weatherEffect = weather?.ChangeEffectivenessMessage?.Invoke(move, this) ?? null;

        float attack = move.Base.Category == MoveCategory.Special ? attacker.SpAttack : attacker.Attack;
        float defense = move.Base.Category == MoveCategory.Special ? SpDefense : Defense;

        float stab = attacker.Base.Type1 == move.Base.Type || attacker.Base.Type2 == move.Base.Type ? 1.5f : 1f;
        //stab = attacker.Ability.OnApplyStab?.Invoke(stab) ?? stab;

        float modifiers = UnityEngine.Random.Range(85, 100) / 100.0f * effectiveness * critical * weatherMod;

        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers) == 0 ? 1 : Mathf.FloorToInt(d * modifiers);

        var damageDetails = new DamageDetails()
        {
            Damage = damage,
            Effectiveness = weatherEffect == null ? effectiveness : effectiveness * weatherMod,
            WeatherEffectMessage = weatherEffect,
            Critical = critical,
            Fainted = false
        };

        UpdateHP(damage);
        return damageDetails;
    }

    public List<LearnableMove> GetLearnableMovesAtCurrentLevel()
    {

        return Base.LearnableMoves.Where(x => x.Level == level).ToList();

    }

    public void LearnMove(MoveBase newMove)
    {
        Moves.Add(new Move(newMove));
    }

    public bool HasMove(MoveBase move)
    {
        return Moves.Count(m => m.Base == move) > 0;
    }

    public void UpdateHP(int damage)
    {
        HP = HP - damage < 0 ? 0 : HP - damage;
        if (damage > 0)
        {
            OnHpChanged?.Invoke();
        }
    }

    public void HealHP(int heal)
    {
        if (HP != MaxHp)
        {
            HP = HP + heal <= MaxHp ? HP + heal : MaxHp;
            OnHpChanged?.Invoke();
        }
    }

    // For now only works if enemy has moves with PP, will be fixed later when introducing Struggle
    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.Pp > 0).ToList();

        int r = UnityEngine.Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public void HealHPFromMove(Move move, int damage)
    {
        var heal = damage == 1 ? 1 : Mathf.FloorToInt(damage * (move.Base.HealPercentage / 100f));
        HealHP(heal);
    }

    public Pokemon(PokemonSaveData saveData)
    {

        _base = PokemonDB.GetPokemonByName(saveData.name);
        level = saveData.level;
        HP = saveData.hp;
        Exp = saveData.exp;

        if (saveData.statusID != null)
            Status = ConditionsDB.Conditions[saveData.statusID.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(m => new Move(m)).ToList();

        CalculateStats();

        StatusChanges = new Queue<string>();
        WeatherDamages = new Queue<string>();

        //ability will probably be saved later since pokemons can get different abilities and they WILL be changable
        Ability = AbilitiesDB.Abilities[Base.Ability];
        ResetStatBoost();

        VolatileStatus = null;
    }

    public PokemonSaveData GetPokemonSaveData()
    {

        var saveData = new PokemonSaveData
        {
            name = Base.Name,
            level = Level,
            hp = HP,
            exp = Exp,
            statusID = Status?.Id,
            moves = Moves.Select(m => m.GetMoveSaveData()).ToList()
        };

        return saveData;
    }
}

public class DamageDetails
{

    public int Damage { get; set; }
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Effectiveness { get; set; }
    public string WeatherEffectMessage { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public string name;
    public int level;
    public int hp;
    public int exp;
    public ConditionID? statusID;
    public List<MoveSaveData> moves;
}
