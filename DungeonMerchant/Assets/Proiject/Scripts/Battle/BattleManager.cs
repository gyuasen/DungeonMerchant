using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Battle Data")]
    [SerializeField] private MercenaryDataSO mercenaryData;
    [SerializeField] private EnemyDataSO enemyData;

    private BattleUnit playerUnit;
    private BattleUnit enemyUnit;

    private bool battleEnd = false;

    private void Start()
    {
        CreateBattleUnits();
        StartCoroutine(BattleRoutine());
    }

    private void CreateBattleUnits()
    {
        playerUnit = new BattleUnit(
            mercenaryData.mercenaryName,
            mercenaryData.maxHP,
            mercenaryData.attack,
            mercenaryData.defense,
            mercenaryData.attackSpeed
        );

        enemyUnit = new BattleUnit(
            enemyData.enemyName,
            enemyData.maxHP,
            enemyData.attack,
            enemyData.defense,
            enemyData.attackSpeed
        );
    }

    private IEnumerator BattleRoutine()
    {
        Debug.Log("戦闘開始");

        while (!battleEnd)
        {
            yield return new WaitForSeconds(1f);

            Attack(playerUnit, enemyUnit);

            if (CheckBattleEnd())
                yield break;

            yield return new WaitForSeconds(1f);

            Attack(enemyUnit, playerUnit);

            if (CheckBattleEnd())
                yield break;
        }
    }

    private void Attack(BattleUnit attacker, BattleUnit target)
    {
        int damage = attacker.CalculateDamage();

        target.TakeDamage(damage);

        Debug.Log(
            $"{attacker.UnitName} → {target.UnitName} " +
            $"{damage}ダメージ / " +
            $"{target.UnitName} HP:{target.CurrentHP}/{target.MaxHP}"
        );
    }

    private bool CheckBattleEnd()
    {
        if (playerUnit.IsDead)
        {
            Debug.Log("敗北");
            battleEnd = true;
            return true;
        }

        if (enemyUnit.IsDead)
        {
            Debug.Log("勝利");
            battleEnd = true;
            return true;
        }

        return false;
    }
}