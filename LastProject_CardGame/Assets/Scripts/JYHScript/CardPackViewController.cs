using UnityEngine;
using UnityEngine.UI;

public class CardPackViewController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float effectRange = 300f;
    public float maxScale = 1f;
    public float minScale = 0.7f;
    public float maxAlpha = 1f;
    public float minAlpha = 0.5f;

    void Update()
    {
        Vector3 viewportCenter = scrollRect.viewport.TransformPoint(new Vector3(scrollRect.viewport.rect.width / 2f, 0, 0));

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform card = content.GetChild(i) as RectTransform;

            // 아이템의 월드 기준 중심 위치 계산
            Vector3 cardCenter = card.TransformPoint(card.rect.center);
            float distance = Mathf.Abs(viewportCenter.x - cardCenter.x);
            float t = Mathf.Clamp01(distance / effectRange);

            float scale = Mathf.Lerp(maxScale, minScale, t);
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);

            card.localScale = Vector3.one * scale;

            if (card.TryGetComponent(out CanvasGroup cg))
                cg.alpha = alpha;
        }
    }
}
