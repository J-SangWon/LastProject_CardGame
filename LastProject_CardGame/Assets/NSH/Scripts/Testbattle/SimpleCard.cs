using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SimpleCard : MonoBehaviour, IPointerClickHandler
{
    public Test_Carddate cardData; // 인스펙터에서 노출됨

    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;

    private bool hasAttackedThisTurn = false;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (nameText != null) nameText.text = cardData.cardName;
        if (attackText != null) attackText.text = cardData.attack.ToString();
        if (healthText != null) healthText.text = cardData.health.ToString();
    }

    public void ReduceHealth(int amount)
    {
        cardData.health -= amount;
        if (cardData.health < 0) cardData.health = 0;
        UpdateUI();
        if (cardData.health == 0)
        {
            Debug.Log($"{cardData.cardName} has died.");
            Destroy(gameObject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{cardData.cardName} clicked!");

        if (BattleManager_test.Instance == null)
        {
            Debug.LogError("BattleManager_test 인스턴스 없음!");
            return;
        }

        if (!BattleManager_test.Instance.HasAttacker())
        {
            // 공격자가 아직 없으면 이 카드를 공격자로 등록
            BattleManager_test.Instance.SetAttacker(gameObject);

            //Arrow 효과를 시작
            BattleManager_test.Instance.BeginAttack(this.gameObject);
        }
        else
        {
            // 이미 공격자가 선택된 상태면 이 카드를 공격 대상(Target)으로 등록
            if (BattleManager_test.Instance != null)
                BattleManager_test.Instance.SetTarget(gameObject);

            BattleManager_test.Instance.EndAttack(this.gameObject);
        }
    }


    public bool HasAttackedThisTurn()
    {
        return hasAttackedThisTurn;
    }

    public void SetAttackedThisTurn(bool value)
    {
        hasAttackedThisTurn = value;
    }
}
