using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;


    Character character;
    float idleTimer = 0;
    NPCState state;
    int currentMovePattern = 0;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);
            yield return DialogManager.Instance.ShowDialog(dialog);

            idleTimer = 0;
            state = NPCState.Idle;

        }
    }

    public void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentMovePattern]);

        if(transform.position != oldPos)
            currentMovePattern = (currentMovePattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }
}

public enum NPCState
{
    Idle,
    Walking,
    Dialog

}
