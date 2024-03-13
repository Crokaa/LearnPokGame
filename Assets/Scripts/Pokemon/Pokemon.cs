using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonBase pokBase;
    int level;

    public Pokemon(PokemonBase pBase, int pLevel) {
        pokBase = pBase;
        level = pLevel;

    }

    public int Attack {
        get { return Mathf.FloorToInt((pokBase.Attack * level) / 100f) + 5; }
    }

    public int Defense {
        get { return Mathf.FloorToInt((pokBase.Defense * level) / 100f) + 5; }
    }

    public int SpAttack {
        get { return Mathf.FloorToInt((pokBase.SpAttack * level) / 100f) + 5; }
    }

    public int SpDefense {
        get { return Mathf.FloorToInt((pokBase.SpDefense * level) / 100f) + 5; }
    }

    public int Speed {
        get { return Mathf.FloorToInt((pokBase.Speed * level) / 100f) + 5; }
    }

    public int MaxHp {
        get { return Mathf.FloorToInt((pokBase.MaxHp * level) / 100f) + 10; }
    }
}
