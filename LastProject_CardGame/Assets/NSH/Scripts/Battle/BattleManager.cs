using Kalkatos.DottedArrow;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private GameObject attacker;
    private GameObject target;

    private GameObject abilityCaster;
    private GameObject abilityTarget;

    public Arrow Arrow { get => arrow; set => arrow = value; }
    [Header("Arrow Effects")]
    [SerializeField] private Arrow arrow;
    [SerializeField] private AnimationCurve attackAnimCurve;

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
    private bool isAbilityTargeting = false;

    public bool IsAbilityTargeting
    {
        get { return isAbilityTargeting; }
        set { isAbilityTargeting = value; }
    }

    public GameObject AbilityCaster
	{
        get { return abilityCaster; }
        set { abilityCaster = value; }
    }

    public GameObject AbilityTarget
	{
        get { return abilityTarget; }
        set { abilityTarget = value; }
    }



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
        BeginAttack(attacker);
    }

    /// <summary>
    /// 공격 대상 지정 → 전투 실행
    /// </summary>
    public void SetTarget(GameObject card)
    {
        CardUI targetUI = card.GetComponent<CardUI>();
        if (targetUI == null || !targetUI.isOnField || attacker == null) return;
       
        CardUI attackerUI = attacker.GetComponent<CardUI>();
        if (attackerUI == null) return;
        // 자기 자신을 타겟으로 지정 못하게
        if (card == attacker)
        {
            Debug.Log("자기 자신은 공격할 수 없습니다.");
            return;
        }

        if (attackerUI.ownerType == targetUI.ownerType)
        {
            Debug.Log("아군 몬스터는 공격할 수 없습니다.");
            return;
        }
        target = card;
        ExecuteBattle();
    }

    public void SetAbilityCaster(GameObject card)
    {
		CardUI cardUI = card.GetComponent<CardUI>();
		if (cardUI == null || !cardUI.isOnField) return;

		abilityCaster = card;
		Debug.Log($"능력 시전자 설정됨: {cardUI.cardData.cardName}");
		BeginAttack(abilityCaster);
	}

    public void SetAbilityTarget(GameObject card)
    {
		CardUI targetUI = card.GetComponent<CardUI>();
		if (targetUI == null || !targetUI.isOnField || abilityCaster == null) return;

		if (card == abilityCaster)
		{
			Debug.Log("자기 자신에게 능력을 시전할 수 없습니다.");
			return;
		}

		abilityTarget = card;
	}


	/// <summary>
	/// 전투 실행
	/// </summary>
	private void ExecuteBattle()
    {
        if (isResolvingBattle) return;
        StartCoroutine(ExecuteBattleCoroutine());
    }

    private bool isResolvingBattle; // 재진입 방지

    private IEnumerator ExecuteBattleCoroutine()
    {
        if (attacker == null || target == null) yield break;

        var atkUI = attacker.GetComponent<CardUI>();
        var tgtUI = target.GetComponent<CardUI>();
        if (atkUI == null || tgtUI == null) { ResetBattleState(); yield break; }

        // 자기 자신 공격 방지
        if (attacker == target)
        {
            Debug.Log("자기 자신은 공격할 수 없습니다.");
            ResetBattleState();
            yield break;
        }

        // 이미 공격했는지 확인
        if (atkUI.hasAttackedThisTurn)
        {
            Debug.Log($"{atkUI.cardData.cardName} 은(는) 이미 이번 턴 공격했습니다.");
            ResetBattleState();
            yield break;
        }

        isResolvingBattle = true;
        Debug.Log($"{atkUI.cardData.cardName} 이(가) {tgtUI.cardData.cardName} 을(를) 공격!");
        arrow.Deactivate();

        // === 전투 연출 + 데미지 처리까지 코루틴 내부에서 수행 ===
        yield return StartCoroutine(AttackAnimationCoroutine(attacker, target));

        // === (연출 끝) 상태 정리 ===
        // 중간에 카드가 파괴되어도 여기까지 오면 AttackAnimationCoroutine 안에서 처리됨
        attacker = null;
        target = null;

        isResolvingBattle = false;
    }


    /// <summary>
    /// 공격자/대상 수동 초기화
    /// </summary>
    public void ResetBattleState()
    {
        attacker = null;
        target = null;
    }

    #region Attack Effect
    private IEnumerator AttackAnimationCoroutine(GameObject _attacker, GameObject _receiver)
    {
        if (_attacker == null || _receiver == null) yield break;

        var atkUI = _attacker.GetComponent<CardUI>();
        var tgtUI = _receiver.GetComponent<CardUI>();
        if (atkUI == null || tgtUI == null) yield break;

        // 파괴 대비: 공격력은 선계산(고정)
        int damageToTarget = atkUI.attack;
        int damageToAttacker = tgtUI.attack;

        // 1) 공격자/대상 위치 확인
        Transform atkTrParent = _attacker.transform.parent;
        Vector3 originalUp = _attacker.transform.up;
        Vector3 startPos = _attacker.transform.position;
        yield return MoveTo(_attacker.transform, startPos + Vector3.back, 0.2f);
        yield return new WaitForSeconds(0.1f);

        // 2) 공격자 위치 조정(대상 방향으로 회전)
        Vector3 distance = _receiver.transform.position - startPos;
        distance = Vector3.MoveTowards(distance, distance * 0.001f, 1f);
        _attacker.transform.up = distance;
        yield return MoveTo(_attacker.transform, startPos + distance, 0.3f, attackAnimCurve);

        // 3) 히트 연출(히트스톱 등)
        yield return new WaitForSeconds(0.05f);

        // 4) 데미지 적용(존재/참조 재확인)
        tgtUI.ReduceHealth(damageToTarget);
        atkUI.ReduceHealth(damageToAttacker);

        // 5) 한 턴 1회 공격 플래그
        atkUI.MarkAsAttacked();

        // 6) 사망 처리()
        if (atkUI.IsDead) atkUI.ResolveDeath();
        if (tgtUI.IsDead) tgtUI.ResolveDeath();

        // 7) 공격자 원위치
        if(!atkUI.IsDead)
            yield return MoveTo(_attacker.transform, startPos, 0.3f);
        _attacker.transform.up = originalUp;
        _attacker.transform.parent = atkTrParent;
    }

    private IEnumerator MoveTo(Transform transform, Vector3 endPos, float time, AnimationCurve curve = null)
    {
        float startTime = Time.time;
        float elapsed = 0;
        Vector3 startPos = transform.position;
        while (elapsed < time)
        {
            elapsed = Time.time - startTime;
            float t = curve != null ? curve.Evaluate(elapsed / time) : elapsed / time;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;
    }

    public void BeginAttack(GameObject card)
    {
        CancelAttack();
        arrow.SetupAndActivate(card.transform);
        attacker = card;    
    }

    public void CancelAttack()
    {
        arrow.Deactivate();
        if (attacker != null)
        {
            attacker = null;
        }
    }
    #endregion
}
