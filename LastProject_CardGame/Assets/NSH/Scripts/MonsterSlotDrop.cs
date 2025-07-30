using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterSlotDrop : MonoBehaviour, IDropHandler
{
    public bool isOccupied = false;

    // 이 슬롯에 소속된 플레이어(필요하면 할당)
    public PlayerController_N ownerPlayer;

    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied) return;

        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            dropped.transform.SetParent(transform, false);
            dropped.transform.localPosition = Vector3.zero;
            isOccupied = true;

            var dragHandler = dropped.GetComponent<CardDragHandler>();
            if (dragHandler != null)
                dragHandler.droppedOnSlot = true;

            // 몬스터 소환 효과를 바로 SetEffect로 호출 (예: 데미지 3, 플레이어 전달)
            var effect = dropped.GetComponent<MonsterEffectOnSummon>();
            if (effect != null)
            {
                effect.SetEffect(CardEffectType.DealDamageToTargetOnSummon, 3, ownerPlayer);
            }

            // 카드가 파괴되면 슬롯 비우기
            var targetable = dropped.GetComponent<TargetableCard>();
            if (targetable != null)
            {
                targetable.OnDestroyed += () => { isOccupied = false; };
            }
        }
    }
}
