using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MonsterZoneSlot : MonoBehaviour, IPointerClickHandler
{
    public bool isOccupied = false;

    private void Update()
    {
        //슬롯이 점유되었는데 자식 오브젝트가 없거나 모두 비활성 상태면 비어있다고 판단
        if (isOccupied)
        {
            if (transform.childCount == 0)
            {
                isOccupied = false;
            }
            else
            {
                bool allInactive = true;
                foreach (Transform child in transform)
                {
                    if (child.gameObject.activeSelf)
                    {
                        allInactive = false;
                        break;
                    }
                }
                if (allInactive)
                    isOccupied = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isOccupied) return;

        GameObject selectedCard = CardSummonManager.Instance.GetSelectedCard();
        if (selectedCard != null)
        {
            var cardUI = selectedCard.GetComponent<CardUI>();
            if (cardUI == null)
            {
                return;
            }

            // 몬스터 카드인지 체크 (추가)
            if (cardUI.cardData == null || cardUI.cardData.cardType != CardType.Monster)
            {
                Debug.Log("몬스터 카드만 몬스터 존에 소환할 수 있습니다.");
                return;
            }

            bool isPlayerSlot = CompareTag("PlayerZone");
            bool isEnemySlot = CompareTag("EnemyZone");

            if (isPlayerSlot && cardUI.ownerType != OwnerType.Player)
            {
                return;
            }

            if (isEnemySlot && cardUI.ownerType != OwnerType.Opponent)
            {
                return;
            }

            SummonCardToSlot(selectedCard);
            CardSummonManager.Instance.DeselectCard();
        }
    }

    private void SummonCardToSlot(GameObject card)
    {
        card.transform.SetParent(transform);
        card.transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutQuad);

        var handCard = card.GetComponent<HandCard>();
        if (handCard != null) handCard.isInHand = false;

        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null) cardUI.isOnField = true;

        var fm = card.GetComponent<FildMonster>();
        if (fm != null)
        {
            fm.OnPlacedOnField();
        }

        isOccupied = true;
    }
}
