using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy, PartyScreen }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD playerHud;
    [SerializeField] BattleHUD enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon());
        enemyUnit.Setup(wildPokemon);
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared");

        PlayerAction();
    }

    void PlayerAction()
    {

        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerMove()
    {

        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {

        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        move.Pp--;
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();
        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fainted!");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {

        state = BattleState.EnemyMove;
        var move = enemyUnit.Pokemon.GetRandomMove();
        move.Pp--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();
        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} has fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                OpenPartyScreen();
            }
            else
                OnBattleOver(false);
        }
        else
            PlayerAction();
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
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleActionSelectionMoveSelection();
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
                PlayerMove();
            }
            else if (currentAction == 1)
            {

                //Bag
            }
            else if (currentAction == 2)
            {

                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {

                //Run
            }  
        }
    }

    void HandleActionSelectionMoveSelection()
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
            PlayerAction();
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
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
            PlayerAction();
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
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));

        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {

        bool fainted = false;

        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}.");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        } else
            fainted = true;

        playerUnit.Setup(newPokemon);
        playerHud.SetData(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");
        if(fainted)
            PlayerAction();
        else
            StartCoroutine(EnemyMove());

    }

}
