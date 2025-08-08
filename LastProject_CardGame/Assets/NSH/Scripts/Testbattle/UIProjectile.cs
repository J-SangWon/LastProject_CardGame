using UnityEngine;
using UnityEngine.UI;

public class UIProjectile : MonoBehaviour
{
    private RectTransform targetUI;
    private RectTransform rectTransform;
    public float speed = 1500f;

    public void Initialize(RectTransform target)
    {
        targetUI = target;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (targetUI == null) return;

        // 방향 및 이동
        Vector2 dir = (targetUI.anchoredPosition - rectTransform.anchoredPosition).normalized;
        rectTransform.anchoredPosition += dir * speed * Time.deltaTime;

        // 도착 판정
        if (Vector2.Distance(rectTransform.anchoredPosition, targetUI.anchoredPosition) < 10f)
        {
            BattleManager_test.Instance.OnProjectileHit();  // 데미지 처리
            Destroy(gameObject);  // 투사체 제거
        }
    }
}
