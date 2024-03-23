using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Resources;
using Unity.VisualScripting;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver }
public enum BattleAction { Move, SwitchPokemon, Items, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared");

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

    IEnumerator RunTurns(BattleAction battleAction)
    {

        state = BattleState.RunningTurn;
        BattleUnit fastestUnit = null;
        BattleUnit slowestUnit = null;

        if (battleAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            //Check who attacks first
            bool playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            fastestUnit = playerGoesFirst ? playerUnit : enemyUnit;
            slowestUnit = playerGoesFirst ? enemyUnit : playerUnit;

            var slowestPokemon = slowestUnit.Pokemon;

            yield return RunMove(fastestUnit, slowestUnit, fastestUnit.Pokemon.CurrentMove);

            if (state == BattleState.BattleOver)
                yield break;

            // Update the current fastest unit (even if not faster) cause the fastest one can die with a VolatileStatus and we don't want to attack it
            if (playerGoesFirst && fastestUnit.Pokemon.HP <= 0)
                fastestUnit = playerUnit;

            if (slowestPokemon.HP >= 0)
            {
                yield return RunMove(slowestUnit, fastestUnit, slowestUnit.Pokemon.CurrentMove);

                if (state == BattleState.BattleOver)
                    yield break;
            }
        }
        else if (battleAction == BattleAction.SwitchPokemon)
        {
            var selectedPokemon = playerParty.Pokemons[currentMember];
            state = BattleState.Busy;
            yield return SwitchPokemon(selectedPokemon);

            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);

            bool playerIsFaster = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            fastestUnit = playerIsFaster ? playerUnit : enemyUnit;
            slowestUnit = playerIsFaster ? enemyUnit : playerUnit;
            if (state == BattleState.BattleOver)
                yield break;
        }

        if (state != BattleState.BattleOver)
        {

            yield return RunAfterTurn(fastestUnit);
            yield return RunAfterTurn(slowestUnit);
            ActionSelection();
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
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
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

            yield return CheckIfDead(targetUnit);
        }
        else
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed.");

    }

    IEnumerator CheckIfDead(BattleUnit unit)
    {

        if (unit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{unit.Pokemon.Base.Name} fainted!");
            unit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            CheckBattleOver(unit);
        }

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

        yield return CheckIfDead(sourceUnit);
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
            BattleOver(true);
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {

        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.Effectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.Effectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective...");
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
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
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
        dialogBox.SetMoveNames(newPokemon.Moves);
        currentAction = 0;
        currentMove = 0;
        currentMember = 0;
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        state = BattleState.RunningTurn;

    }

}
