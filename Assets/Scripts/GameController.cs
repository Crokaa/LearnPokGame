using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;


    GameState state;

    private void Start()
    {

        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        playerController.OnEnterTrainerView += (Collider2D trainerCollider) => {

            var trainer = trainerCollider.GetComponentInParent<TrainerController>();

            state = GameState.Cutscene;

            if(trainer != null)
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
        };

        DialogManager.Instance.OnShowDialog += () => {

            state = GameState.Dialog;
        };
        
        DialogManager.Instance.OnCloseDialog += () => {

            if(state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
    }

    private void Awake() {

        ConditionsDB.Init();
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {

        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if(state == GameState.Dialog)
            DialogManager.Instance.HandleUpdate();
    }
}
