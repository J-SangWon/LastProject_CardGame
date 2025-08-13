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
//{ get; private set; }
	public CardUI cardUI; 
//{ get; private set; }

	private bool isAppeared = false;
    private bool hasReverberated = false;

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
			BattleManager.Instance.SetAbilityCaster(gameObject);

            if (monsterCardData.cardAbility.targetType == TargetType.Single) BattleManager.Instance.IsAbilityTargeting = true;
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
            
        // 지속 마법/함정 효과 처리
        HandleContinuousSpellTrap();
    }

    private void OnDestroy()
    {
        // 주의: 파괴 시점(OnDestroy)에는 트랜스폼 이동/생성 등 무거운 로직을 호출하지 않습니다.
        // 여운(리버브)은 TargetableCard.OnDestroyed 이벤트에서 선행 처리되며,
        // OnDestroy에서는 중복 실행을 피하고 안전하게 정리만 수행합니다.
        
        // 이 카드가 소스로 등록한 모든 지속 오라 해제
        if (cardUI != null)
        {
            AuraManager.UnregisterAllFromSource(cardUI);
        }
        
        // 마법/함정 카드 제거 시 효과
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
		// 마법/함정 카드 클릭 처리
		if (cardUI.cardData is SpellCardData || cardUI.cardData is TrapCardData)
		{
			HandleSpellTrapClick();
			return;
		}
		
        if(BattleManager.Instance.AbilityCaster != null && BattleManager.Instance.IsAbilityTargeting)
        {
            BattleManager.Instance.SetAbilityTarget(gameObject);
            Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);
            BattleManager.Instance.IsAbilityTargeting = false;
        }
		
	}

	private void Entrance(CardAbility cardAbility, int abilityValue) // 진입
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
			if (cardUI != null)
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
        
        // 마법/함정 카드 필드 배치 효과
        HandleFieldPlacement();

        // 이 카드가 필드에 진입했으므로, 현재 활성 오라를 적용
        if (cardUI != null && cardUI.isOnField)
        {
            AuraManager.NotifyCardEnteredField(cardUI);
        }
    }
    
    // 마법/함정 카드 필드 배치 시 효과
    private void HandleFieldPlacement()
    {
        if (cardUI.cardData is SpellCardData spellCard)
        {
            Debug.Log($"마법 카드 필드 배치: {spellCard.cardName}");
            
            // 지속 마법이나 필드 마법인 경우
            if (spellCard.cardAbility != null)
            {
                AbilityParameter param = new AbilityParameter();
                param.value = spellCard.abilityValue;
                spellCard.cardAbility.Activate(cardUI, param);
            }
        }
        else if (cardUI.cardData is TrapCardData trapCard)
        {
            Debug.Log($"함정 카드 필드 배치: {trapCard.cardName}");
            
            // 지속 함정인 경우
            if (trapCard.cardAbility != null)
            {
                AbilityParameter param = new AbilityParameter();
                param.value = trapCard.abilityValue;
                trapCard.cardAbility.Activate(cardUI, param);
            }
        }
    }
    
    // 마법/함정 카드 클릭 시 효과
    private void HandleSpellTrapClick()
    {
        if (cardUI.cardData is SpellCardData spellCard && spellCard.spellType != SpellType.Field)
        {
            Debug.Log($"마법 카드 클릭: {spellCard.cardName}");
            ActivateSpellEffect(spellCard);
        }
        else if (cardUI.cardData is TrapCardData trapCard)
        {
            Debug.Log($"함정 카드 클릭: {trapCard.cardName}");
            ActivateTrapEffect(trapCard);
        }
    }
    
    // 마법 카드 효과 발동
    private void ActivateSpellEffect(SpellCardData spellCard)
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
    
    // 지속 마법/함정 효과 처리
    private void HandleContinuousSpellTrap()
    {
        if (cardUI.cardData is SpellCardData spellCard && spellCard.cardAbility != null)
        {
            // 지속 마법 효과 (필요시 구현)
        }
        else if (cardUI.cardData is TrapCardData trapCard && trapCard.cardAbility != null)
        {
            // 지속 함정 효과 (필요시 구현)
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
