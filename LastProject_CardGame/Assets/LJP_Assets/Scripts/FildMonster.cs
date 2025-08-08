using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FildMonster : MonoBehaviour, IPointerClickHandler
{
    public MonsterCardData monsterCardData { get; private set; }

    [SerializeField] private Image illustration;
    [SerializeField] private Text AttackTex;
    [SerializeField] private Text HealthTex;
    private Sprite artWork;
    public int Attack { get; private set; }
    private int maxHealth;
    private int currentHealth;

	[HideInInspector] public bool isAppeared = false;
	private bool hasAttackedThisTurn = false;
	private bool isEntrance = false;

	void Start()
    {
        monsterCardData = GetComponent<MonsterCardData>();
        InitStatus();

        if (monsterCardData.monsterAbilityType == MonsterCardAbilityType.Entrance && isAppeared == false) Entrance(monsterCardData.abilityType, monsterCardData.abiltyValue);
    }

    void Update()
    {
        if(monsterCardData.monsterAbilityType == MonsterCardAbilityType.Continuous) Continuous();
        UpdateStatus();
    }

	private void OnDestroy()
	{
		if(monsterCardData.monsterAbilityType == MonsterCardAbilityType.Reverberation) Reverberation(monsterCardData.abilityType, monsterCardData.abiltyValue);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log($"{monsterCardData.cardName} clicked!");

		if (BattleManager_test.Instance == null)
		{
			Debug.LogError("BattleManager_test 인스턴스 없음!");
			return;
		}

		if (!isEntrance)
		{
			if (!BattleManager_test.Instance.HasAttacker())
			{
				// 공격자가 아직 없으면 이 카드를 공격자로 등록
				BattleManager_test.Instance.SetAttacker(gameObject);
			}
			else
			{
				// 이미 공격자가 선택된 상태면 이 카드를 공격 대상(Target)으로 등록
				if (BattleManager_test.Instance != null)
					BattleManager_test.Instance.SetTarget(gameObject);
			}
		}
		else
		{
			isEntrance = false;
		}
	}

	private void InitStatus()
    {
        if (monsterCardData == null) return;

        Attack = monsterCardData.attack;
		maxHealth = monsterCardData.health;
        currentHealth = maxHealth;
        artWork = monsterCardData.artwork;

        AttackTex.text = Attack.ToString();
        HealthTex.text = maxHealth.ToString();
        illustration.sprite = artWork;
    }

    private void UpdateStatus()
    {
		AttackTex.text = Attack.ToString();
		HealthTex.text = currentHealth.ToString();
	}

    public void TakeDamage(int amount)
    {
        currentHealth = currentHealth - amount;
        if (currentHealth <= 0) Destroy(gameObject);
    }

	public void Heal(int amount)
	{
		currentHealth = currentHealth + amount;
		if (currentHealth > maxHealth) currentHealth = maxHealth;
	}

	private void Entrance(AbilityType abilityType, int abilityValue) //진입
    {
        switch (abilityType)
        {
            case AbilityType.TakeDamage:
				if (BattleManager_test.Instance != null)
					BattleManager_test.Instance.SetAttacker(gameObject);
				isEntrance = true;
				break;
            case AbilityType.TakeDamageAll:
                break;
            case AbilityType.Heal:
                break;
            case AbilityType.HealAll:
                break;
            case AbilityType.Destroy:
                break;
        }
    }

    private void Continuous() // 지속효과
    {

    }


	private void Reverberation(AbilityType abilityType, int abilityValue) //여운
    {

		switch (abilityType)
		{
			case AbilityType.TakeDamage:
				break;
			case AbilityType.TakeDamageAll:
				break;
			case AbilityType.Heal:
				break;
			case AbilityType.HealAll:
				break;
			case AbilityType.Destroy:
				break;
		}
	}

	public bool HasAttackedThisTurn()
	{
		return hasAttackedThisTurn;
	}

	public void SetAttackedThisTurn(bool value)
	{
		hasAttackedThisTurn = value;
	}
}
