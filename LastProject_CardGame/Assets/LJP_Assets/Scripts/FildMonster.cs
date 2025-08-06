using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class FildMonster : MonoBehaviour
{
    private MonsterCardData monsterCardData;

    private Image illustration;
    private Text AttackTex;
    private Text HealthTex;
    private Sprite artWork;
    private int Attack;
    private int maxHealth
    private int currentHealth;

	[HideInInspector] public bool isAppeared = false;

    void Start()
    {
        monsterCardData = GetComponentInParent<MonsterCardData>();
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

	private void Entrance(AbilityType abilityType, int abilityValue) //진입
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
}
