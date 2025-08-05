using UnityEngine;

public enum MonsterCardAbilityType
{
    None,
	Entrance,           //진입
	Reverberation,      //여운
	Continuous          //지속효과
}

[CreateAssetMenu(menuName = "Card/MonsterCard")]
public class MonsterCardData : BaseCardData
{
    public MonsterType monsterType;
    public int attack;
    public int health;
    public Race race;
    public MonsterCardAbilityType abilityType;

    protected override void OnEnable()
    {
        base.OnEnable();
        cardType = CardType.Monster;
    }
}