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
        // 턴 확인
        if (cardOwner == Owner.Player && !GameManager.Instance.IsPlayerTurn())
        {
            Debug.Log("당신의 턴이 아닙니다. 드래그 불가.");
            return;
        }

        // 이미 소환된 카드면 드래그 금지 (원하면 소환 해제 로직 추가)
        if (isSummoned)
        {
            Debug.Log("이 카드는 이미 필드에 소환되어 드래그할 수 없습니다.");
            return;
        }

        // 원래 부모(슬롯 등) 저장
        originalParent = transform.parent;

        // 원래 부모가 슬롯이면 비우기 표시
        var origSlot = originalParent?.GetComponent<Slot>();
        if (origSlot != null) origSlot.isOccupied = false;

        // 드래그 중에는 캔버스 최상위로 올려서 raycast 충돌 방지
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 메인페이즈 + 내 턴 체크 (프로젝트의 조건에 맞게 수정)
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

        // 메인 페이즈가 아닐 경우 소환 금지
        if (cardOwner == Owner.Player && GameManager.Instance.CurrentPhase != GamePhase.MainPhase)
        {
            Debug.Log("메인 페이즈가 아니므로 소환할 수 없습니다.");
            ReturnToOriginalPosition();
            return;
        }

        bool validDrop = false;

        if (eventData.pointerEnter != null)
        {
            // 1) pointerEnter가 슬롯 내부/자식인지 확인 -> Slot 컴포넌트를 우선 찾음
            Slot slotHit = eventData.pointerEnter.GetComponentInParent<Slot>();
            if (slotHit != null)
            {
                if (!slotHit.isOccupied)
                {
                    PlaceInSlot(slotHit.transform);
                    validDrop = true;
                }
                else
                {
                    Debug.Log("해당 슬롯은 이미 점유되어 있습니다.");
                }
            }
            else
            {
                // 2) 슬롯이 아닌 경우, pointerEnter가 속한 zone(부모 HorizontalLayoutGroup) 확인
                var zoneLayout = eventData.pointerEnter.GetComponentInParent<HorizontalLayoutGroup>();
                if (zoneLayout != null)
                {
                    Transform zone = zoneLayout.transform;
                    // (선택) zone 태그로 플레이어/적 영역 판단 가능
                    // Find nearest available slot under this zone
                    Transform closest = FindClosestAvailableSlot(zone, eventData);
                    if (closest != null)
                    {
                        PlaceInSlot(closest);
                        validDrop = true;
                    }
                    else
                    {
                        Debug.Log("빈 슬롯이 없습니다.");
                    }
                }
            }
        }

        if (!validDrop)
        {
            ReturnToOriginalPosition();
        }

        droppedOnSlot = false;
    }

    private Transform FindClosestAvailableSlot(Transform zone, PointerEventData eventData)
    {
        Slot[] slots = zone.GetComponentsInChildren<Slot>(true);
        Transform best = null;
        float bestDist = float.MaxValue;
        Vector2 pointerPos = eventData.position; // screen position

        foreach (var s in slots)
        {
            if (s.isOccupied) continue;
            RectTransform rt = s.GetComponent<RectTransform>();
            if (rt == null) continue;
            Vector2 slotScreen = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rt.position);
            float dist = Vector2.Distance(slotScreen, pointerPos);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = rt.transform;
            }
        }
        return best;
    }

    private void PlaceInSlot(Transform slot)
    {
        // 슬롯의 자식으로 넣기 (local position 0)
        transform.SetParent(slot, false);
        transform.localPosition = Vector3.zero;
        transform.SetAsLastSibling();

        // 상태 갱신
        isSummoned = true;
        droppedOnSlot = true;

        CardUI cardUI = GetComponent<CardUI>();
        if (cardUI != null) cardUI.isOnField = true;

        // 슬롯 점유 표시
        Slot s = slot.GetComponent<Slot>();
        if (s != null) s.isOccupied = true;

        // 소환 시 효과 호출(필요하면)
        var fildMonster = GetComponent<FildMonster>();
        if (fildMonster != null) fildMonster.OnPlacedOnField();

        Debug.Log($"{gameObject.name} 이(가) 슬롯 '{slot.name}' 에 소환됨.");
    }

    private void ReturnToOriginalPosition()
    {
        // 원래 부모로 복귀 + 레이아웃 강제 갱신
        transform.SetParent(originalParent, false);
        transform.SetAsLastSibling();

        var rt = originalParent.GetComponent<RectTransform>();
        if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        // 원래 부모가 슬롯이면 점유 표시 복구
        var slot = originalParent.GetComponent<Slot>();
        if (slot != null) slot.isOccupied = true;
    }

    public void Unsummon()
    {
        isSummoned = false;

        CardUI cardUI = GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.isOnField = false;
        }

        // 슬롯 비우기 (현재 부모가 슬롯이면)
        var slot = transform.parent.GetComponent<Slot>();
        if (slot != null) slot.isOccupied = false;

        Debug.Log($"{gameObject.name} 소환 해제됨. 다시 드래그 가능.");
    }
}
