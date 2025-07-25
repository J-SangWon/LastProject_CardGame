using UnityEngine;
using UnityEngine.EventSystems;

public class eCardDraghandler : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 originalPos;

    public RectTransform[] enemyZones; // Inspector에서 연결 예정
    public Camera uiCamera;            // Canvas가 Screen Space - Camera인 경우

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPos = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        bool droppedOnEnemyZone = false;

        foreach (RectTransform zone in enemyZones)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(zone, Input.mousePosition, uiCamera))
            {
                droppedOnEnemyZone = true;
                break;
            }
        }

        if (droppedOnEnemyZone)
        {
            gameObject.tag = "Enemy";
            Debug.Log($"{gameObject.name}이(가) 적 몬스터 존에 놓였습니다! 태그: Enemy");
        }
        else
        {
            gameObject.tag = "Untagged";
            Debug.Log($"{gameObject.name}이(가) 몬스터 존에 놓이지 않았습니다. 태그 초기화");
            rectTransform.anchoredPosition = originalPos; // 원래 위치로 복귀
        }
    }
}
