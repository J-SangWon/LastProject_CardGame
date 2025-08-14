using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FildMonster : MonoBehaviour, IPointerClickHandler
{
    public MonsterCardData monsterCardData; 
	public CardUI cardUI; 

	private bool isAppeared = false;
    private bool hasReverberated = false;
    // 동일 턴 내 중복 트리거 방지용
    private int lastStartTriggeredTurn = -1;
    private int lastEndTriggeredTurn = -1;

	void Awake()
    { 
		cardUI = GetComponent<CardUI>();
		if(cardUI.cardData is MonsterCardData)
		{
			monsterCardData = (MonsterCardData)cardUI.cardData;
        }
        // 파괴(사망) 이벤트 구독: TargetableCard 경로에서 먼저 호출됨
        var targetable = GetComponent<TargetableCard>();
        if (targetable != null)
        {
            targetable.OnDestroyed += HandleDestroyed;
        }
    }

    private void OnEnable()
    {
        // 필드에 올라간 상태에서만 진입 효과 1회 발동
        if (monsterCardData != null && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Entrance 
            && isAppeared == false 
            && cardUI != null && cardUI.isOnField)
        {

            if (monsterCardData.cardAbility.targetType == TargetType.Single)
            {
                BattleManager.Instance.IsAbilityTargeting = true;
			    BattleManager.Instance.SetAbilityCaster(this.gameObject);
            }
            else Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);

			isAppeared = true;
        }
        
        // 마법/함정 카드의 필드 배치 효과
        if (cardUI != null && cardUI.isOnField)
        {
            HandleFieldPlacement();
        }
    }

	void Update()
    {
        if(monsterCardData != null && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Continuous) 
            Continuous();
            
    }

    private void OnDestroy()
    {
        if (cardUI != null)
        {
            AuraManager.Instance.UnregisterAllFromSource(cardUI);
        }
        
        HandleSpellTrapRemoval();
        
        var targetable = GetComponent<TargetableCard>();
        if (targetable != null)
        {
            targetable.OnDestroyed -= HandleDestroyed;
        }
    }

    public void SetIsAppeared(bool isVisble)
    {
        isAppeared = isVisble; 
    }

	public void OnPointerClick(PointerEventData eventData)
	{
		if (cardUI.cardData is SpellCardData || cardUI.cardData is TrapCardData)
		{
            var spellData = cardUI.cardData as SpellCardData;
            if (spellData != null)
            {
                if (spellData.spellType != SpellType.Field && spellData.spellType != SpellType.Continuous)
                {
                    CardSummonManager.Instance?.SelectCard(this.gameObject);
                    return;
                }
                return;
            }
        }
	}

	public void Entrance(CardAbility cardAbility, int abilityValue) // 진입
	{
		AbilityParameter parameter = new AbilityParameter() { value = abilityValue };

		// targets 리스트가 내부에서 초기화되지 않을 수 있으므로 안전하게 보장
		if (parameter.targets == null)
		{
			parameter.targets = new List<CardUI>();
		}

		if (monsterCardData.cardAbility.targetType == TargetType.Single)
		{
			var targetUI = BattleManager.Instance.AbilityTarget?.GetComponent<CardUI>();
			if (targetUI != null)
				parameter.target = targetUI;
		}
		else
		{
			var targets = GetAbilityTargets(
				monsterCardData.cardAbility.targetType,
				monsterCardData.cardAbility.targetOwner
			);
			parameter.targets = parameter.targets ?? new List<CardUI>(); // Initialize targets before using AddRange
			parameter.targets.AddRange(targets);
		}

		cardAbility?.Activate(cardUI, parameter);

		// 정리
		parameter = null;
		BattleManager.Instance.AbilityCaster = null;
		BattleManager.Instance.AbilityTarget = null;
	}

	private void Continuous() // 지속효과
    {
        // 몬스터 지속 효과는 필드 배치 시 1회 등록되고, Update에서는 조건 체크만 수행
        // 실제 등록은 OnPlacedOnField()에서 처리됨
    }


    private void Reverberation(CardAbility cardAbility, int abilityValue) //여운
    {
        AbilityParameter parameter = new AbilityParameter();

		if (BattleManager.Instance.AbilityTarget?.GetComponent<CardUI>() != null)
			parameter = new AbilityParameter() { value = abilityValue, target = BattleManager.Instance.AbilityTarget?.GetComponent<CardUI>() };

        cardAbility?.Activate(cardUI, parameter);
    }

	private IEnumerable<CardUI> GetAbilityTargets(TargetType type, TargetOwner owner)
	{
		switch (type)
		{
			case TargetType.Fild:
				return GetFromZones(owner,
					PlayerCardManager.Instance.playerMonsterZone,
					PlayerCardManager.Instance.enemyMonsterZone,
					getChildOfChild: true);

			case TargetType.Hand:
				return GetFromZones(owner,
					PlayerCardManager.Instance.playerHandZone,
					PlayerCardManager.Instance.enemyHandZone);

			case TargetType.Deck:
				return GetFromZones(owner,
					PlayerCardManager.Instance.playerDeckZone,
					PlayerCardManager.Instance.enemyDeckZone);

			default:
				return Enumerable.Empty<CardUI>();
		}
	}

	private IEnumerable<CardUI> GetFromZones(TargetOwner owner, Transform playerZone, Transform enemyZone, bool getChildOfChild = false)
	{
		switch (owner)
		{
			case TargetOwner.Player:
				return GetCardUIsFromZone(playerZone, getChildOfChild);
			case TargetOwner.Enemy:
				return GetCardUIsFromZone(enemyZone, getChildOfChild);
			case TargetOwner.All:
				return GetCardUIsFromZone(playerZone, getChildOfChild)
					.Concat(GetCardUIsFromZone(enemyZone, getChildOfChild));
			default:
				return Enumerable.Empty<CardUI>();
		}
	}

	private IEnumerable<CardUI> GetCardUIsFromZone(Transform zone, bool getChildOfChild)
	{
		for (int i = 0; i < zone.childCount; i++)
		{
			Transform target = zone.GetChild(i);
			if (getChildOfChild && target.childCount > 0)
				target = target.GetChild(0);

			var cardUI = target?.GetComponent<CardUI>();
			if (cardUI != null && cardUI.gameObject != this.gameObject)
				yield return cardUI;
		}
	}

	private void HandleDestroyed()
    {
        if (hasReverberated) return;
        if (monsterCardData != null && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Reverberation)
        {
            Reverberation(monsterCardData.cardAbility, monsterCardData.abilityValue);
            hasReverberated = true;
        }
    }

    public void OnPlacedOnField()
    {
        if (monsterCardData != null && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Entrance && !isAppeared)
        {
            Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);
            isAppeared = true;
        }
        
        // 몬스터 지속 효과 등록
        if (monsterCardData != null && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Continuous)
        {
            RegisterMonsterContinuousEffect();
        }
        
        // 마법/함정 카드 필드 배치 효과
        HandleFieldPlacement();

        // 이 카드가 필드에 진입했으므로, 현재 활성 오라를 적용
        if (cardUI != null && cardUI.isOnField)
        {
            AuraManager.Instance.NotifyCardEnteredField(cardUI);
        }
    }
    
    // 마법/함정 카드 필드 배치 시 효과
    private void HandleFieldPlacement()
{
    if (cardUI.cardData is SpellCardData spellCard)
    {
        Debug.Log($"마법 카드 필드 배치: {spellCard.cardName}");

        // 1) 필드 마법은 FildMonster가 발동을 절대 수행하지 않는다.
        //    FieldSpellZone 경로에서만 발동/조건 분기가 이루어지도록 완전히 위임하여 2중 발동을 방지.
        if (spellCard.spellType == SpellType.Field)
        {
            Debug.Log($"[FildMonster] 필드 마법은 FieldSpellZone에서만 처리: {spellCard.cardName}");
            return;
        }

        // 2) 지속 마법은 필드에 남아서 지속 효과를 발휘한다.
        if (spellCard.spellType == SpellType.Continuous)
        {
            // 턴 트리거 조건이 있는지 확인
            bool hasTurnTrigger = false;
            var cond = spellCard.cardAbility?.condition;
            if (cond != null && cond.conditionType != null)
            {
                foreach (var t in cond.conditionType)
                {
                    if (t == ConditionType.OnTurnStart || t == ConditionType.OnTurnEnd)
                    {
                        hasTurnTrigger = true;
                        break;
                    }
                }
            }
            
            // 지속 마법 등록
            ActivateContinuousSpell(spellCard);
            if (hasTurnTrigger)
            {
                Debug.Log($"[FildMonster] 지속 마법은 턴 트리거에서만 발동: {spellCard.cardName}");
            }
            return;
        }
    else if (cardUI.cardData is TrapCardData trapCard)
    {
        Debug.Log($"함정 카드 필드 배치: {trapCard.cardName}");
        
        // 현재는 즉시 발동으로 처리
        if (trapCard.cardAbility != null)
        {
            AbilityParameter param = new AbilityParameter();
            param.value = trapCard.abilityValue;
            trapCard.cardAbility.Activate(cardUI, param);
        }
    }
    }
}
    
    
    // 마법 카드 효과 발동
    public void ActivateSpellEffect(SpellCardData spellCard)
    {
        if (spellCard.cardAbility == null)
        {
            Debug.LogError($"마법 카드 {spellCard.cardName}의 cardAbility가 null입니다.");
            return;
        }
        
        // 조건 확인
        if (spellCard.cardAbility.condition != null)
        {
            bool conditionMet = EffectConditionEvaluator.IsConditionMet(
                spellCard.cardAbility.condition, 
                GameManager.Instance.CurrentPhase,
                ConditionType.OnCardPlayed,
                spellCard.cardId,
                0
            );
            
            if (!conditionMet)
            {
                Debug.Log("마법 카드 효과 조건이 충족되지 않았습니다.");
                return;
            }
        }
        
        // 효과 발동
        AbilityParameter param = new AbilityParameter();
        param.value = spellCard.abilityValue;
        
        try
        {
            spellCard.cardAbility.Activate(cardUI, param);
            Debug.Log($"마법 카드 효과 발동 성공: {spellCard.cardName}");
            
            // 즉시 마법은 사용 후 제거
            Destroy(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"마법 카드 효과 발동 실패: {spellCard.cardName}, 오류: {e.Message}");
        }
    }

    // 지속 마법 카드 효과 발동 및 등록
    public void ActivateContinuousSpell(SpellCardData spellCard)
    {
        if (spellCard.cardAbility == null)
        {
            Debug.LogError($"지속 마법 카드 {spellCard.cardName}의 cardAbility가 null입니다.");
            return;
        }
        
        // 지속 효과 등록
        AbilityParameter param = new AbilityParameter();
        param.value = spellCard.abilityValue;
        
        // 대상 산출
        if (spellCard.cardAbility.targetType != TargetType.None)
        {
            var targets = GetAbilityTargets(
                spellCard.cardAbility.targetType,
                spellCard.cardAbility.targetOwner
            );
            param.targets = param.targets ?? new List<CardUI>();
            param.targets.AddRange(targets);
        }
        
        try
        {
            // AuraManager를 통해 지속 효과 등록
            if (AuraManager.Instance != null)
            {
                AuraManager.Instance.RegisterContinuousEffect(cardUI, spellCard.cardAbility, param);
            }
            else
            {
                // AuraManager가 없으면 직접 발동 (폴백)
                spellCard.cardAbility.Activate(cardUI, param);
            }
            
            Debug.Log($"지속 마법 효과 등록 성공: {spellCard.cardName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"지속 마법 효과 등록 실패: {spellCard.cardName}, 오류: {e.Message}");
        }
    }

    // 몬스터 지속 효과 등록
    private void RegisterMonsterContinuousEffect()
    {
        if (monsterCardData?.cardAbility == null)
        {
            Debug.LogError($"몬스터 {monsterCardData?.cardName}의 cardAbility가 null입니다.");
            return;
        }
        
        // 조건 확인
        if (monsterCardData.cardAbility.condition != null)
        {
            bool conditionMet = EffectConditionEvaluator.IsConditionMet(
                monsterCardData.cardAbility.condition, 
                GameManager.Instance.CurrentPhase,
                ConditionType.OnCardPlayed,
                monsterCardData.cardId,
                0
            );
            
            if (!conditionMet)
            {
                Debug.Log("몬스터 지속 효과 조건이 충족되지 않았습니다.");
                return;
            }
        }
        
        // 지속 효과 등록
        AbilityParameter param = new AbilityParameter();
        param.value = monsterCardData.abilityValue;
        
        // 대상 산출
        if (monsterCardData.cardAbility.targetType != TargetType.None)
        {
            var targets = GetAbilityTargets(
                monsterCardData.cardAbility.targetType,
                monsterCardData.cardAbility.targetOwner
            );
            param.targets = param.targets ?? new List<CardUI>();
            param.targets.AddRange(targets);
        }
        
        try
        {
            // AuraManager를 통해 지속 효과 등록
            if (AuraManager.Instance != null)
            {
                AuraManager.Instance.RegisterContinuousEffect(cardUI, monsterCardData.cardAbility, param);
            }
            else
            {
                // AuraManager가 없으면 직접 발동 (폴백)
                monsterCardData.cardAbility.Activate(cardUI, param);
            }
            
            Debug.Log($"몬스터 지속 효과 등록 성공: {monsterCardData.cardName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"몬스터 지속 효과 등록 실패: {monsterCardData.cardName}, 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 턴 시작 시 발동되는 효과 처리 (First Phase)
    /// </summary>
    public void TriggerTurnStartEffect()
    {
        // 같은 턴에 중복 발동 방지
        int currentTurn = GameManager.Instance != null ? GameManager.Instance.TurnNumber : -1;
        if (lastStartTriggeredTurn == currentTurn) return;

        // 몬스터 턴 시작 효과
        if (monsterCardData != null && HasTurnTriggerEffect(ConditionType.OnTurnStart))
        {
            ExecuteTurnTriggerEffect(ConditionType.OnTurnStart);
        }
        
        // 마법/함정 카드 턴 시작 효과
        if (cardUI.cardData is SpellCardData spellCard && HasSpellTurnTriggerEffect(spellCard, ConditionType.OnTurnStart))
        {
            ExecuteSpellTurnTriggerEffect(spellCard, ConditionType.OnTurnStart);
        }
        else if (cardUI.cardData is TrapCardData trapCard && HasTrapTurnTriggerEffect(trapCard, ConditionType.OnTurnStart))
        {
            ExecuteTrapTurnTriggerEffect(trapCard, ConditionType.OnTurnStart);
        }

        // 이번 턴 처리 완료로 표시
        lastStartTriggeredTurn = currentTurn;
    }

    /// <summary>
    /// 턴 종료 시 발동되는 효과 처리 (End Phase)
    /// </summary>
    public void TriggerTurnEndEffect()
    {
        // 같은 턴에 중복 발동 방지
        int currentTurn = GameManager.Instance != null ? GameManager.Instance.TurnNumber : -1;
        if (lastEndTriggeredTurn == currentTurn) return;

        // 몬스터 턴 종료 효과
        if (monsterCardData != null && HasTurnTriggerEffect(ConditionType.OnTurnEnd))
        {
            ExecuteTurnTriggerEffect(ConditionType.OnTurnEnd);
        }
        
        // 마법/함정 카드 턴 종료 효과
        if (cardUI.cardData is SpellCardData spellCard && HasSpellTurnTriggerEffect(spellCard, ConditionType.OnTurnEnd))
        {
            ExecuteSpellTurnTriggerEffect(spellCard, ConditionType.OnTurnEnd);
        }
        else if (cardUI.cardData is TrapCardData trapCard && HasTrapTurnTriggerEffect(trapCard, ConditionType.OnTurnEnd))
        {
            ExecuteTrapTurnTriggerEffect(trapCard, ConditionType.OnTurnEnd);
        }
    }

    // 몬스터 턴 트리거 효과 체크
    private bool HasTurnTriggerEffect(ConditionType triggerType)
    {
        return monsterCardData?.cardAbility?.condition != null &&
               EffectConditionEvaluator.IsConditionMet(
                   monsterCardData.cardAbility.condition,
                   GameManager.Instance.CurrentPhase,
                   triggerType,
                   monsterCardData.cardId,
                   0
               );
    }

    // 몬스터 턴 트리거 효과 실행
    private void ExecuteTurnTriggerEffect(ConditionType triggerType)
    {
        try
        {
            AbilityParameter param = new AbilityParameter();
            param.value = monsterCardData.abilityValue;
            
            // 대상 산출
            if (monsterCardData.cardAbility.targetType != TargetType.None)
            {
                var targets = GetAbilityTargets(
                    monsterCardData.cardAbility.targetType,
                    monsterCardData.cardAbility.targetOwner
                );
                param.targets = param.targets ?? new List<CardUI>();
                param.targets.AddRange(targets);
            }
            
            monsterCardData.cardAbility.Activate(cardUI, param);
            Debug.Log($"[FildMonster] 몬스터 턴 트리거 효과 발동: {monsterCardData.cardName} ({triggerType})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FildMonster] 몬스터 턴 트리거 효과 실패: {e.Message}");
        }
    }

    // 마법 카드 턴 트리거 효과 체크
    private bool HasSpellTurnTriggerEffect(SpellCardData spellCard, ConditionType triggerType)
    {
        return spellCard?.cardAbility?.condition != null &&
               EffectConditionEvaluator.IsConditionMet(
                   spellCard.cardAbility.condition,
                   GameManager.Instance.CurrentPhase,
                   triggerType,
                   spellCard.cardId,
                   0
               );
    }

    // 마법 카드 턴 트리거 효과 실행
    private void ExecuteSpellTurnTriggerEffect(SpellCardData spellCard, ConditionType triggerType)
    {
        try
        {
            AbilityParameter param = new AbilityParameter();
            param.value = spellCard.abilityValue;
            
            if (spellCard.cardAbility.targetType != TargetType.None)
            {
                var targets = GetAbilityTargets(
                    spellCard.cardAbility.targetType,
                    spellCard.cardAbility.targetOwner
                );
                param.targets = param.targets ?? new List<CardUI>();
                param.targets.AddRange(targets);
            }
            
            spellCard.cardAbility.Activate(cardUI, param);
            Debug.Log($"[FildMonster] 마법 카드 턴 트리거 효과 발동: {spellCard.cardName} ({triggerType})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FildMonster] 마법 카드 턴 트리거 효과 실패: {e.Message}");
        }
    }

    // 함정 카드 턴 트리거 효과 체크
    private bool HasTrapTurnTriggerEffect(TrapCardData trapCard, ConditionType triggerType)
    {
        return trapCard?.cardAbility?.condition != null &&
               EffectConditionEvaluator.IsConditionMet(
                   trapCard.cardAbility.condition,
                   GameManager.Instance.CurrentPhase,
                   triggerType,
                   trapCard.cardId,
                   0
               );
    }

    // 함정 카드 턴 트리거 효과 실행
    private void ExecuteTrapTurnTriggerEffect(TrapCardData trapCard, ConditionType triggerType)
    {
        try
        {
            AbilityParameter param = new AbilityParameter();
            param.value = trapCard.abilityValue;
            
            if (trapCard.cardAbility.targetType != TargetType.None)
            {
                var targets = GetAbilityTargets(
                    trapCard.cardAbility.targetType,
                    trapCard.cardAbility.targetOwner
                );
                param.targets = param.targets ?? new List<CardUI>();
                param.targets.AddRange(targets);
            }
            
            trapCard.cardAbility.Activate(cardUI, param);
            Debug.Log($"[FildMonster] 함정 카드 턴 트리거 효과 발동: {trapCard.cardName} ({triggerType})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FildMonster] 함정 카드 턴 트리거 효과 실패: {e.Message}");
        }
    }
    
    // 함정 카드 효과 발동
    private void ActivateTrapEffect(TrapCardData trapCard)
    {
        if (trapCard.cardAbility == null)
        {
            Debug.LogError($"함정 카드 {trapCard.cardName}의 cardAbility가 null입니다.");
            return;
        }
        
        // 조건 확인
        if (trapCard.cardAbility.condition != null)
        {
            bool conditionMet = EffectConditionEvaluator.IsConditionMet(
                trapCard.cardAbility.condition, 
                GameManager.Instance.CurrentPhase,
                ConditionType.OnCardPlayed,
                trapCard.cardId,
                0
            );
            
            if (!conditionMet)
            {
                Debug.Log("함정 카드 효과 조건이 충족되지 않았습니다.");
                return;
            }
        }
        
        // 효과 발동
        AbilityParameter param = new AbilityParameter();
        param.value = trapCard.abilityValue;
        
        try
        {
            trapCard.cardAbility.Activate(cardUI, param);
            Debug.Log($"함정 카드 효과 발동 성공: {trapCard.cardName}");
            
            // 즉시 함정은 사용 후 제거
            Destroy(gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"함정 카드 효과 발동 실패: {trapCard.cardName}, 오류: {e.Message}");
        }
    }
    
    // 마법/함정 카드 제거 시 효과
    private void HandleSpellTrapRemoval()
    {
        if (cardUI.cardData is SpellCardData spellCard)
        {
            if(spellCard.spellType != SpellType.Field)
                Debug.Log($"마법 카드 제거: {spellCard.cardName}");
        }
        else if (cardUI.cardData is TrapCardData trapCard)
        {
            Debug.Log($"함정 카드 제거: {trapCard.cardName}");
        }
    }
}
