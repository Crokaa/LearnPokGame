using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] GameObject exclamation;
    [SerializeField] Dialog dialog;
    [SerializeField] GameObject fov;

    Character character;

    private void Start() {

        SetFovRotations(character.Animator.DefaultDirection);
    }

    private void Awake() {

        character = GetComponent<Character>();
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player) {

        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - character.transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
            Debug.Log("Start battle");
        }));
    }

    public void SetFovRotations(FacingDirection dir) {

        float angle = 0f;

        if(dir == FacingDirection.Right)
            angle = 90;
        else if (dir == FacingDirection.Left)
            angle = -90;
        else if(dir == FacingDirection.Up)
            angle = 180;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}
