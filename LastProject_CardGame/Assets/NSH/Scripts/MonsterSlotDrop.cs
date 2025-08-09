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

        // 필드 상태 플래그 업데이트
        var ui = droppedCard.GetComponent<CardUI>();
        if (ui != null)
        {
            ui.isOnField = true;
        }

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
        // ������ ��� ������ ���� �ʱ�ȭ
        if (isOccupied && transform.childCount == 0)
        {
            isOccupied = false;
        }

        // ���Կ� �ڽ��� ������ ��Ȱ��ȭ�� ��쵵 ó��
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
