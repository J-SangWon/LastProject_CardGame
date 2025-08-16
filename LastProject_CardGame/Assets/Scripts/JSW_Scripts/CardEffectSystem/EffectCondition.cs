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
    WhenGraveyardCount, // 묘지에 특정 개수 이상 카드가 있을 때
    WhenGraveyardHasTag, // 묘지에 특정 태그 카드가 있을 때
}

public enum ConditionCombination
{
    OR,
    AND
}
[CreateAssetMenu(menuName = "EffectCondition/Condition")]
public class EffectCondition : ScriptableObject
{
    public GamePhase gamePhase;
    public ConditionType[] conditionType;
    public ConditionCombination combination = ConditionCombination.AND; // 조건 조합 방식(AND/OR)
    public string targetCardId;  // 특정 카드 필요 시
    public int intValue;         // HP, 턴 수 등 비교 값
    public string requiredTag;   // 묘지/덱/손패 등에서 찾을 태그 ("DogmaRequiem" 등)
    public OwnerScope ownerScope = OwnerScope.Both; // 조건 평가 대상 진영

}
