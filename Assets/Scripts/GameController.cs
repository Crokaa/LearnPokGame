using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Paused, Menu, PartyScreen, Bag }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryController inventory;

    GameState state;
    GameState stateBeforePause;
    TrainerController trainer;
    Weather currWeatherOutside;
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }
    //this will be removed later, I'm just using it for testing
    //float time = 0;
    MenuController menuController;

    public static GameController Instance { get; private set; }
    public GameState State { get { return state; } }

    private void Start()
    {
        partyScreen.Init();

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

        menuController.goBack += () =>
        {
            state = GameState.FreeRoam;
            menuController.CloseMenu();
        };

        menuController.onSelected += MenuSelection;
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

        menuController = GetComponent<MenuController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PokemonDB.Init();
        MovesDB.Init();
        ConditionsDB.Init();
        WeatherDB.Init();
        AbilitiesDB.Init();
        ItemDB.Init();
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

    void MenuSelection(int selected)
    {
        switch (selected)
        {
            case 0:
                //Pokedex
                break;
            case 1:
                //Pokemon Party
                state = GameState.PartyScreen;
                break;
            case 2:
                //Bag
                state = GameState.Bag;
                break;
            case 3:
                SavingSystem.Instance.Save("saveSlot1");
                break;
            case 4:
                SavingSystem.Instance.Load("saveSlot1");
                break;

        }
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {

            if (Input.GetKeyDown(KeyCode.Return))
            {
                state = GameState.Menu;
                menuController.OpenMenu();
            }

            /* No weather for now 
            time += Time.deltaTime;

            if (time > 10)
            {
                currWeatherOutside = WeatherDB.Weathers.ElementAt(UnityEngine.Random.Range(0, WeatherDB.Weathers.Count)).Value;
                time = 0;
                Debug.Log("Weather changed to " + currWeatherOutside.Id);
            } */

            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
            DialogManager.Instance.HandleUpdate();
        else if (state == GameState.Menu)
            menuController.HandleUpdate();
        else if (state == GameState.PartyScreen)
        {
            partyScreen.gameObject.SetActive(true);
            Action onSelected = () =>
               {
                   //Open pokemon summary
               };
            Action goBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.Menu;
            };

            partyScreen.HandleUpdate(onSelected, goBack);
        }
        else if (state == GameState.Bag)
        {
            inventory.gameObject.SetActive(true);
            Action goBack = () =>
            {
                inventory.gameObject.SetActive(false);
                state = GameState.Menu;
            };

            inventory.HandleUpdate(goBack);
        }

        //Just to speed things up while testing
        if (Input.GetKeyDown(KeyCode.P))
            Time.timeScale = 2f;
        else if (Input.GetKeyDown(KeyCode.O))
            Time.timeScale = 1f;

    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }
}
