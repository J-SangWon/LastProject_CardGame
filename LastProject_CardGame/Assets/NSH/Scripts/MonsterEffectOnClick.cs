using UnityEngine;

public class MonsterEffectOnClick : MonoBehaviour
{
    // 중복 발동 방지용 플래그
    private bool effectActivated = false;

    // 카드 매니저 참조 (카드 드로우 기능을 위해 필요)
    public CardManager_test cardManager;

    // 카드가 클릭될 때 발동하는 함수
    private void OnMouseDown()
    {
        // 이미 효과가 발동되었으면 무시
        if (effectActivated)
        {
            Debug.Log($"{gameObject.name}의 효과는 이미 발동되었습니다.");
            return;
        }

        Debug.Log($"{gameObject.name} 클릭됨! 효과 발동");
        ActivateEffect(); // 클릭 시 효과 발동
    }

    // 실제 효과 발동 → 드로우 처리
    private void ActivateEffect()
    {
        Debug.Log($"{gameObject.name}의 클릭 효과 발동: 드로우!");

        if (cardManager != null)
        {
            cardManager.DrawCard(); // 카드 드로우 실행
        }
        else
        {
            Debug.LogWarning("CardManager가 설정되지 않았습니다.");
        }

        effectActivated = true; // 중복 발동 방지
    }
}
