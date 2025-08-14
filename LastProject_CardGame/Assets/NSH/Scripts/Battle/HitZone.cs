using UnityEngine;
using UnityEngine.EventSystems;

public class HitZone : MonoBehaviour, IPointerClickHandler
{
    [Header("HitZone 설정")]
    public bool isPlayerZone = true; // true = 플레이어 체력, false = 적 체력

    /// <summary>
    /// 카드가 히트존을 클릭했을 때 BattleManager를 통해 공격
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭만 처리
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // 공격자가 선택되어 있는 경우만 처리
        if (BattleManager.Instance.attacker != null)
        {
            OnHitByCard(BattleManager.Instance.attacker);
            BattleManager.Instance.CancelAttack(); // 공격 끝나면 선택 초기화
        }
    }

    /// <summary>
    /// BattleManager에서 공격자가 선택될 때 호출
    /// </summary>
    /// <param name="attacker">공격 카드</param>
    public void OnHitByCard(GameObject attacker)
    {
        var cardUI = attacker.GetComponent<CardUI>();
        if (cardUI == null) return;

        int damage = cardUI.attack;

        if (isPlayerZone)
            GameManager.Instance.TakeDamageToPlayer(damage);
        else
            GameManager.Instance.TakeDamageToEnemy(damage);

        cardUI.MarkAsAttacked();

        Debug.Log($"{cardUI.cardData.cardName} 가 {(isPlayerZone ? "Player" : "Enemy")} 히트존을 공격, {damage} 데미지!");
    }
}
