using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
            player.Character.Animator.IsMoving = true;
            if (Random.Range(1, 101) <= 10)
            {
                player.Character.Animator.IsMoving = false;
                GameController.Instance.StartBattle();
            }
    }
}
