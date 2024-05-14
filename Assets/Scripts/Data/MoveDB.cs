using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovesDB
{

    static Dictionary<string, MoveBase> moves;

    public static void Init()
    {

        moves = new Dictionary<string, MoveBase>();

        var moveArray = Resources.LoadAll<MoveBase>("");

        foreach (var move in moveArray)
        {
            //Haven't implemented a bunch of moves so they just keep logging errors
            if (move.Name == "")
                continue;
            if (moves.ContainsKey(move.Name))
            {
                Debug.LogError($"There are 2 moves with the name {move.Name}");
                continue;
            }
            moves.Add(move.Name, move);
        }
    }
    public static MoveBase GetMoveByName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"No move found with the name {name}");
            return null;
        }

        return moves[name];
    }
}