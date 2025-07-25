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
    public float effectRange = 100f;
    public float maxScale = 1f;
    public float minScale = 0.7f;

    [Header("���� ����")]
    public float velocityThreshold = 30f;
    public float snapSmoothTime = 0.1f;

    [Header("���õ� ī��")]
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
    /// ī�� ũ�� ���� �� ���� ����� ī�� ����
    /// </summary>
    void UpdateCardScaleAndSelectClosest()
    {
        float closestDistance = float.MaxValue;
        CardPackView closestCard = null;

        // Viewport �߽��� "���� ��ǥ"
        // scrollRect.viewport�� ��ǥ�� ���ؿ����� �߽���
        Vector3 centerLocal = scrollRect.viewport.rect.center;

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform card = content.GetChild(i) as RectTransform;

            // ī���� "���� ��ǥ"�� Viewport ���� �������� ��ȯ�� ��
            // Viewport�� ��ǥ�� �ȿ��� ī�尡 �󸶳� ������ �ִ����� �Ǵ��ϱ� ����
            Vector3 cardLocalPos = scrollRect.viewport.InverseTransformPoint(card.position);

            // �߽ɰ��� �Ÿ� ���
            float distance = Mathf.Abs(centerLocal.x - cardLocalPos.x);

            // �Ÿ� ��� ������ ���� (�ּ��� �۾���)
            float t = Mathf.Clamp01(distance / effectRange);
            card.localScale = Vector3.one * Mathf.Lerp(maxScale, minScale, t);

            // ���� ����� ī�� ����
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
    /// �巡�� ���ο� ���� ���� �۵� ����
    /// </summary>
    void HandleSnapping()
    {
        float velocity = scrollRect.velocity.magnitude;

        if (velocity > velocityThreshold)
        {
            if (!isDragging)
            {
                isDragging = true;
                hasSnapped = false; // �� �巡�� ���� �� �ʱ�ȭ
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
    /// ���õ� ī�带 ScrollRect �߽����� ���� �̵�
    /// </summary>
    IEnumerator SnapToCenter(CardPackView targetCard)
    {
        yield return null; // 1������ ���

        if (targetCard == null) yield break;

        RectTransform rt = targetCard.GetComponent<RectTransform>();
        if (rt == null) yield break;

        // ī���� "���� ��ǥ"�� Viewport�� ���� ��ǥ�� ��ȯ
        // Viewport �߽ɰ��� ���̸� ����ϱ� ����
        Vector3 cardLocalPos = scrollRect.viewport.InverseTransformPoint(rt.position);

        // Viewport �߽��� "���� ��ǥ"
        Vector3 centerLocal = scrollRect.viewport.rect.center;

        // Viewport �߽ɰ� ī���� �߽� ���� (x��)
        float diffX = cardLocalPos.x - centerLocal.x;

        // ���� content.anchoredPosition���� diffX��ŭ ����
        // anchoredPosition�� Content�� �ǹ� ���� ��ġ (���� ����� (0,0)�� �ƴ�)
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

        // ������ ����
        content.anchoredPosition = new Vector2(targetX, content.anchoredPosition.y);
        onSnapEnd?.Invoke();
        snapCoroutine = null;
        hasSnapped = true;

    }
}
