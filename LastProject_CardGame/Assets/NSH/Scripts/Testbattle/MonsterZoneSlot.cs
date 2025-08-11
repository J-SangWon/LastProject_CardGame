using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MonsterZoneSlot : MonoBehaviour, IPointerClickHandler
{
    public bool isOccupied = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isOccupied) return;

        GameObject selectedCard = CardSummonManager.Instance.GetSelectedCard();
        if (selectedCard != null)
        {
            SummonCardToSlot(selectedCard);
            CardSummonManager.Instance.DeselectCard();
        }
    }

    private void SummonCardToSlot(GameObject card)
    {
        card.transform.SetParent(transform);
        card.transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutQuad);
        // 상태 변경
        var handCard = card.GetComponent<HandCard>();
        if (handCard != null) handCard.isInHand = false;

        isOccupied = true;
    }
}
