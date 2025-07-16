using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterSlotDrop : MonoBehaviour, IDropHandler
{
    public bool isOccupied = false;

    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied) return;

        GameObject dropped = eventData.pointerDrag;
        if (dropped != null)
        {
            dropped.transform.SetParent(transform, false);
            dropped.transform.localPosition = Vector3.zero;
            isOccupied = true;

            dropped.GetComponent<CardDragHandler>().droppedOnSlot = true;

            // 카드가 몬스터 효과 있으면 실행
            var effect = dropped.GetComponent<MonsterEffectOnSummon>();
            if (effect != null)
            {
                effect.OnSummon();
            }
        }
    }
}
