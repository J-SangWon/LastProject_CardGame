using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private GameObject attacker;
    private GameObject target;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 공격자가 현재 지정되었는지 여부
    /// </summary>
    public bool HasAttacker() => attacker != null;

    /// <summary>
    /// 공격할 몬스터 지정
    /// </summary>
    public void SetAttacker(GameObject card)
    {
        CardUI cardUI = card.GetComponent<CardUI>();
        if (cardUI == null || !cardUI.isOnField) return;

        if (cardUI.hasAttackedThisTurn)
        {
            Debug.Log("이 카드는 이미 공격했습니다.");
            return;
        }

        attacker = card;
        Debug.Log($"공격자 설정됨: {cardUI.cardName}");
    }

    /// <summary>
    /// 공격 대상 지정 → 전투 실행
    /// </summary>
    public void SetTarget(GameObject card)
    {
        CardUI targetUI = card.GetComponent<CardUI>();
        if (targetUI == null || !targetUI.isOnField || attacker == null) return;

        target = card;
        ExecuteBattle();
    }

    /// <summary>
    /// 전투 실행
    /// </summary>
    private void ExecuteBattle()
    {
        CardUI atkUI = attacker.GetComponent<CardUI>();
        CardUI tgtUI = target.GetComponent<CardUI>();

        if (atkUI == null || tgtUI == null) return;

        Debug.Log($"{atkUI.cardName} 이(가) {tgtUI.cardName} 을(를) 공격!");

        tgtUI.ReduceHealth(atkUI.attack);
        atkUI.ReduceHealth(tgtUI.health);

        atkUI.hasAttackedThisTurn = true;

        if (atkUI.IsDead)
            atkUI.HandleDeath();
        if (tgtUI.IsDead)
            tgtUI.HandleDeath();

        attacker = null;
        target = null;
    }

    /// <summary>
    /// 공격자/대상 수동 초기화
    /// </summary>
    public void ResetBattleState()
    {
        attacker = null;
        target = null;
    }
}
