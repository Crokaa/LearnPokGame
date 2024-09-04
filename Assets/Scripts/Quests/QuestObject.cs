using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase questToCheck;
    [SerializeField] QuestObjectAction onStart;
    [SerializeField] QuestObjectAction onComplete;

    QuestList questList;

    public void Start()

    {
        questList = QuestList.GetQuestList();

        questList.OnUpdated += UpdateObjectStatus;

        UpdateObjectStatus();
    }

    public void OnDestroy()
    {
        questList.OnUpdated -= UpdateObjectStatus;
    }


    public void UpdateObjectStatus()
    {
        if (onStart != QuestObjectAction.DoNothing && questList.IsStarted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onStart == QuestObjectAction.Enable)
                    child.gameObject.SetActive(true);
                else if (onStart == QuestObjectAction.Disable)
                    child.gameObject.SetActive(false);
            }
        }

        Debug.Log("estou aqui amigo");

        if (onComplete != QuestObjectAction.DoNothing && questList.IsCompleted(questToCheck.Name))
        {
            Debug.Log("fora do for amigo");
            foreach (Transform child in transform)
            {
                Debug.Log("dentro do for amigo");
                if (onComplete == QuestObjectAction.Enable)
                    child.gameObject.SetActive(true);
                else if (onComplete == QuestObjectAction.Disable)
                    child.gameObject.SetActive(false);
            }
        }

    }


}

public enum QuestObjectAction { DoNothing, Enable, Disable }
