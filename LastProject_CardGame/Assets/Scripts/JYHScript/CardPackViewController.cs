using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CardPackViewController : MonoBehaviour
{
    [Header("��ũ�� ����")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("ī�� ũ�� ����")]
    public float effectRange = 300f;
    public float maxScale = 1f;
    public float minScale = 0.7f;

    [Header("���� ����")]
    public float snapSpeed = 20f;                // SmoothDamp ������
    public float velocityThreshold = 30f;        // �巡�� �ӵ� ���� �Ӱ谪
    public float snapSmoothTime = 0.1f;          // Snap �ε巴�� �̵��� �ð�
    public Action onDragStart;
    public Action onSnapEnd;

    [Header("���õ� ī��")]
    public CardPackView selectedCardPackView;

    private Coroutine snapCoroutine;
    private bool isDragging = false;
    private Vector3 viewportCenterWorld;

    void Start()
    {
        // ScrollRect ����Ʈ�� �߾��� ������ǥ�� ��� (������ ����)
        viewportCenterWorld = scrollRect.viewport.TransformPoint(scrollRect.viewport.rect.center);
    }

    void Update()
    {
        float closestDistance = float.MaxValue;
        CardPackView closestCard = null;

        // ī�� ������ ���� �� �߽� ī�� Ž��
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

        // ���� ���� ����: �巡�� �� �ƴ� + �ӵ��� ���� + ���� ���� �ƴ�
        if (!isDragging && scrollRect.velocity.magnitude < velocityThreshold && snapCoroutine == null)
        {
            snapCoroutine = StartCoroutine(SnapToCenter(selectedCardPackView));
        }

        // �巡�� ���̸� ���� �ߴ�
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
    /// ���õ� ī�带 ScrollRect�� ����� ������Ŵ
    /// </summary>
    IEnumerator SnapToCenter(CardPackView targetCard)
    {
        yield return null; // 1������ ��� (��������)

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
