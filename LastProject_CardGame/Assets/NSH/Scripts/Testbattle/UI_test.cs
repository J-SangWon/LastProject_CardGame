using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_test : MonoBehaviour
{
    [Header("카드 데이터")]
    public MonsterCardData cardData;

    [Header("UI 요소")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;

    [Header("상태 플래그")]
    public bool isOnField = true;
    public bool hasAttackedThisTurn = false;

    //  외부에서 안전하게 참조할 수 있는 프로퍼티들
    public string CardName => cardData != null ? cardData.cardName : "???";
    public int Attack => cardData != null ? cardData.attack : 0;
    public int CurrentHP => cardData != null ? cardData.currentHP : 0;
    public bool IsDead => cardData != null && cardData.IsDead();

    private void Start()
    {
        if (cardData != null)
        {
            UpdateUI();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();  // 기존 OnClick 메서드 호출
    }
    public void UpdateUI()
    {
        if (cardData == null) return;

        if (nameText != null) nameText.text = cardData.cardName;
        if (attackText != null) attackText.text = cardData.attack.ToString();
        if (healthText != null) healthText.text = cardData.currentHP.ToString();
    }

    public void ReduceHealth(int amount)
    {
        if (cardData == null) return;

        cardData.TakeDamage(amount);
        UpdateUI();
    }

    public void HandleDeath()
    {
        Debug.Log($"{CardName} 사망!");
        Destroy(gameObject);
    }

    public void OnClick()
    {
        if (!BattleManager.Instance.HasAttacker())
            BattleManager.Instance.SetAttacker(gameObject);
        else
            BattleManager.Instance.SetTarget(gameObject);

}
}
