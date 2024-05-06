using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] GameObject exclamation;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject fov;
    [SerializeField] Sprite sprite;

    public string Name
    {

        get { return name; }
    }

    public Sprite Sprite
    {

        get { return sprite; }
    }

    Character character;
    bool lostBattle = false;

    private void Start()
    {
        SetFovRotations(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if(!lostBattle)
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                GameController.Instance.StartTrainerBattle(this);
            }));
        else 
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {

        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - character.transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        player.LookTowards(transform.position);
        
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
        {
            GameController.Instance.StartTrainerBattle(this);
        }));
    }

    public void BattleLost() {
        lostBattle = true;
        fov.SetActive(false);
    }

    public void SetFovRotations(FacingDirection dir)
    {

        float angle = 0f;

        if (dir == FacingDirection.Right)
            angle = 90;
        else if (dir == FacingDirection.Left)
            angle = -90;
        else if (dir == FacingDirection.Up)
            angle = 180;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return lostBattle;
    }

    public void RestoreState(object state)
    {
        lostBattle = (bool)state;
        Debug.Log(lostBattle);
        if(lostBattle)
            fov.SetActive(false);
    }
}
