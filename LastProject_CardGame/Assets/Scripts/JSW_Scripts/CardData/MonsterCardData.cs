using UnityEngine;

[CreateAssetMenu(menuName = "Card/MonsterCard")]
public class MonsterCardData : BaseCardData
{
    public MonsterType monsterType;
    public int attack;
    public int health;
    public Race race;

    protected override void OnEnable()
    {
        base.OnEnable();
        cardType = CardType.Monster;
    }
}