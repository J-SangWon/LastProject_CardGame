using UnityEngine;

public enum MonsterCardAbilityType
{
    None,
	Entrance,           //진입
	Reverberation,      //여운
	Continuous          //지속효과
}

public enum AbilityType { TakeDamage, TakeAllDamage, Heal, AllHeal, Destroy, Draw}

[CreateAssetMenu(menuName = "Card/MonsterCard")]
public class MonsterCardData : BaseCardData
{
    public MonsterType monsterType;
    public int attack;
    public int health;
    public Race race;
    public MonsterCardAbilityType monsterAbilityType;
    public AbilityType abilityType;
    public int abiltyValue;

    protected override void OnEnable()
    {
        base.OnEnable();
        cardType = CardType.Monster;
    }
}