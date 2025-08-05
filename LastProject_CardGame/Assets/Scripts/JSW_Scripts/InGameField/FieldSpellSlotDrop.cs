using UnityEngine;
using UnityEngine.EventSystems;

public class FieldSpellSlotDrop : MonoBehaviour, IDropHandler
{
    public bool isOccupied = false;

    // FieldSpellSlotDrop.cs의 OnDrop 메서드 수정
    public void OnDrop(PointerEventData eventData)
    {
        if (isOccupied)
        {
            Debug.Log("이미 필드 마법이 발동 중입니다.");
            return;
        }

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        // CardUI를 통해 카드 데이터 접근
        var cardUI = dropped.GetComponent<CardUI>();
        if (cardUI == null || cardUI.cardData == null)
        {
            Debug.Log("cardUI // 유효한 카드가 아닙니다.");
            return;
        }
        var cardUI_N = dropped.GetComponent<CardUI_N>();
        if (cardUI_N == null)
        {
            Debug.Log("cardUI_N // 유효한 카드가 아닙니다.");
            return;
        }

        var cardData = cardUI.cardData;

        // 카드 타입 확인
        if (cardData.cardType != CardType.Spell)
        {
            Debug.Log("이 슬롯에는 마법 카드만 배치할 수 있습니다.");
            return;
        }

        // 필드 마법인지 확인
        if (cardData is SpellCardData spellData)
        {
            if (spellData.spellType != SpellType.Field)
            {
                Debug.Log("이 슬롯에는 필드 마법만 배치할 수 있습니다.");
                return;
            }
        }
        else
        {
            Debug.Log("SpellCardData 캐스팅 실패");
            return;
        }

        // FieldSpellZone 컴포넌트 찾기
        var fieldSpellZone = GetComponentInParent<FieldSpellZone>();
        if (fieldSpellZone == null)
        {
            Debug.LogError("FieldSpellZone을 찾을 수 없습니다!");
            return;
        }

        // 필드 마법 발동
        fieldSpellZone.OnCardDropped(dropped);
        isOccupied = true;

        // 드래그 핸들러 업데이트
        var dragHandler = dropped.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.droppedOnSlot = true;
        }
    }
}
