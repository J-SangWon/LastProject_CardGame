using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Owner { Player, Enemy }
    public Owner cardOwner;

    public bool isSummoned = false;
    public bool droppedOnSlot = false;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private CardUI cardUI;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardUI = GetComponent<CardUI>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PlayerCardManager.Instance != null &&
            transform.parent == PlayerCardManager.Instance.deckZone)
            return;

        if (cardOwner == Owner.Player &&
            !(GameManager.Instance?.IsPlayerTurn() ?? false))
            return;

        if (isSummoned)
            return;

        if (cardOwner == Owner.Player && cardUI != null &&
            !CostManager.Instance.CanSpendPlayerCost(cardUI.cardData.cost))
            return;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        if (canvas != null)
            transform.SetParent(canvas.transform, false);
        else
            transform.SetParent(originalParent, true);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PlayerCardManager.Instance != null &&
            transform.parent == PlayerCardManager.Instance.deckZone)
            return;

        if (cardOwner == Owner.Player &&
            (!GameManager.Instance?.IsPlayerTurn() ?? true ||
             GameManager.Instance.CurrentPhase != GamePhase.MainPhase))
            return;

        if (isSummoned)
            return;

        rectTransform.anchoredPosition += eventData.delta / (canvas != null ? canvas.scaleFactor : 1f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (PlayerCardManager.Instance != null &&
            transform.parent == PlayerCardManager.Instance.deckZone)
            return;

        canvasGroup.blocksRaycasts = true;

        if (cardOwner == Owner.Player &&
            GameManager.Instance != null &&
            GameManager.Instance.CurrentPhase != GamePhase.MainPhase)
        {
            ReturnToOriginalPosition();
            return;
        }

        bool validDrop = false;

        if (eventData.pointerEnter != null)
        {
            Transform dropZone = FindDropZoneTransform(eventData.pointerEnter.transform);

            if (IsValidDropZone(dropZone))
            {
                if (cardOwner == Owner.Player && cardUI != null)
                {
                    int cost = cardUI.cardData.cost;
                    if (!CostManager.Instance.TrySpendPlayerCost(cost))
                    {
                        Debug.Log("소환 실패: 코스트 부족");
                        ReturnToOriginalPosition();
                        return;
                    }
                }

                transform.SetParent(dropZone, false);
                transform.localPosition = Vector3.zero;
                isSummoned = true;
                droppedOnSlot = true;

                if (cardUI != null)
                    cardUI.isOnField = true;

                Debug.Log($"{gameObject.name} 필드에 소환됨.");
                validDrop = true;
            }
        }

        if (!validDrop)
        {
            ReturnToOriginalPosition();
            droppedOnSlot = false;
        }
    }

    private void ReturnToOriginalPosition()
    {
        if (originalParent == null)
            return;

        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);
        isSummoned = false;

        var parentRect = originalParent.GetComponent<RectTransform>();
        if (parentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }

    private Transform FindDropZoneTransform(Transform current)
    {
        while (current != null)
        {
            if (current.CompareTag("PlayerZone") || current.CompareTag("EnemyZone"))
                return current;
            current = current.parent;
        }
        return null;
    }

    private bool IsValidDropZone(Transform dropZone)
    {
        if (dropZone == null) return false;

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
        droppedOnSlot = false;

        if (cardUI != null)
            cardUI.isOnField = false;
    }
}
