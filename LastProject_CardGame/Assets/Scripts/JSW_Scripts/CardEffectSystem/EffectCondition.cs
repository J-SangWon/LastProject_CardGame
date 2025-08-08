using UnityEngine;

public enum ConditionType
{
    None,
    OnSummon,           // 소환 시
    OnAttack,           // 공격 시
    OnDestroyed,        // 파괴 시
    OnDamage,           // 피해를 입었을 때
    OnHeal,             // 회복되었을 때
    OnCardDrawn,       // 카드를 뽑았을 때
    OnCardPlayed,       // 카드를 사용했을 때
    OnCardDiscarded,    // 카드를 버렸을 때
    OnCardExhausted,    // 카드를 소모했을 때
    OnTurnStart,        // 턴 시작 시
    OnTurnEnd,          // 턴 종료 시
    WhenCardInHand,     // 특정 카드가 손패에 있을 때
}
[CreateAssetMenu(menuName = "EffectCondition/Condition")]
public class EffectCondition : ScriptableObject
{
    public GamePhase gamePhase;
    public ConditionType[] conditionType;
    public string targetCardId;  // 특정 카드 필요 시
    public int intValue;         // HP, 턴 수 등 비교 값

}
