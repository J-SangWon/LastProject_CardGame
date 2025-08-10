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

        // 소유자와 슬롯 태그(PlayerZone/EnemyZone) 매칭 검사
        var ui = droppedCard.GetComponent<CardUI>();
        if (ui == null)
        {
            Debug.LogWarning("[MonsterSlotDrop] CardUI가 없는 오브젝트입니다.");
            return;
        }

        bool isPlayerSlot = CompareTag("PlayerZone");
        bool isEnemySlot = CompareTag("EnemyZone");

        if (isPlayerSlot && ui.ownerType != OwnerType.Player)
        {
            Debug.Log("플레이어 슬롯에는 플레이어 카드만 배치할 수 있습니다.");
            return;
        }

        if (isEnemySlot && ui.ownerType != OwnerType.Opponent)
        {
            Debug.Log("적 슬롯에는 적 카드만 배치할 수 있습니다.");
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
