using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    private Vector2 input;
    private Character character;

    [SerializeField] Sprite sprite;

    public string Name
    {

        get { return name; }
    }

    public Sprite Sprite
    {

        get { return sprite; }
    }

    public Character Character
    {

        get { return character; }
    }

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {

        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0)
                input.y = 0;

            if (input != Vector2.zero)
                StartCoroutine(character.Move(input, OnMoveOver));

            character.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Z))
                Interact();
        }
    }
    void Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.1f, GameLayers.Instance.InteractableLayer);

        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {

        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.Instance.TriggerableLayers);

        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                character.Animator.IsMoving = false;
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public void LookTowards(Vector3 position)
    {
        character.LookTowards(position);
    }

    public object CaptureState()
    {

        float [] data = new float[] {transform.position.x, transform.position.y, character.lookingAt.x, character.lookingAt.y};

        return data;
    }

    public void RestoreState(object state)
    {
        float [] data = (float [])state;
        
        transform.position = new Vector3(data[0], data[1]);
        character.LookTowards(transform.position + new Vector3(data[2], data[3]));
    }
}
