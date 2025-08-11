using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIFLipper : MonoBehaviour
{
    public Image[] illustrations; // UI Image 배열
    public float flipDuration = 1.0f; // 회전 시간
    public float waitTime = 1.0f; // 다음 전환 전 대기 시간

    private int currentIndex = 0;
    private Coroutine flipCoroutine;

    void OnEnable()
    {
        ResetAnimation();
        flipCoroutine = StartCoroutine(FlipRoutine());
    }

    void OnDisable()
    {
        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);
    }

    void ResetAnimation()
    {
        currentIndex = 0;

        // 모든 이미지 초기화 (첫 장만 보이게)
        for (int i = 0; i < illustrations.Length; i++)
        {
            Color c = illustrations[i].color;
            c.a = (i == 0) ? 1f : 0f;
            illustrations[i].color = c;
            illustrations[i].rectTransform.localRotation = Quaternion.identity;
        }
    }

    IEnumerator FlipRoutine()
    {
        while (true)
        {
            int nextIndex = (currentIndex + 1) % illustrations.Length;
            yield return StartCoroutine(FlipTransition(currentIndex, nextIndex));
            currentIndex = nextIndex;
            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator FlipTransition(int fromIndex, int toIndex)
    {
        float elapsed = 0f;

        illustrations[fromIndex].rectTransform.localRotation = Quaternion.identity;
        illustrations[toIndex].rectTransform.localRotation = Quaternion.Euler(0, -180, 0);

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;

            if (t < 0.5f)
            {
                float rotY = Mathf.Lerp(0, 90, t * 2f);
                illustrations[fromIndex].rectTransform.localRotation = Quaternion.Euler(0, rotY, 0);

                Color c = illustrations[fromIndex].color;
                c.a = Mathf.Lerp(1f, 0f, t * 2f);
                illustrations[fromIndex].color = c;
            }
            else
            {
                float rotY = Mathf.Lerp(-90, 0, (t - 0.5f) * 2f);
                illustrations[toIndex].rectTransform.localRotation = Quaternion.Euler(0, rotY, 0);

                Color c = illustrations[toIndex].color;
                c.a = Mathf.Lerp(0f, 1f, (t - 0.5f) * 2f);
                illustrations[toIndex].color = c;
            }

            yield return null;
        }

        // 최종 상태 고정
        Color fromC = illustrations[fromIndex].color;
        fromC.a = 0f;
        illustrations[fromIndex].color = fromC;
        illustrations[fromIndex].rectTransform.localRotation = Quaternion.identity;

        Color toC = illustrations[toIndex].color;
        toC.a = 1f;
        illustrations[toIndex].color = toC;
        illustrations[toIndex].rectTransform.localRotation = Quaternion.identity;
    }
}
