using UnityEngine;
using UnityEngine.EventSystems;

public class HandCard : MonoBehaviour, IPointerClickHandler
{
    public bool isInHand = true;
    private CardUI cardUI;

    private void Awake()
    {
        cardUI = GetComponent<CardUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInHand) return;

        // GameManager, CardUI, CardData 모두 존재하는지 체크
        if (cardUI == null || cardUI.cardData == null || GameManager.Instance == null)
            return;

        // 메인 페이즈가 아니면 소환 불가
        if (GameManager.Instance.CurrentPhase != GamePhase.MainPhase)
        {
            Debug.Log("소환 실패: 메인 페이즈가 아닙니다.");
            return;
        }

        // 코스트 체크 및 소비 시도
        int cost = cardUI.cardData.cost;
        if (!GameManager.Instance.CanSpendPlayerCost(cost))
        {
            Debug.Log("소환 실패: 코스트가 부족합니다.");
            return;
        }

        if (!GameManager.Instance.SpendPlayerCost(cost))
        {
            Debug.Log("소환 실패: 코스트 소비 실패");
            return;
        }

        // 코스트 검사 통과하면 카드 선택 처리
        CardSummonManager.Instance.SelectCard(this.gameObject);
        cardUI.SetOutline(true);
        Debug.Log("카드 선택됨: " + gameObject.name);
    }
}
