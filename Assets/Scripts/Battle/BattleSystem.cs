using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver, AboutToUse, MoveToForget, ShowDialog }
public enum BattleAction { Move, SwitchPokemon, Items, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoise = true;

    // These 3 are used for running. Even after speed drops the formula uses the original speed.
    int originalPlayerSpeed;
    int originalEnemySpeed;
    int runAttempts;
    MoveBase moveToLearn;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;
    bool isTrainerBattle;
    PlayerController player;
    TrainerController trainer;
    Weather currWeather;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, Weather outsideWeather)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;
        Debug.Log(outsideWeather);
        currWeather = outsideWeather;

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, Weather outsideWeather)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        isTrainerBattle = true;
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        currWeather = outsideWeather;

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {

            // Wild Pokemon
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            // These are only relevant to run away (which we can't in trainer battles)
            originalPlayerSpeed = playerUnit.Pokemon.Speed;
            originalEnemySpeed = enemyUnit.Pokemon.Speed;

            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared");
        }
        else
        {

            // Trainer battle
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle.");

            //Send first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);

            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Base.Name}");


            //Send first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);

            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
        }

        if (currWeather?.NaturalStartMessage is not null)
            yield return dialogBox.TypeDialog(currWeather.NaturalStartMessage);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;

        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newPokemon.Base.Name}.");

        while (!Input.GetKeyDown(KeyCode.Z) && !Input.GetKeyDown(KeyCode.X))
            yield return null;

        yield return dialogBox.TypeDialog($"Will {player.Name} change pokemon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator RunTurns(BattleAction battleAction)
    {

        state = BattleState.RunningTurn;
        BattleUnit fastestUnit;
        BattleUnit slowestUnit;

        if (battleAction == BattleAction.Move)
        {

            //Choose Move Phase
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Check who attacks first
            bool playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            if (playerMovePriority > enemyMovePriority)
                playerGoesFirst = true;
            else if (playerMovePriority < enemyMovePriority)
                playerGoesFirst = false;

            fastestUnit = playerGoesFirst ? playerUnit : enemyUnit;
            slowestUnit = playerGoesFirst ? enemyUnit : playerUnit;

            var slowestPokemon = slowestUnit.Pokemon;

            //Execute Move Phase
            yield return RunMove(fastestUnit, slowestUnit, fastestUnit.Pokemon.CurrentMove);

            if (state == BattleState.BattleOver)
                yield break;

            // Update the current fastest unit (even if not faster) cause the fastest one can die with a VolatileStatus and we don't want to attack it
            if (playerGoesFirst && fastestUnit.Pokemon.HP <= 0)
                fastestUnit = playerUnit;
            else if (!playerGoesFirst && fastestUnit.Pokemon.HP <= 0)
                fastestUnit = enemyUnit;

            if (slowestPokemon.HP > 0)
            {
                yield return RunMove(slowestUnit, fastestUnit, slowestUnit.Pokemon.CurrentMove);

                if (state == BattleState.BattleOver)
                    yield break;
            }
        }
        else
        {
            // Every action here required state to be busy because it's doing something
            state = BattleState.Busy;

            if (battleAction == BattleAction.SwitchPokemon)
            {

                var selectedPokemon = playerParty.Pokemons[currentMember];
                yield return SwitchPokemon(selectedPokemon);

            }
            else if (battleAction == BattleAction.Run)
                yield return TryToEscape();
            else if (battleAction == BattleAction.Items)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }

            if (state == BattleState.BattleOver)
                yield break;


            if (isTrainerBattle && battleAction == BattleAction.Run)
            {
                ActionSelection();
                yield break;
            }
            // Enemy turn as this runs every time the player does something that isn't attacking
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);

        }

        //After turn phase

        if (state != BattleState.BattleOver)
        {

            // Calculate this again because there's a chance the player increased its speed or swapped pokemons
            bool playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            fastestUnit = playerGoesFirst ? playerUnit : enemyUnit;
            slowestUnit = playerGoesFirst ? enemyUnit : playerUnit;

            yield return WeatherDamage(fastestUnit, slowestUnit);

            yield return RunAfterTurn(fastestUnit);
            yield return RunAfterTurn(slowestUnit);
            ActionSelection();
        }
    }

    IEnumerator WeatherDamage(BattleUnit fastestUnit, BattleUnit slowestUnit)
    {
        if (state == BattleState.BattleOver || currWeather?.RoundMessage == null)
            yield break;

        // Case where Pokemon dies due to Status and we need to wait until the player switches
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        yield return dialogBox.TypeDialog(currWeather.RoundMessage);


        currWeather.OnAfterTurn?.Invoke(fastestUnit.Pokemon);

        if(fastestUnit.Pokemon.WeatherDamages.Count > 0)
        {
            yield return fastestUnit.Hud.UpdateHP();
            yield return dialogBox.TypeDialog(fastestUnit.Pokemon.WeatherDamages.Dequeue());
        }

        
        if (fastestUnit.Pokemon.HP <= 0)
        {

            yield return CheckIfDead(fastestUnit);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }

        currWeather.OnAfterTurn?.Invoke(slowestUnit.Pokemon);

        if(slowestUnit.Pokemon.WeatherDamages.Count > 0)
        {
            yield return slowestUnit.Hud.UpdateHP();
            yield return dialogBox.TypeDialog(slowestUnit.Pokemon.WeatherDamages.Dequeue());
        }

        if (slowestUnit.Pokemon.HP <= 0)
        {

            yield return CheckIfDead(slowestUnit);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }


    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {

        // Case where Pokemon dies due to VolatileStatus (Confusion) and we need to wait until the player switches
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        bool canAttack = sourceUnit.Pokemon.OnBeforeTurn();

        if (!canAttack)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield return CheckIfDead(sourceUnit);
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit.Pokemon);
        move.Pp--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        if (CheckMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {

            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target, move.Base.Effects);
            }
            else
            {
                targetUnit.PlayHitAnimation();
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon, currWeather);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails, targetUnit.Pokemon);
            }

            if (move.Base.SecEffects is not null && move.Base.SecEffects.Count > 0 && targetUnit.Pokemon.HP > 0)
            {

                foreach (var secEffect in move.Base.SecEffects)
                {

                    var rdm = UnityEngine.Random.Range(1, 100);
                    if (rdm <= secEffect.Chance)
                        yield return RunMoveEffects(sourceUnit.Pokemon, targetUnit.Pokemon, secEffect.Target, secEffect);
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return CheckIfDead(targetUnit);
            }
        }
        else
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed.");

    }

    IEnumerator CheckIfDead(BattleUnit unit)
    {
        yield return dialogBox.TypeDialog($"{unit.Pokemon.Base.Name} fainted!");
        unit.PlayFaintAnimation();

        yield return new WaitForSeconds(2f);


        if (!unit.IsPlayerUnit)
        {

            //Exp Gain
            int expYield = unit.Pokemon.Base.ExpYield;
            int enemyLevel = unit.Pokemon.Level;
            float trainerBonus = isTrainerBattle ? 1.5f : 1.0f;

            //later on add the rest of the variables
            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 5);

            playerUnit.Pokemon.Exp += expGain;

            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} EXP. Points!");
            yield return playerUnit.Hud.SetExpSmooth();

            //Check level Up

            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to LV. {playerUnit.Pokemon.Level}!");
                while (!Input.GetKeyDown(KeyCode.Z) && !Input.GetKeyDown(KeyCode.X))
                    yield return null;

                var learnableMoves = playerUnit.Pokemon.GetLearnableMovesAtCurrentLevel();

                foreach (var newMove in learnableMoves)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumMoves)
                    {

                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.MoveBase.Name}!");

                        /* I will add this if I need to
                        while (!Input.GetKeyDown(KeyCode.Z) && !Input.GetKeyDown(KeyCode.X))
                            yield return null;
                        */
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} is trying to learn {newMove.MoveBase.Name}.");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {PokemonBase.MaxNumMoves} moves.");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.MoveBase);
                        yield return new WaitUntil(() => state == BattleState.RunningTurn);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
                dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            }
            yield return new WaitForSeconds(1f);
        }

        CheckBattleOver(unit);

    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog("Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {

        if (state == BattleState.BattleOver)
            yield break;

        // Case where Pokemon dies due to Status and we need to wait until the player switches
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0)
        {

            yield return CheckIfDead(sourceUnit);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    IEnumerator RunMoveEffects(Pokemon source, Pokemon target, MoveTarget moveTarget, MoveEffects effects)
    {

        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);

            if (effects.Status != ConditionID.none)
            {

                target.SetStatus(effects.Status);
            }

            if (effects.VolatileStatus != ConditionID.none)
            {

                target.SetVolatileStatus(effects.VolatileStatus);
            }

            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
        }
    }

    bool CheckMoveHits(Move move, Pokemon source, Pokemon target)
    {

        if (move.Base.AlwaysHits)
            return true;


        float moveAccuracy = move.Base.Accuracy;

        if (currWeather != null && currWeather.Id == WeatherID.fog)
            moveAccuracy = move.Base.Accuracy * 3 / 5f;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var accBoost = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= accBoost[accuracy];
        else
            moveAccuracy /= accBoost[-accuracy];


        if (evasion > 0)
            moveAccuracy /= accBoost[evasion];
        else
            moveAccuracy *= accBoost[-evasion];



        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }


    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckBattleOver(BattleUnit faintedUnit)
    {

        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
                BattleOver(false);
        }
        else
        {
            if (!isTrainerBattle)
                BattleOver(true);
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                    BattleOver(true);
            }

        }
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextPokemon.Base.Name}.");

        state = BattleState.RunningTurn;
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails, Pokemon target)
    {

        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.ShowWeatherEffectOnMove)
        {
            yield return dialogBox.TypeDialog(currWeather.MoveEffectivenessMessage);
        }
        else
        {
            if (damageDetails.Effectiveness > 1f)
                yield return dialogBox.TypeDialog("It's super effective!");
            else if (damageDetails.Effectiveness < 1f)
                yield return dialogBox.TypeDialog("It's not very effective...");
            else if (damageDetails.Effectiveness == 0)
                yield return dialogBox.TypeDialog("It doesn't affect " + target.Base.Name);
        }
    }

    public void HandleUpdate()
    {

        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartyScreenSelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> OnMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumMoves)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}"));
                    state = BattleState.RunningTurn;
                }
                else
                {
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(PrintMoveLearned(selectedMove, moveToLearn));
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;

            };
            moveSelectionUI.HandleMoveSelection(OnMoveSelected);
        }
    }

    private IEnumerator PrintMoveLearned(MoveBase selectedMove, MoveBase moveToLearn)
    {
        /*This state is used because I will show some dialog and if my state stays MoveToForget it will keep running the handle move selection
        ruining the whole execution of forgetting an old move and learning a new one. Could be changed in the future*/
        state = BattleState.ShowDialog;

        yield return dialogBox.TypeDialog("One...two...and...ta-da!");
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name}...");
        yield return dialogBox.TypeDialog($"And it learned {moveToLearn.Name} instead!");

        //used here so it won't change before I want to (it could be implemented differently, later I can change it)
        state = BattleState.RunningTurn;
    }

    void HandleActionSelection()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < 3 && currentAction != 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 0 && currentAction != 2)
                --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2)
                currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 1)
                currentAction -= 2;
        }


        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {

            if (currentAction == 0)
            {

                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {

                //Bag
                StartCoroutine(RunTurns(BattleAction.Items));
            }
            else if (currentAction == 2)
            {

                //Pokemon
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1 && currentMove != 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0 && currentMove != 2)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            if (move.Pp == 0)
                StartCoroutine(MoveNoPp());
            else
                StartCoroutine(RunTurns(BattleAction.Move));
        }

    }

    void HandlePartyScreenSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMember < playerParty.Pokemons.Count - 1 && currentMember != 1 && currentMember != 3)
                ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMember > 0 && currentMember != 2 && currentMember != 4)
                --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMember < playerParty.Pokemons.Count - 2)
                currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMember > 1)
                currentMember -= 2;
        }

        partyScreen.UpdateMemberSelection(currentMember);


        if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            dialogBox.EnableDialogText(true);

            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a healthy pokemon to continue.");
                return;
            }
            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted Pokemon.");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch to the same Pokemon.");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoise = !aboutToUseChoise;

        dialogBox.UpdateChoiceBox(aboutToUseChoise);

        if (Input.GetKeyDown(KeyCode.Z))
        {

            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoise)
            {
                // Yes option
                prevState = state;
                OpenPartyScreen();
            }
            else
            {
                // No option
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {

            // Same as no option
            prevState = state;
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {

        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}.");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Pokemon.ResetStatBoost();
        playerUnit.Pokemon.CureVolatileStatus();
        playerUnit.Setup(newPokemon);
        originalPlayerSpeed = playerUnit.Pokemon.Speed;
        dialogBox.SetMoveNames(newPokemon.Moves);
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        if (prevState == null)
            state = BattleState.RunningTurn;
        else if (prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerPokemon());
        }

    }

    IEnumerator MoveNoPp()
    {

        state = BattleState.Busy;
        yield return dialogBox.TypeDialog("There's no PP left for this move!");

        while (!Input.GetKeyDown(KeyCode.X) && !Input.GetKeyDown(KeyCode.Z))
            yield return null;

        MoveSelection();

    }

    IEnumerator TryToEscape()
    {
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("No! There's no running from a Trainer battle!");
            while (!Input.GetKeyDown(KeyCode.X) && !Input.GetKeyDown(KeyCode.Z))
                yield return null;

            state = BattleState.RunningTurn;
        }
        else
        {
            if (originalPlayerSpeed >= originalEnemySpeed)
            {
                yield return dialogBox.TypeDialog("Ran away safely!");
                while (!Input.GetKeyDown(KeyCode.X) && !Input.GetKeyDown(KeyCode.Z))
                    yield return null;
                BattleOver(true);
            }
            else
            {

                runAttempts++;
                var r = UnityEngine.Random.Range(0, 256);

                if (originalPlayerSpeed * 128 / originalEnemySpeed + 30 * runAttempts >= r)
                {
                    yield return dialogBox.TypeDialog("Ran away safely!");
                    while (!Input.GetKeyDown(KeyCode.X) && !Input.GetKeyDown(KeyCode.Z))
                        yield return null;
                    BattleOver(true);
                }
                else
                {
                    yield return dialogBox.TypeDialog("Can't escape!");
                    while (!Input.GetKeyDown(KeyCode.X) && !Input.GetKeyDown(KeyCode.Z))
                        yield return null;
                    state = BattleState.RunningTurn;
                }
            }
        }
    }

    IEnumerator ThrowPokeball()
    {

        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"{player.Name} You can't catch trainers pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used POKEBALL!");

        var pokeballOjbect = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballOjbect.GetComponent<SpriteRenderer>();

        //Animations

        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCapturedAnimation();


        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 2, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {

            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Caught
            yield return dialogBox.TypeDialog($"Gotcha!\n{enemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party.");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //Pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.BreakOutAnimation();

            switch (shakeCount)
            {
                case 0:
                    yield return dialogBox.TypeDialog($"Oh no! The Pokémon broke free!");
                    break;
                case 1:
                    yield return dialogBox.TypeDialog($"Aww! It appeared to be caught!");
                    break;
                case 2:
                    yield return dialogBox.TypeDialog($"Aargh! Almost had it!");
                    break;
                case 3:
                    yield return dialogBox.TypeDialog($"Gah! It was so close, too!");
                    break;
            }

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }

    }

    int TryToCatchPokemon(Pokemon pokemon)
    {

        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65536) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }


}
