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
        //  턴 확인: 내 턴이 아니라면 드래그 금지
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
        //  메인페이즈 + 내 턴이 아닌 경우 금지
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
        canvasGroup.blocksRaycasts = true;

        //  메인페이즈가 아닐 경우 소환 금지
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

                // 소환 시점 효과 트리거
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