using UnityEngine;
using UnityEngine.EventSystems;

public class HitZone : MonoBehaviour, IPointerClickHandler
{
    [Header("HitZone ����")]
    public bool isPlayerZone = true; // true = �÷��̾� ü��, false = �� ü��

    /// <summary>
    /// ī�尡 ��Ʈ���� Ŭ������ �� BattleManager�� ���� ����
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // ��Ŭ���� ó��
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // �����ڰ� ���õǾ� �ִ� ��츸 ó��
        if (BattleManager.Instance.attacker != null)
        {
            // 카드 전투와 동일한 연출로 직접 공격 처리
            BattleManager.Instance.DirectAttackHitZone(this);
        }
    }

    /// <summary>
    /// BattleManager���� �����ڰ� ���õ� �� ȣ��
    /// </summary>
    /// <param name="attacker">���� ī��</param>
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

        Debug.Log($"{cardUI.cardData.cardName} �� {(isPlayerZone ? "Player" : "Enemy")} ��Ʈ���� ����, {damage} ������!");
    }
}
