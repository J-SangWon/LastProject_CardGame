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
        if (monsterCardData.monsterAbilityType == MonsterCardAbilityType.Entrance 
            && isAppeared == false 
            && cardUI != null && cardUI.isOnField)
        {
            Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);
            isAppeared = true;
        }
    }

	void Update()
    {
        if(monsterCardData.monsterAbilityType == MonsterCardAbilityType.Continuous) Continuous();
    }

    private void OnDestroy()
    {
        // 직접 Destroy(target)로 파괴된 경우에도 여운 1회만 발동
        if(!hasReverberated && monsterCardData.monsterAbilityType == MonsterCardAbilityType.Reverberation)
        {
            Reverberation(monsterCardData.cardAbility, monsterCardData.abilityValue);
            hasReverberated = true;
        }
        var targetable = GetComponent<TargetableCard>();
        if (targetable != null)
        {
            targetable.OnDestroyed -= HandleDestroyed;
        }
    }

	public void OnPointerClick(PointerEventData eventData)
	{
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
        if (monsterCardData.monsterAbilityType == MonsterCardAbilityType.Reverberation)
        {
            Reverberation(monsterCardData.cardAbility, monsterCardData.abilityValue);
            hasReverberated = true;
        }
    }

    public void OnPlacedOnField()
    {
        if (monsterCardData.monsterAbilityType == MonsterCardAbilityType.Entrance && !isAppeared)
        {
            Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);
            isAppeared = true;
        }
    }
}
