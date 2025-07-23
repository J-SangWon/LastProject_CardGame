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
    public float effectRange = 100f;
    public float maxScale = 1f;
    public float minScale = 0.7f;

    [Header("스냅 설정")]
    public float velocityThreshold = 30f;
    public float snapSmoothTime = 0.1f;

    [Header("선택된 카드")]
    public CardPackView selectedCardPackView;

    public Action onDragStart;
    public Action onSnapEnd;

    private Coroutine snapCoroutine;
    private bool isDragging = false;
    private bool hasSnapped = false;

    void Update()
    {
        UpdateCardScaleAndSelectClosest();
        HandleSnapping();
    }

    /// <summary>
    /// 카드 크기 조정 및 가장 가까운 카드 선택
    /// </summary>
    void UpdateCardScaleAndSelectClosest()
    {
        float closestDistance = float.MaxValue;
        CardPackView closestCard = null;

        // Viewport 중심의 "로컬 좌표"
        // scrollRect.viewport의 좌표계 기준에서의 중심점
        Vector3 centerLocal = scrollRect.viewport.rect.center;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform card = content.GetChild(i) as RectTransform;

            // 카드의 "월드 좌표"를 Viewport 로컬 기준으로 변환한 값
            // Viewport의 좌표계 안에서 카드가 얼마나 떨어져 있는지를 판단하기 위함
            Vector3 cardLocalPos = scrollRect.viewport.InverseTransformPoint(card.position);

            // 중심과의 거리 계산
            float distance = Mathf.Abs(centerLocal.x - cardLocalPos.x);

            // 거리 비례 스케일 조정 (멀수록 작아짐)
            float t = Mathf.Clamp01(distance / effectRange);
            card.localScale = Vector3.one * Mathf.Lerp(maxScale, minScale, t);

            // 가장 가까운 카드 선택
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCard = card.GetComponent<CardPackView>();
            }
        }

        if (closestCard != null)
            selectedCardPackView = closestCard;
    }

    /// <summary>
    /// 드래그 여부에 따라 스냅 작동 제어
    /// </summary>
    void HandleSnapping()
    {
        float velocity = scrollRect.velocity.magnitude;

        if (velocity > velocityThreshold)
        {
            if (!isDragging)
            {
                isDragging = true;
                hasSnapped = false; // 새 드래그 시작 시 초기화
                onDragStart?.Invoke();
            }

            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
            }
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
            }


            if (!hasSnapped && snapCoroutine == null && selectedCardPackView != null)
            {
                snapCoroutine = StartCoroutine(SnapToCenter(selectedCardPackView));
            }
        }
    }

    /// <summary>
    /// 선택된 카드를 ScrollRect 중심으로 스냅 이동
    /// </summary>
    IEnumerator SnapToCenter(CardPackView targetCard)
    {
        yield return null; // 1프레임 대기

        if (targetCard == null) yield break;

        RectTransform rt = targetCard.GetComponent<RectTransform>();
        if (rt == null) yield break;

        // 카드의 "월드 좌표"를 Viewport의 로컬 좌표로 변환
        // Viewport 중심과의 차이를 계산하기 위해
        Vector3 cardLocalPos = scrollRect.viewport.InverseTransformPoint(rt.position);

        // Viewport 중심의 "로컬 좌표"
        Vector3 centerLocal = scrollRect.viewport.rect.center;

        // Viewport 중심과 카드의 중심 차이 (x축)
        float diffX = cardLocalPos.x - centerLocal.x;

        // 현재 content.anchoredPosition에서 diffX만큼 보정
        // anchoredPosition은 Content의 피벗 기준 위치 (왼쪽 상단이 (0,0)이 아님)
        float targetX = content.anchoredPosition.x - diffX;

        Vector2 velocity = Vector2.zero;

        while (Mathf.Abs(content.anchoredPosition.x - targetX) > 5f)
        {
            float newX = Mathf.SmoothDamp(
                content.anchoredPosition.x,
                targetX,
                ref velocity.x,
                snapSmoothTime
            );

            content.anchoredPosition = new Vector2(newX, content.anchoredPosition.y);

            yield return null;
        }

        // 마지막 보정
        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
        onSnapEnd?.Invoke();
        snapCoroutine = null;
        hasSnapped = true;

    }
}
