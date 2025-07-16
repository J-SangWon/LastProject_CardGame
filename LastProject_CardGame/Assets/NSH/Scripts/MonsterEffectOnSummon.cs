using UnityEngine;

public class MonsterEffectOnSummon : MonoBehaviour
{
    private bool effectActivated = false;

    public CardManager_test cardManager;

    public void OnSummon()
    {
        if (effectActivated) return;

        Debug.Log($"{gameObject.name} 소환됨! 드로우 효과 발동");

        if (cardManager != null)
        {
            cardManager.DrawCard(); // 카드 1장 드로우
        }
        else
        {
            Debug.LogWarning("CardManager가 할당되지 않았습니다.");
        }

        effectActivated = true;
    }
}
