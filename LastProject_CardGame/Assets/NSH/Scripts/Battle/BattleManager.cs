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
        Debug.Log($"공격자 설정됨: {cardUI.cardData.cardName}");
    }

    /// <summary>
    /// 공격 대상 지정 → 전투 실행
    /// </summary>
    public void SetTarget(GameObject card)
    {
        CardUI targetUI = card.GetComponent<CardUI>();
        if (targetUI == null || !targetUI.isOnField || attacker == null) return;

        // 자기 자신을 타겟으로 지정 못하게
        if (card == attacker)
        {
            Debug.Log("자기 자신은 공격할 수 없습니다.");
            return;
        }

        target = card;
        ExecuteBattle();
    }



    /// <summary>
    /// 전투 실행
    /// </summary>
    private void ExecuteBattle()
    {
        if (attacker == null || target == null) return;

        CardUI atkUI = attacker.GetComponent<CardUI>();
        CardUI tgtUI = target.GetComponent<CardUI>();

        if (atkUI == null || tgtUI == null)
        {
            ResetBattleState();
            return;
        }

        // 자기 자신 공격 방지
        if (attacker == target)
        {
            Debug.Log("자기 자신은 공격할 수 없습니다.");
            ResetBattleState();
            return;
        }

        // 이미 공격했는지 확인
        if (atkUI.hasAttackedThisTurn)
        {
            Debug.Log($"{atkUI.cardData.cardName} 은(는) 이미 이번 턴 공격했습니다.");
            ResetBattleState();
            return;
        }

        Debug.Log($"{atkUI.cardData.cardName} 이(가) {tgtUI.cardData.cardName} 을(를) 공격!");

        // 데미지 값 미리 계산(파괴/Destroy 중 참조 안전하게)
        int damageToTarget = atkUI.attack;
        int damageToAttacker = tgtUI.attack;

        // 1) 양쪽 데미지 적용 (파괴는 나중에)
        tgtUI.ReduceHealth(damageToTarget);
        atkUI.ReduceHealth(damageToAttacker);

        // 2) 공격 표시
        atkUI.MarkAsAttacked();

        // 3) 전투 후 사망 처리(Resolve)
        if (atkUI.IsDead) atkUI.ResolveDeath();
        if (tgtUI.IsDead) tgtUI.ResolveDeath();

        // 4) 상태 초기화
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
