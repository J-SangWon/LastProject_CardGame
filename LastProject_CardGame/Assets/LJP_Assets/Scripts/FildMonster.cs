using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FildMonster : MonoBehaviour
{
    public MonsterCardData monsterCardData { get; private set; }
	public CardUI cardUI { get; private set; }

	[HideInInspector] public bool isAppeared = false;
	private bool isEntrance = false;

	void Awake()
    { 
		cardUI = GetComponent<CardUI>();
		monsterCardData = cardUI.monsterCardData;
    }

	private void OnEnable()
	{
		if (monsterCardData.monsterAbilityType == MonsterCardAbilityType.Entrance && isAppeared == false) Entrance(monsterCardData.cardAbility, monsterCardData.abilityValue);
	}

	void Update()
    {
        if(monsterCardData.monsterAbilityType == MonsterCardAbilityType.Continuous) Continuous();
    }

	private void OnDestroy()
	{
		if(monsterCardData.monsterAbilityType == MonsterCardAbilityType.Reverberation) Reverberation(monsterCardData.cardAbility, monsterCardData.abilityValue);
	}

	private void Entrance(CardAbility cardAbility, int abilityValue) //진입
    {
		AbilityParameter parameter = new AbilityParameter() { value = abilityValue };
		cardAbility.Activate(cardUI, parameter);
    }

    private void Continuous() // 지속효과
    {

    }


	private void Reverberation(CardAbility cardAbility, int abilityValue) //여운
	{
		CardUI _target = new CardUI();
		AbilityParameter parameter = new AbilityParameter() { value = abilityValue, target = _target };
		cardAbility.Activate(cardUI, parameter);
	}
}
