using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CardPackViewController : MonoBehaviour
{
    [Header("스크롤 설정")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("카드 크기 조절")]
    public float effectRange = 300f;
    public float maxScale = 1f;
    public float minScale = 0.7f;

    [Header("스냅 설정")]
    public float snapSpeed = 20f;                // SmoothDamp 빠르기
    public float velocityThreshold = 30f;        // 드래그 속도 감지 임계값
    public float snapSmoothTime = 0.1f;          // Snap 부드럽게 이동할 시간
    public Action onDragStart;
    public Action onSnapEnd;

    [Header("선택된 카드")]
    public CardPackView selectedCardPackView;

    private Coroutine snapCoroutine;
    private bool isDragging = false;
    private Vector3 viewportCenterWorld;

    void Start()
    {
        // ScrollRect 뷰포트의 중앙을 월드좌표로 계산 (변하지 않음)
        viewportCenterWorld = scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.center);
    }

    void Update()
    {
        float closestDistance = float.MaxValue;
        CardPackView closestCard = null;

        // 카드 스케일 조절 및 중심 카드 탐색
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform card = content.GetChild(i) as RectTransform;
            Vector3 cardCenter = card.TransformPoint(card.rect.center);
            float distance = Mathf.Abs(viewportCenterWorld.x - cardCenter.x);

            float t = Mathf.Clamp01(distance / effectRange);
            float scale = Mathf.Lerp(maxScale, minScale, t);
            card.localScale = Vector3.one * scale;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCard = card.GetComponent<CardPackView>();
            }
        }

        if (closestCard != null)
            selectedCardPackView = closestCard;

        // 스냅 시작 조건: 드래그 중 아님 + 속도가 느림 + 스냅 중이 아님
        if (!isDragging && scrollRect.velocity.magnitude < velocityThreshold && snapCoroutine == null)
        {
            snapCoroutine = StartCoroutine(SnapToCenter(selectedCardPackView));
        }

        // 드래그 중이면 스냅 중단
        if (scrollRect.velocity.magnitude > velocityThreshold)
        {
            isDragging = true;

            onDragStart?.Invoke();

            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
            }
        }
        else if (isDragging && scrollRect.velocity.magnitude < velocityThreshold)
        {
            isDragging = false;
        }
    }

    /// <summary>
    /// 선택된 카드를 ScrollRect의 가운데로 스냅시킴
    /// </summary>
    IEnumerator SnapToCenter(CardPackView targetCard)
    {
        yield return null; // 1프레임 대기 (안정성용)

        Vector3 cardWorldPos = targetCard.transform.TransformPoint(targetCard.GetComponent<RectTransform>().rect.center);
        float diffX = cardWorldPos.x - viewportCenterWorld.x;
        float targetX = scrollRect.content.anchoredPosition.x - diffX;

        Vector2 velocity = Vector2.zero;

        while (Mathf.Abs(scrollRect.content.anchoredPosition.x - targetX) > 0.01f)
        {
            float newX = Mathf.SmoothDamp(
                scrollRect.content.anchoredPosition.x,
                targetX,
                ref velocity.x,
                snapSmoothTime
            );

            scrollRect.content.anchoredPosition = new Vector2(newX, scrollRect.content.anchoredPosition.y);
        }

        scrollRect.content.anchoredPosition = new Vector2(targetX, scrollRect.content.anchoredPosition.y);
        snapCoroutine = null;

        onSnapEnd?.Invoke();
    }
}
