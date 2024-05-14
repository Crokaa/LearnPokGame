using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;
    GameState stateBeforePause;
    TrainerController trainer;
    Weather currWeatherOutside;
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }
    //this will be removed later, I'm just using it for testing
    float time = 0;

    public static GameController Instance { get; private set; }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

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

    public void PauseGame(bool pause)
    {

        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    private void Awake()
    {
        Instance = this;
        PokemonDB.Init();
        MovesDB.Init();
        ConditionsDB.Init();
        WeatherDB.Init();
        AbilitiesDB.Init();
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        // I will pass the weather here
        battleSystem.StartBattle(playerParty, wildPokemonCopy, currWeatherOutside);
    }

    public void OnEnterTrainerView(TrainerController trainer)
    {

        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;

        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty, currWeatherOutside);
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

        if (state == GameState.FreeRoam)
        {

            if(Input.GetKeyDown(KeyCode.K))
                SavingSystem.Instance.Save("saveSlot1");
            else if (Input.GetKeyDown(KeyCode.L))
                SavingSystem.Instance.Load("saveSlot1");

            time += Time.deltaTime;

            if (time > 10)
            {
                currWeatherOutside = WeatherDB.Weathers.ElementAt(Random.Range(0, WeatherDB.Weathers.Count)).Value;
                time = 0;
                Debug.Log("Weather changed to " + currWeatherOutside.Id);
            }

            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
            DialogManager.Instance.HandleUpdate();
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }
}
