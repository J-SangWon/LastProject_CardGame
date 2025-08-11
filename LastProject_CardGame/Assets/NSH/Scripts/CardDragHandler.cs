using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Kalkatos.DottedArrow;

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
    private CardUI cardUI;
    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent == PlayerCardManager.Instance.deckZone)
            return;

        if (cardOwner == Owner.Player && !GameManager.Instance.IsPlayerTurn())
            return;

        if (isSummoned)
            return;

        var cardUI = GetComponent<CardUI>();
        if (cardOwner == Owner.Player && cardUI != null &&
            !CostManager.Instance.CanSpendPlayerCost(cardUI.cardData.cost))
            return;


        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (transform.parent == PlayerCardManager.Instance.deckZone)
            return;

        if (cardOwner == Owner.Player &&
            (!GameManager.Instance.IsPlayerTurn() || GameManager.Instance.CurrentPhase != GamePhase.MainPhase))
            return;

        if (isSummoned)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == PlayerCardManager.Instance.deckZone)
            return;

        canvasGroup.blocksRaycasts = true;

        if (cardOwner == Owner.Player && GameManager.Instance.CurrentPhase != GamePhase.MainPhase)
        {
            ReturnToOriginalPosition();
            return;
        }


        bool validDrop = false;

        if (eventData.pointerEnter != null)
        {
            Transform dropZone = eventData.pointerEnter.transform;
            if (IsValidDropZone(dropZone))
            {
                // 소환 시 코스트 차감 시도
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

                validDrop = true;
                transform.SetParent(dropZone);
                transform.SetAsLastSibling();
                isSummoned = true;
                droppedOnSlot = true;

                if (cardUI != null)
                    cardUI.isOnField = true;

                var fieldMonster = GetComponent<FildMonster>();
                if (fieldMonster != null)
                    fieldMonster.OnPlacedOnField();

                Debug.Log($"{gameObject.name} 필드에 소환됨.");
            }
        }

        if (!validDrop)
            ReturnToOriginalPosition();

        droppedOnSlot = false;
    }

    private void ReturnToOriginalPosition()
    {
        if (originalParent == null)
        {
            return;
        }
        transform.SetParent(originalParent);
        transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(originalParent.GetComponent<RectTransform>());
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

        CardUI cardUI = GetComponent<CardUI>();
        if (cardUI != null)
            cardUI.isOnField = false;
    }
}
