using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MonsterSlotDrop : MonoBehaviour, IDropHandler
{
    public bool isOccupied = false;
    public PlayerController_N ownerPlayer;
    
    void Start()
    {

    }
    
    /// <summary>
    /// 카드를 손패로 되돌리는 메서드
    /// </summary>
    private void ReturnCardToHand(GameObject card, OwnerType ownerType)
    {
        if (card == null) return;
        
        // 카드를 적절한 손패 존으로 되돌리기
        if (ownerType == OwnerType.Player)
        {
            if (PlayerCardManager.Instance != null && PlayerCardManager.Instance.handZone != null)
            {
                card.transform.SetParent(PlayerCardManager.Instance.handZone, false);
                card.transform.localScale = Vector3.one;
                card.transform.localPosition = Vector3.zero;
                Debug.Log($"카드를 플레이어 손패로 되돌렸습니다.");
            }
        }
        else if (ownerType == OwnerType.Opponent)
        {
            if (OpponentCardManager.Instance != null && OpponentCardManager.Instance.handZone != null)
            {
                card.transform.SetParent(OpponentCardManager.Instance.handZone, false);
                card.transform.localScale = Vector3.one;
                card.transform.localPosition = Vector3.zero;
                Debug.Log($"카드를 상대방 손패로 되돌렸습니다.");
            }
        }
        
        // 드래그 상태 초기화
        var dragHandler = card.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.isSummoned = false;
            dragHandler.droppedOnSlot = false;
        }
        
        // 필드 상태 플래그 초기화
        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.isOnField = false;
        }
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied) return;

        GameObject droppedCard = eventData.pointerDrag;
        if (droppedCard == null)
        {
            return;
        }

        var dragHandler = droppedCard.GetComponent<CardDragHandler>();
        if (dragHandler == null)
        {
            return;
        }

        if (dragHandler.isSummoned)
        {
            return;
        }

        // 소유자와 슬롯 태그(PlayerZone/EnemyZone) 매칭 검사
        var ui = droppedCard.GetComponent<CardUI>();
        if (ui == null)
        {
            Debug.LogWarning("[MonsterSlotDrop] CardUI가 없는 오브젝트입니다.");
            return;
        }

        // 몬스터 카드만 몬스터 존에 소환 가능
        if (ui.cardData == null)
        {
            Debug.LogWarning("[MonsterSlotDrop] 카드 데이터가 없는 오브젝트입니다.");
            return;
        }

        if (ui.cardData.cardType != CardType.Monster)
        {
            Debug.Log($"몬스터 카드가 아닌 '{ui.cardData.cardName}'은(는) 몬스터 존에 소환할 수 없습니다. (카드 타입: {ui.cardData.cardType})");
            
            // 카드를 원래 위치로 되돌리기
            ReturnCardToHand(droppedCard, ui.ownerType);
            return;
        }

        bool isPlayerSlot = CompareTag("PlayerZone");
        bool isEnemySlot = CompareTag("EnemyZone");

        if (isPlayerSlot && ui.ownerType != OwnerType.Player)
        {
            Debug.Log("플레이어 슬롯에는 플레이어 카드만 배치할 수 있습니다.");
            ReturnCardToHand(droppedCard, ui.ownerType);
            return;
        }

        if (isEnemySlot && ui.ownerType != OwnerType.Opponent)
        {
            Debug.Log("적 슬롯에는 적 카드만 배치할 수 있습니다.");
            ReturnCardToHand(droppedCard, ui.ownerType);
            return;
        }

        // 배치
        droppedCard.transform.SetParent(transform, false);
        droppedCard.transform.localPosition = Vector3.zero;
        isOccupied = true;

        dragHandler.droppedOnSlot = true;
        dragHandler.isSummoned = true;

        // 필드 상태 플래그 업데이트
        ui.isOnField = true;

        // 소환 시점 효과 트리거
        var fm = droppedCard.GetComponent<FildMonster>();
        if (fm != null)
        {
            fm.OnPlacedOnField();
        }

        //Image slotImage = GetComponent<Image>();
        //if (slotImage != null)
        //{
        //    slotImage.raycastTarget = false;
        //}
    }

    void Update()
    {
        if (isOccupied && transform.childCount == 0)
        {
            isOccupied = false;
        }

        if (isOccupied && transform.childCount > 0)
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
            {
                isOccupied = false;
            }
        }
    }
}
