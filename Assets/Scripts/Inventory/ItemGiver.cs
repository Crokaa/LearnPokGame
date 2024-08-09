using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialog dialog;

    bool given = false;


    public IEnumerator GiveItem(PlayerController player)
    {

        yield return DialogManager.Instance.ShowDialog(dialog);

        var inventory = player.GetComponent<Inventory>();


        inventory.AddItem(item, count);

        given = true;

        string itemsText = $"{player.Name} has received a {item.Name}.";

        if (count > 1)
            itemsText = $"{player.Name} has received {count} {item.Name}s.";

        yield return DialogManager.Instance.ShowDialogText(itemsText, false, false);
        yield return new WaitForSeconds(1f);
        yield return DialogManager.Instance.ShowDialogText($"{player.Name} put away the " + item.Name.ToUpper() + $" in the {inventory.GetItemCategory(item).ToString().ToUpper()} POCKET.");

    }


    public bool CanGive()
    {
        return !given && count > 0 && item != null;
    }

    public object CaptureState()
    {
        return given;
    }

    public void RestoreState(object state)
    {
        given = (bool) state;
    }
}
