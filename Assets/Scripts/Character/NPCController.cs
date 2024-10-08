using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;


    Character character;
    float idleTimer = 0;
    NPCState state;
    int currentMovePattern = 0;
    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    Quest activeQuest;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if (questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(initiator);
                questToComplete = null;

                Debug.Log($"{quest.Base.Name} completed");
            }

            if (itemGiver != null && itemGiver.CanGive())
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            else if (pokemonGiver != null && pokemonGiver.CanGive())
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;
            }
            else if (activeQuest != null)
            {
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgresstDialog);
            }
            else
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

        if (transform.position != oldPos)
            currentMovePattern = (currentMovePattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCSaveData();

        if(activeQuest != null)
            saveData.activeQuest = activeQuest.GetQuestSaveData();

        if (questToStart != null)
            saveData.questToStart = new Quest(questToStart).GetQuestSaveData();

        if (questToComplete != null)
            saveData.questToComplete = new Quest(questToComplete).GetQuestSaveData();


        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCSaveData;

        activeQuest = saveData.activeQuest == null ? null : new Quest(saveData.activeQuest);

        questToStart = saveData.questToStart == null ? null : new Quest(saveData.questToStart).Base;
        questToComplete = saveData.questToComplete == null ? null : new Quest(saveData.questToComplete).Base;

    }
}

[System.Serializable]
public class NPCSaveData
{
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;

    public QuestSaveData activeQuest;
}

public enum NPCState
{
    Idle,
    Walking,
    Dialog

}
