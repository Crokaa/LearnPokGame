using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
            if (Random.Range(1, 101) <= 10)
            {
                GameController.Instance.StartBattle();
            }
    }
}
