using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable, ISavable
{

    [SerializeField] ItemBase item;
    bool picked = false;



    public IEnumerator Interact(Transform initiator)
    {

        if (!picked)
        {
            var inventory = initiator.GetComponent<Inventory>();
            var playerName = initiator.GetComponent<PlayerController>().Name;

            inventory.AddItem(item);

            picked = true;

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            yield return DialogManager.Instance.ShowDialogText($"{playerName} found one " + item.Name.ToUpper() + ".", false, false);
            yield return new WaitForSeconds(1f);
            yield return DialogManager.Instance.ShowDialogText($"{playerName} put away the " + item.Name.ToUpper() + $" in the {inventory.GetItemCategory(item).ToString().ToUpper()} POCKET.");
        }
    }

    public object CaptureState()
    {
        return picked;
    }

    public void RestoreState(object state)
    {
        picked = (bool)state;

        if (picked)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
