using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterSlotDrop : MonoBehaviour, IDropHandler
{
    public bool isOccupied = false;
    public PlayerController_N ownerPlayer;

    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied) return;

        GameObject droppedCard = eventData.pointerDrag;
        if (droppedCard == null) return;

        var dragHandler = droppedCard.GetComponent<CardDragHandler>();
        if (dragHandler == null) return;

        // 필드에 이미 소환된 카드면 드롭 금지
        if (dragHandler.isSummoned)
        {
            Debug.Log("이 카드는 이미 필드에 존재합니다. 다른 슬롯으로 이동할 수 없습니다.");
            return;
        }

        //  유효한 슬롯일 경우 카드 배치
        droppedCard.transform.SetParent(transform, false);
        droppedCard.transform.localPosition = Vector3.zero;
        isOccupied = true;

        // 카드 드래그 상태 갱신
        dragHandler.droppedOnSlot = true;
        dragHandler.isSummoned = true; //  이제 필드에 존재함
    }

    void Update()
    {
        // 슬롯이 비어 있으면 상태 초기화
        if (isOccupied && transform.childCount == 0)
        {
            isOccupied = false;
        }

        // 슬롯에 자식이 있지만 비활성화된 경우도 처리
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
