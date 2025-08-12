using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카드 개체에 부착되어, 오라(지속효과)로 인한 능력치 변화를 소스별로 관리합니다.
/// CardUI는 수정하지 않고, 이 컴포넌트가 CardUI.AddAttack() 등을 호출합니다.
/// </summary>
[DisallowMultipleComponent]
public class AuraTracker : MonoBehaviour
{
    private CardUI cardUI;

    // sourceId(소스 카드 인스턴스ID) -> 적용량
    private readonly Dictionary<int, int> attackBySource = new Dictionary<int, int>();

    private void Awake()
    {
        cardUI = GetComponent<CardUI>();
        if (cardUI == null)
        {
            Debug.LogError("AuraTracker requires CardUI on the same GameObject.");
        }
    }

    public bool HasAttackAuraFrom(int sourceId)
    {
        return attackBySource.ContainsKey(sourceId);
    }

    public void ApplyAttackAura(int sourceId, int amount)
    {
        if (cardUI == null) return;
        // 같은 소스에서 여러 번 등록되면 누적 적용(스택 허용)
        if (attackBySource.TryGetValue(sourceId, out var accumulated))
        {
            accumulated += amount;
            attackBySource[sourceId] = accumulated;
            cardUI.AddAttack(amount);
        }
        else
        {
            attackBySource[sourceId] = amount;
            cardUI.AddAttack(amount);
        }
    }

    public void RemoveAttackAura(int sourceId)
    {
        if (cardUI == null) return;
        if (!attackBySource.TryGetValue(sourceId, out var amount)) return;

        // 원복
        cardUI.AddAttack(-amount);
        attackBySource.Remove(sourceId);
    }

    public void RemoveAllAuras()
    {
        if (cardUI == null) return;
        foreach (var kv in attackBySource)
        {
            cardUI.AddAttack(-kv.Value);
        }
        attackBySource.Clear();
    }
}
