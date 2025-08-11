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
        // 덱존 카드면 드래그 시작 차단
        if (transform.parent == PlayerCardManager.Instance.deckZone)
        {
            Debug.Log("덱존에 있는 카드는 드래그할 수 없습니다.");
            return;
        }

        // 턴 체크 및 소환 여부 체크
        if (cardOwner == Owner.Player && !GameManager.Instance.IsPlayerTurn())
        {
            Debug.Log("당신의 턴이 아닙니다. 드래그 불가.");
            return;
        }

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
        if (transform.parent == PlayerCardManager.Instance.deckZone)
            return; // 덱존 카드 드래그 무시

        if (cardOwner == Owner.Player &&
            (!GameManager.Instance.IsPlayerTurn() || GameManager.Instance.CurrentPhase != GamePhase.MainPhase))
        {
            return;
        }

        if (isSummoned) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == PlayerCardManager.Instance.deckZone)
            return; // 덱존 카드 드래그 무시

        canvasGroup.blocksRaycasts = true;

        if (cardOwner == Owner.Player && GameManager.Instance.CurrentPhase != GamePhase.MainPhase)
        {
            Debug.Log("메인 페이즈가 아니므로 소환할 수 없습니다.");
            ReturnToOriginalPosition();
            return;
        }

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

                var fildMonster = GetComponent<FildMonster>();
                if (fildMonster != null)
                {
                    fildMonster.OnPlacedOnField();
                }

                Debug.Log($"{gameObject.name} 이(가) 필드에 소환됨.");
            }
        }

        if (!validDrop)
        {
            ReturnToOriginalPosition();
        }

        droppedOnSlot = false;
    }

    private void ReturnToOriginalPosition()
    {
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
        {
            cardUI.isOnField = false;
        }

        Debug.Log($"{gameObject.name} 소환 해제됨. 다시 드래그 가능.");
    }
}