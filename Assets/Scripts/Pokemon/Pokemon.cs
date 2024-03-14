using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonBase pokBase;
    int level;

    public List<Move> Moves { get; set; }
    public int HP { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel) {
        pokBase = pBase;
        level = pLevel;
        HP = pokBase.MaxHp;       

        //Generate Moves
        Moves = new List<Move>();
        foreach(var move in pokBase.LearnableMoves) {
            if (move.Level <= level)
                Moves.Add(new Move(move.MoveBase));

            if (Moves.Count >= 4)
                break;
        }
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
