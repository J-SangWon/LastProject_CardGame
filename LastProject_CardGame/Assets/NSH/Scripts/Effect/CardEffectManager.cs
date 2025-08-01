using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 카드 효과들을 관리하는 싱글톤 매니저.
/// 카드 효과 종류별로 처리하며, 대상 지정 효과도 지원.
/// </summary>
public class CardEffectManager : MonoBehaviour
{
    public static CardEffectManager Instance;

    public enum CardEffect
    {
        DrawCard,           // 카드 뽑기
        DestroyCard,        // 자기 카드 파괴
        TargetDestroyCard,  // 대상 카드 파괴 (대상 지정)
        DamageTarget        // 대상 데미지 (대상 지정)
    }

    public List<PlayerController_N> players; // 플레이어 리스트(필요 시)

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 카드 효과 실행 진입점
    /// </summary>
    public void ApplyEffect(PlayerController_N player, CardEffect effect, Action onComplete)
    {
        Debug.Log($"[CardEffectManager] 카드 효과: {effect}");

        switch (effect)
        {
            case CardEffect.DrawCard:
                EffectDrawCard(player, onComplete);
                break;
            case CardEffect.DestroyCard:
                EffectDestroyCard(player, onComplete);
                break;
            case CardEffect.TargetDestroyCard:
            case CardEffect.DamageTarget:
                StartCoroutine(EffectWaitForTarget(player, effect, onComplete));
                break;
            default:
                Debug.LogWarning("[CardEffectManager] 정의되지 않은 카드 효과");
                onComplete?.Invoke();
                break;
        }
    }

    // 카드 뽑기 예시 (CardManager_test 활용)
    private void EffectDrawCard(PlayerController_N player, Action onComplete)
    {
        CardManager_test.Instance.ResolveCard(player, onComplete);
    }

    // 자기 카드 하나 파괴 예시
    private void EffectDestroyCard(PlayerController_N player, Action onComplete)
    {
        if (player.HasCards)
        {
            GameObject card = player.GetCardToDestroy();
            if (card != null)
            {
                Destroy(card);
                Debug.Log($"[CardEffectManager] 카드 파괴: {card.name}");
            }
            else
            {
                Debug.Log("[CardEffectManager] 파괴할 카드 없음");
            }
        }
        else
        {
            Debug.Log("[CardEffectManager] 플레이어 카드 없음");
        }
        onComplete?.Invoke();
    }

    // 대상 지정 카드 효과 처리 코루틴
    private IEnumerator EffectWaitForTarget(PlayerController_N player, CardEffect effect, Action onComplete)
    {
        Debug.Log("[CardEffectManager] 대상 선택 대기 중...");

        TargetSelector.Instance.StartSelecting(target =>
        {
            if (target == null)
            {
                Debug.LogWarning("[CardEffectManager] 대상이 선택되지 않음");
                onComplete?.Invoke();
                return;
            }

            // TargetableCard 컴포넌트 확인 (안 붙어있으면 경고 후 종료)
            TargetableCard targetable = target.GetComponent<TargetableCard>();
            if (targetable == null)
            {
                Debug.LogWarning("[CardEffectManager] 선택 대상에 TargetableCard가 없습니다.");
                onComplete?.Invoke();
                return;
            }

            switch (effect)
            {
                case CardEffect.TargetDestroyCard:
                    Destroy(target);
                    Debug.Log("[CardEffectManager] 대상 카드 파괴 완료");
                    break;

                case CardEffect.DamageTarget:
                    targetable.TakeDamage(500); // 예시: 500 데미지
                    break;
            }

            onComplete?.Invoke();
        });

        yield return null;
    }
}
