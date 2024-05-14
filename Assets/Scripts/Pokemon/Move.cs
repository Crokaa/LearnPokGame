using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBase Base { get; set; }
    public int Pp { get; set; }

    public Move(MoveBase mBase)
    {
        Base = mBase;
        Pp = mBase.Pp;
    }

    public Move(MoveSaveData saveData)
    {
        Base = MovesDB.GetMoveByName(saveData.name);
        Pp = saveData.pp;
    }

    public MoveSaveData GetMoveSaveData()
    {
        var saveData = new MoveSaveData
        {
            name = Base.Name,
            pp = Pp
        };

        return saveData;
    }
}
[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
