using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD playerHud;
    [SerializeField] BattleHUD enemyHud;

    private void Start() {
        setupBattle();
    }

    public void setupBattle() {
        playerUnit.setup();
        enemyUnit.setup();
        playerHud.setData(playerUnit.Pokemon);
        enemyHud.setData(enemyUnit.Pokemon);
    }
}
