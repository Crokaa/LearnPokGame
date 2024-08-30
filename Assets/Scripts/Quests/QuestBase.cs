using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    
    [SerializeField] string name;
    [SerializeField] string description;

    [SerializeField] Dialog startDialog;
    [SerializeField] Dialog inProgresstDialog;
    [SerializeField] Dialog completedDialog;

    [SerializeField] ItemBase requestItem;
    [SerializeField] ItemBase rewardItem;

    public String Name { get { return name; } }
    public string Description { get { return description; } }
    public Dialog StartDialog { get { return startDialog; } }
    public Dialog InProgresstDialog { get { return inProgresstDialog; } }
    public Dialog CompletedDialog { get { return completedDialog; } }
    public ItemBase RequestItem { get { return requestItem; } }
    public ItemBase RewardItem { get { return rewardItem; } }
}
