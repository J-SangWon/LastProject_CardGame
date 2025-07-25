using UnityEngine;
using UnityEngine.EventSystems;

public class eCardDraghandler : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private Vector2 originalPos;

    public RectTransform[] enemyZones; // Inspector���� ���� ����
    public Camera uiCamera;            // Canvas�� Screen Space - Camera�� ���

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
            Debug.Log($"{gameObject.name}��(��) �� ���� ���� �������ϴ�! �±�: Enemy");
        }
        else
        {
            gameObject.tag = "Untagged";
            Debug.Log($"{gameObject.name}��(��) ���� ���� ������ �ʾҽ��ϴ�. �±� �ʱ�ȭ");
            rectTransform.anchoredPosition = originalPos; // ���� ��ġ�� ����
        }
    }
}
