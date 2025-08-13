using UnityEngine;
using UnityEngine.EventSystems;

public class FieldSpellSlotDrop : MonoBehaviour, IDropHandler
{
    // 드래그-드롭 기반 흐름은 더 이상 사용하지 않습니다. (클릭 기반으로 전환)
    public bool isOccupied = false; // 남겨두되, 실제 점유 상태는 FieldSpellZone이 관리

    public void OnDrop(PointerEventData eventData)
    {
        // No-Op: 클릭 기반 전환으로 드롭은 처리하지 않습니다.
        Debug.Log("[FieldSpellSlotDrop] Drag&Drop은 비활성화되었습니다. 필드마법 존을 클릭하여 선택된 카드를 발동하세요.");
    }
}
