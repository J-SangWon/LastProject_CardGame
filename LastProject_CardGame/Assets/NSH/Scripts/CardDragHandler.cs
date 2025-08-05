using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Owner { Player, Enemy }
    public Owner cardOwner;

    public bool isSummoned = false;
    public bool droppedOnSlot = false;
    private Transform originalParent;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isSummoned)
        {
            Debug.Log("이 카드는 이미 필드에 소환되어 드래그할 수 없습니다.");
            return;
        }

        originalParent = transform.parent;
        transform.SetParent(canvas.transform); // 드래그 시 캔버스로 옮겨 앞에 보이게
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isSummoned) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isSummoned) return;

        canvasGroup.blocksRaycasts = true;

        bool validDrop = false;

        if (eventData.pointerEnter != null)
        {
            Transform dropZone = eventData.pointerEnter.transform;
            if (IsValidDropZone(dropZone))
            {
                validDrop = true;
                // 정상적으로 드롭된 경우: 드롭 존 쪽에서 처리
            }
        }

        // 유효한 드롭이 아니면 핸드존으로 복귀
        if (!validDrop)
        {
            transform.SetParent(originalParent);
            transform.SetAsLastSibling(); // 핸드존의 LayoutGroup이 자동 정렬되도록

            // Layout 강제 재적용
            LayoutRebuilder.ForceRebuildLayoutImmediate(originalParent.GetComponent<RectTransform>());
        }

        droppedOnSlot = false;
    }

    private bool IsValidDropZone(Transform dropZone)
    {
        string zoneTag = dropZone.tag;

        if (cardOwner == Owner.Player && zoneTag == "PlayerZone")
            return true;

        if (cardOwner == Owner.Enemy && zoneTag == "EnemyZone")
            return true;

        return false;
    }

    public void Unsummon()
    {
        isSummoned = false;
        Debug.Log($"{gameObject.name} 소환 해제됨. 다시 드래그 가능.");
    }
}
