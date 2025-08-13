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
    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private CardUI cardUI;
    private Vector2 pointerOffsetInRoot; // 루트 캔버스 좌표계에서의 포인터 오프셋

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardUI = GetComponent<CardUI>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PlayerCardManager.Instance != null &&
            transform.parent == PlayerCardManager.Instance.playerDeckZone)
            return;

        if (cardOwner == Owner.Player &&
            !(GameManager.Instance?.IsPlayerTurn() ?? false))
            return;

        if (isSummoned)
            return;

        if (cardOwner == Owner.Player && cardUI != null &&
            !(GameManager.Instance?.CanSpendPlayerCost(cardUI.cardData.cost) ?? false))
            return;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 드래그 시작 시 루트 캔버스로 옮기되, 월드 좌표 유지하여 점프 방지
        if (rootCanvas != null)
            transform.SetParent(rootCanvas.transform, true);
        else if (canvas != null)
            transform.SetParent(canvas.transform, true);
        else
            transform.SetParent(originalParent, true);

        // 포인터와 카드 중심의 오프셋을 루트 캔버스 좌표로 계산
        if (rootCanvas != null)
        {
            RectTransform rootRect = rootCanvas.transform as RectTransform;
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            // 포인터의 루트 캔버스 로컬 좌표
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, eventData.position, cam, out var pointerLocal);

            // 카드의 루트 캔버스 로컬 좌표
            Vector2 cardScreenPos = RectTransformUtility.WorldToScreenPoint(cam, rectTransform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, cardScreenPos, cam, out var cardLocal);

            pointerOffsetInRoot = cardLocal - pointerLocal;
        }

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PlayerCardManager.Instance != null &&
            transform.parent == PlayerCardManager.Instance.playerDeckZone)
            return;

        if (cardOwner == Owner.Player &&
            (!GameManager.Instance?.IsPlayerTurn() ?? true ||
             GameManager.Instance.CurrentPhase != GamePhase.MainPhase))
            return;

        if (isSummoned)
            return;

        if (rootCanvas != null)
        {
            RectTransform rootRect = rootCanvas.transform as RectTransform;
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            // 현재 포인터를 루트 캔버스 로컬 좌표로 변환
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, eventData.position, cam, out var pointerLocal))
            {
                // 오프셋을 적용한 위치로 이동
                var targetLocal = pointerLocal + pointerOffsetInRoot;
                rectTransform.anchoredPosition = targetLocal;
            }
        }
        else
        {
            // 폴백: 기존 방식 유지
            rectTransform.anchoredPosition += eventData.delta / (canvas != null ? canvas.scaleFactor : 1f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (PlayerCardManager.Instance != null &&
            transform.parent == PlayerCardManager.Instance.playerDeckZone)
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
                    if (!(GameManager.Instance?.SpendPlayerCost(cost) ?? false))
                    {
                        Debug.Log("소환 실패: 코스트 부족");
                        ReturnToOriginalPosition();
                        return;
                    }
                }

                // 소환 성공 처리 ...
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
