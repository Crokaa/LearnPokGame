using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using System.Linq;

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
                StartCoroutine(Interact());
        }
    }
    IEnumerator Interact()
    {
        var faceDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + faceDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.1f, GameLayers.Instance.InteractableLayer);

        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
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

        var saveData = new PlayerSaveData {
            position = new float [] {transform.position.x, transform.position.y},
            lookingAt = new float [] {character.lookingAt.x, character.lookingAt.y},
            pokParty = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetPokemonSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData) state;

        transform.position = new Vector3(saveData.position[0], saveData.position[1]);
        character.LookTowards(transform.position + new Vector3(saveData.lookingAt[0], saveData.lookingAt[1]));

        GetComponent<PokemonParty>().Pokemons = saveData.pokParty.Select(p => new Pokemon(p)).ToList();
    }
}

[System.Serializable]
public class PlayerSaveData {

    public float[] position;
    public  float[] lookingAt;
    public List<PokemonSaveData> pokParty;
}

