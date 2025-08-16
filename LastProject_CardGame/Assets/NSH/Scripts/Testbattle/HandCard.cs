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

        // 이미 선택된 카드면 선택 해제
        if (CardSummonManager.Instance.GetSelectedCard() == gameObject)
        {
            cardUI.SetOutline(false);
            CardSummonManager.Instance.DeselectCard();
            return;
        }
        if (GameManager.Instance.IsDiscardSelectionActive)
        {
            if (GameManager.Instance.IsCardSelectedForDiscard(gameObject))
                GameManager.Instance.DeselectCardForDiscard(gameObject);
            else
                GameManager.Instance.SelectCardForDiscard(gameObject);
            return;
        }
        // 새 카드 선택
        CardSummonManager.Instance.DeselectCard();
        CardSummonManager.Instance.SelectCard(gameObject);
        cardUI.SetOutline(true);
    }

}
