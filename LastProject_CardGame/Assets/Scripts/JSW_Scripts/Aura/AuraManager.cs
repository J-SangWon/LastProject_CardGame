using System.Collections.Generic;
using UnityEngine;

public enum OwnerScope
{
    Self,
    Opponent,
    Both
}

/// <summary>
/// 지속효과(오라)를 등록/해제하고, 대상에게 적용/원복하는 정적 매니저.
/// - 같은 소스(카드 인스턴스)에서 동일 효과는 1회만 적용(비중첩)
/// - 서로 다른 소스는 각각 적용되어 스택 가능
/// </summary>
public class AuraManager : MonoBehaviour
{
    private class SourceState
    {
        public List<AuraTracker> Targets = new List<AuraTracker>();
        public int StackCount = 0;            // 동일 소스에서 몇 번 등록되었는지
        public int AmountPerStack = 0;        // 한 번 등록 시 적용량
        public OwnerScope Scope;
        public bool MonstersOnly;
    }

    public static AuraManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 소스ID -> 상태
    private static readonly Dictionary<int, SourceState> stateBySource = new Dictionary<int, SourceState>();

    public void RegisterAttackAura(CardUI source, OwnerScope scope, int amount, bool monstersOnly = true)
    {
        if (source == null) { Debug.LogWarning("[AuraManager] source is null"); return; }
        int sourceId = source.GetInstanceID();

        if (!stateBySource.TryGetValue(sourceId, out var state))
        {
            state = new SourceState
            {
                Scope = scope,
                MonstersOnly = monstersOnly,
                AmountPerStack = amount,
            };

            var targets = CollectTargets(source, scope, monstersOnly);
            foreach (var t in targets)
            {
                var tracker = t.GetComponent<AuraTracker>();
                if (tracker == null) tracker = t.gameObject.AddComponent<AuraTracker>();
                tracker.ApplyAttackAura(sourceId, amount);
                state.Targets.Add(tracker);
            }
            state.StackCount = 1;
            stateBySource[sourceId] = state;
        }
        else
        {
            // 동일 소스에서 재등록 -> 현재 추적 중인 대상들에게만 추가 스택 적용
            foreach (var tracker in state.Targets)
            {
                if (tracker != null)
                {
                    tracker.ApplyAttackAura(sourceId, amount);
                }
            }
            state.StackCount += 1;
        }
    }

