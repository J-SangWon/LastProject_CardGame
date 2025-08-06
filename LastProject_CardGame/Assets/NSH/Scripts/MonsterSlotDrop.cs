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

        droppedCard.transform.SetParent(transform, false);
        droppedCard.transform.localPosition = Vector3.zero;
        isOccupied = true;

        dragHandler.droppedOnSlot = true;
        dragHandler.isSummoned = true;

        //Image slotImage = GetComponent<Image>();
        //if (slotImage != null)
        //{
        //    slotImage.raycastTarget = false;
        //}
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
