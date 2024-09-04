using System;
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
        Base = MovesDB.GetObjectByName(saveData.name);
        Pp = saveData.pp;
    }

    public MoveSaveData GetMoveSaveData()
    {
        var saveData = new MoveSaveData
        {
            name = Base.name,
            pp = Pp
        };

        return saveData;
    }

    internal void RecoverPP(int ppAmount)
    {
        Pp = Mathf.Clamp(Pp + ppAmount, 0, Base.Pp);
    }
}
[System.Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}
