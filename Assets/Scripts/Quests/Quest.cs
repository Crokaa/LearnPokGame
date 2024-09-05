using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{

    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }


    public Quest(QuestBase qBase)
    {
        Base = qBase;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialog);

        QuestList.GetQuestList().AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform player)
    {

        Status = QuestStatus.Completed;

        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialog);


        var inventory = Inventory.GetInventory();

        if (Base.RequestItem != null)
            inventory.RemoveItem(Base.RequestItem);


        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);
            var playerName = player.GetComponent<PlayerController>().Name;
            yield return DialogManager.Instance.ShowDialogText($"{playerName} received {Base.RewardItem.Name.ToUpper()}.", false, false);
            yield return new WaitForSeconds(1f);
            yield return DialogManager.Instance.ShowDialogText($"{playerName} put away the " + Base.RewardItem.Name.ToUpper() + $" in the {inventory.GetItemCategory(Base.RewardItem).ToString().ToUpper()} POCKET.");
        }
        
        QuestList.GetQuestList().AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if (Base.RequestItem != null)
        {
            if (!inventory.HasItem(Base.RequestItem))
                return false;
        }

        return true;
    }

    public QuestSaveData GetQuestSaveData()
    {

        var saveData = new QuestSaveData
        {
            name = Base.name,
            status = Status
        };

        return saveData;
    }

    public Quest(QuestSaveData questSaveData)
    {
        Base = QuestDB.GetObjectByName(questSaveData.name);
        Status = questSaveData.status;
    }

}

[System.Serializable]
public class QuestSaveData
{

    public string name;
    public QuestStatus status;
}

public enum QuestStatus { None, Started, Completed }
