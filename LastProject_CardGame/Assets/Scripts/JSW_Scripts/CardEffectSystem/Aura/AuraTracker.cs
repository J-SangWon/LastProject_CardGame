using System.Collections.Generic;
using UnityEngine;

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
