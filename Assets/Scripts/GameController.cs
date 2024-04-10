using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;
    TrainerController trainer;
    Weather currWeatherOutside;
    //this will be removed later, I'm just using it for testing
    float time = 0;

    public static GameController Instance { get; private set; }

    private void Start()
    {

        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        playerController.OnEnterTrainerView += (Collider2D trainerCollider) =>
        {

            var trainer = trainerCollider.GetComponentInParent<TrainerController>();

            state = GameState.Cutscene;

            if (trainer != null)
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
        };

        DialogManager.Instance.OnShowDialog += () =>
        {

            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {

            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
    }

    private void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
        WeatherDB.Init();
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        // I will pass the weather here
        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        // I will pass the weather here
        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {

        time += Time.deltaTime;

        if (time > 10)
        {
            currWeatherOutside = WeatherDB.Weathers.ElementAt(Random.Range(0, WeatherDB.Weathers.Count)).Value;
            time = 0;
            Debug.Log("Weather changed to " + currWeatherOutside.Id);
        }
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
            DialogManager.Instance.HandleUpdate();
    }
}
