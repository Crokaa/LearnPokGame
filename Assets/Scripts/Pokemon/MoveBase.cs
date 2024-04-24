using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create a new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] MoveTarget target;
    [SerializeField] List<SecondaryEffects> secEffects;
    [SerializeField] Vector2Int hitRange;


    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public PokemonType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    public int Pp
    {
        get { return pp; }
    }

    public int Priority
    {
        get { return priority; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects
    {
        get { return effects; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }

    public List<SecondaryEffects> SecEffects
    {
        get { return secEffects; }
    }

    // This will suffer some changes probably due to some moves who get accuracy check every turn.
    public int GetHitTimes()
    {
        if (hitRange.x == 0)
            return 1;

        if (hitRange.y == 0)
            return hitRange.x;

        int r = Random.Range(1, 101);

        int hitCount = 1;
        switch (r)
        {
            case <= 35:
                hitCount = hitRange.x;
                break;
            case <= 70:
                hitCount = hitRange.x + 1;
                break;
            case <= 85:
                hitCount = hitRange.x + 2;
                break;
            case <= 100:
                hitCount = hitRange.y;
                break;
        }
        return hitCount;
    }
}

[System.Serializable]
public class MoveEffects
{

    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] WeatherID weather;

    public List<StatBoost> Boosts
    {

        get
        {
            return boosts;
        }
    }

    public ConditionID Status
    {
        get
        {
            return status;
        }
    }

    public ConditionID VolatileStatus
    {
        get
        {
            return volatileStatus;
        }
    }
    public WeatherID Weather
    {
        get
        {
            return weather;
        }
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{

    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance
    {
        get
        {
            return chance;
        }
    }

    public MoveTarget Target
    {
        get
        {
            return target;
        }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

public enum MoveTarget
{
    Foe,
    Self
}