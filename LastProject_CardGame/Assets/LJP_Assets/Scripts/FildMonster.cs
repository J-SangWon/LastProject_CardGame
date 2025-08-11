using System.Data;
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

	[HideInInspector] public bool isAppeared = false;
	private bool isEntrance = false;
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
            Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);
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
        // 직접 Destroy(target)로 파괴된 경우에도 여운 1회만 발동
        if(monsterCardData != null && !hasReverberated && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Reverberation)
        {
            Reverberation(monsterCardData.cardAbility, monsterCardData.abilityValue);
            hasReverberated = true;
        }
        
        // 마법/함정 카드 제거 시 효과
        HandleSpellTrapRemoval();
        
        var targetable = GetComponent<TargetableCard>();
        if (targetable != null)
        {
            targetable.OnDestroyed -= HandleDestroyed;
        }
    }

	public void OnPointerClick(PointerEventData eventData)
	{
		// 마법/함정 카드 클릭 처리
		if (cardUI.cardData is SpellCardData || cardUI.cardData is TrapCardData)
		{
			HandleSpellTrapClick();
			return;
		}
		
		// 몬스터 카드 클릭 처리 (기존 로직)
		//Debug.Log($"{monsterCardData.cardName} clicked!");

		//if (BattleManager.Instance == null)
		//{
		//	Debug.LogError("BattleManager_test 인스턴스 없음!");
		//	return;
		//}

		//if (!isEntrance)
		//{
		//	if (!BattleManager.Instance.HasAttacker())
		//	{
		//		// 공격자가 아직 없으면 이 카드를 공격자로 등록
		//		BattleManager_test.Instance.SetAttacker(gameObject);
		//	}
		//	else
		//	{
		//		// 이미 공격자가 선택된 상태면 이 카드를 공격 대상(Target)으로 등록
		//		if (BattleManager.Instance != null)
		//			BattleManager.Instance.SetTarget(gameObject);
		//	}
		//}
		//else
		//{
		//	isEntrance = false;
		//}
	}

    private void Entrance(CardAbility cardAbility, int abilityValue) //진입
    {
        AbilityParameter parameter = new AbilityParameter() { value = abilityValue };
        cardAbility?.Activate(cardUI, parameter);
    }

    private void Continuous() // 지속효과
    {

    }


    private void Reverberation(CardAbility cardAbility, int abilityValue) //여운
    {
        AbilityParameter parameter = new AbilityParameter() { value = abilityValue };
        cardAbility?.Activate(cardUI, parameter);
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
