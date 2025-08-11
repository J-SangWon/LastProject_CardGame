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
        maxHP = health;  // MonsterCardData에서 설정한 health 값을 maxHP에 할당
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);  // 부모 클래스에서 데미지 처리

        // 추가적인 몬스터 특성 처리 (예: 몬스터 카드의 죽음 상태를 로그로 출력)
        if (IsDead())
        {
            Debug.Log($"{cardName} has died!");
        }
    }

    // 카드가 사망했는지 확인
    public bool IsDead()
    {
        return currentHP <= 0;
    }
}