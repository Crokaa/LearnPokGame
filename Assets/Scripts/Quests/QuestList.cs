using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour
{

    List<Quest> questList = new List<Quest>();
    public event Action OnUpdated;

    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerController>().GetComponent<QuestList>();
    }

    public void AddQuest(Quest quest)
    {
        if (!questList.Contains(quest))
            questList.Add(quest);

        Debug.Log("Adicionei quest");

        OnUpdated?.Invoke();
    }

    public bool IsStarted(string questName)
    {
        var questStatus = questList.FirstOrDefault(q => q.Base.Name == questName)?.Status;

        return  questStatus == QuestStatus.Started || questStatus == QuestStatus.Completed;
    }

    public bool IsCompleted(string questName)
    {
        return questList.FirstOrDefault(q => q.Base.Name == questName)?.Status == QuestStatus.Completed;
    }

}
