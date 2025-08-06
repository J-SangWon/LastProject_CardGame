using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
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
        transform.SetParent(canvas.transform);
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

        StartCoroutine(EnableRaycastNextFrame());

        bool validDrop = false;

        if (eventData.pointerEnter != null)
        {
            Transform dropZone = eventData.pointerEnter.transform;
            if (IsValidDropZone(dropZone))
            {
                validDrop = true;
                transform.SetParent(dropZone);
                transform.SetAsLastSibling();

                isSummoned = true;
                droppedOnSlot = true;

                CardUI cardUI = GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.isOnField = true;
                }

                Debug.Log($"{gameObject.name} 이(가) 필드에 소환됨.");
            }
        }

        if (!validDrop)
        {
            transform.SetParent(originalParent);
            transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(originalParent.GetComponent<RectTransform>());
        }

        droppedOnSlot = false;
    }

    private IEnumerator EnableRaycastNextFrame()
    {
        yield return null; // 한 프레임 대기
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            Debug.Log("blocksRaycasts = true (한 프레임 뒤에 적용됨)");
        }
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

        // 필드 상태도 해제
        CardUI cardUI = GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.isOnField = false;
        }

        Debug.Log($"{gameObject.name} 소환 해제됨. 다시 드래그 가능.");
    }
}