    /// <summary>
    /// 지속 마법/함정 효과를 등록하고 지속적으로 적용
    /// </summary>
    public void RegisterContinuousEffect(CardUI source, CardAbility ability, AbilityParameter param)
    {
        if (source == null || ability == null)
        {
            Debug.LogWarning("[AuraManager] RegisterContinuousEffect: source or ability is null");
            return;
        }

        try
        {
            // 지속 효과 즉시 발동 (초기 적용)
            ability.Activate(source, param);
            
            Debug.Log($"[AuraManager] 지속 효과 등록: {source.cardData.cardName}");
            
            // TODO: 실제 지속 효과 추적이 필요하다면 여기에 추가 로직 구현
            // 현재는 단순히 즉시 발동 후, 카드가 파괴될 때 UnregisterAllFromSource로 정리
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AuraManager] 지속 효과 등록 실패: {e.Message}");
        }
    }

    /// <summary>
    /// 턴 시작 시 발동되는 지속 효과들을 처리
    /// </summary>
    public void TriggerTurnStartEffects()
    {
        var allCards = Object.FindObjectsOfType<CardUI>();
        foreach (var cardUI in allCards)
        {
            if (!cardUI.isOnField) continue;
            
            var fm = cardUI.GetComponent<FildMonster>();
            if (fm != null)
            {
                fm.TriggerTurnStartEffect();
            }
        }
    }

    /// <summary>
    /// 턴 종료 시 발동되는 지속 효과들을 처리
    /// </summary>
    public void TriggerTurnEndEffects()
    {
        var allCards = Object.FindObjectsOfType<CardUI>();
        foreach (var cardUI in allCards)
        {
            if (!cardUI.isOnField) continue;
            
            var fm = cardUI.GetComponent<FildMonster>();
            if (fm != null)
            {
                fm.TriggerTurnEndEffect();
            }
        }
    }

    public void UnregisterAllFromSource(CardUI source)
    {
        if (source == null) return;
        int sourceId = source.GetInstanceID();
        if (!stateBySource.TryGetValue(sourceId, out var state)) return;

        foreach (var tracker in state.Targets)
        {
            if (tracker != null)
            {
                tracker.RemoveAttackAura(sourceId);
            }
        }
        stateBySource.Remove(sourceId);
    }

    // 새 카드가 필드에 들어왔을 때, 현재 활성화된 모든 오라를 그 카드에 적용
    public void NotifyCardEnteredField(CardUI entered)
    {
        if (entered == null) return;
        if (!entered.isOnField) return;

        foreach (var kv in stateBySource)
        {
            int sourceId = kv.Key;
            var state = kv.Value;
            // source 존재 여부 확인
            // source CardUI를 직접 저장하지 않으므로, owner 범위 판단을 위해 대상 수집 로직을 재사용
            // 간단화를 위해 entered가 해당 범위에 속하는지 개별 판정

            bool ownerOk = false;
            // state.Scope 에 따라 entered.ownerType 과 소스의 ownerType 비교가 필요하지만,
            // 소스 ownerType을 직접 보관하지 않으므로 Targets 중 하나를 참조하여 유추 (비어있다면 스킵)
            CardUI anyTargetOwnerRef = null;
            foreach (var t in state.Targets)
            {
                if (t != null)
                {
                    anyTargetOwnerRef = t.GetComponent<CardUI>();
                    if (anyTargetOwnerRef != null) break;
                }
            }
            if (anyTargetOwnerRef == null)
            {
                // 대상이 아직 없다면 스킵 (소스가 파괴 직전이거나 등록 직후 대상이 없을 수 있음)
                continue;
            }

            switch (state.Scope)
            {
                case OwnerScope.Self:
                    ownerOk = (entered.ownerType == anyTargetOwnerRef.ownerType);
                    break;
                case OwnerScope.Opponent:
                    ownerOk = (entered.ownerType != anyTargetOwnerRef.ownerType);
                    break;
                case OwnerScope.Both:
                    ownerOk = true;
                    break;
            }
            if (!ownerOk) continue;
            if (state.MonstersOnly && !(entered.cardData is MonsterCardData)) continue;

            var tracker = entered.GetComponent<AuraTracker>();
            if (tracker == null) tracker = entered.gameObject.AddComponent<AuraTracker>();

            // 누적 스택 수만큼 적용
            for (int i = 0; i < state.StackCount; i++)
            {
                tracker.ApplyAttackAura(sourceId, state.AmountPerStack);
            }

            // 추적 목록에 포함 (해제 시 제거를 위해)
            if (!state.Targets.Contains(tracker))
                state.Targets.Add(tracker);
        }
    }

    private List<CardUI> CollectTargets(CardUI source, OwnerScope scope, bool monstersOnly)
    {
        var result = new List<CardUI>();
        var all = Object.FindObjectsOfType<CardUI>(true);
        foreach (var ui in all)
        {
            if (ui == null) continue;
            if (!ui.isOnField) continue;

            // 소유자 범위 필터
            bool ownerOk = false;
            switch (scope)
            {
                case OwnerScope.Self:
                    ownerOk = (ui.ownerType == source.ownerType);
                    break;
                case OwnerScope.Opponent:
                    ownerOk = (ui.ownerType != source.ownerType);
                    break;
                case OwnerScope.Both:
                    ownerOk = true;
                    break;
            }
            if (!ownerOk) continue;

            // 몬스터만 필터
            if (monstersOnly && !(ui.cardData is MonsterCardData)) continue;

            result.Add(ui);
        }
        return result;
    }
}
