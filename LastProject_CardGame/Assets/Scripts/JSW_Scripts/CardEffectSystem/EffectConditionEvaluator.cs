public static class EffectConditionEvaluator
{
    public static bool IsConditionMet(EffectCondition condition, GamePhase currentPhase, ConditionType triggeredType, string currentCardId, int currentValue, OwnerType casterOwner = OwnerType.Player)
    {
        if (condition == null || condition.conditionType == null || condition.conditionType.Length == 0)
            return true; // 조건 없으면 항상 발동

        // 1) 페이즈 체크
        if (condition.gamePhase != GamePhase.None && condition.gamePhase != currentPhase)
            return false;

        // 2) 각 조건을 개별 평가한 뒤, 선택된 조합(AND/OR)으로 집계
        bool anyGrave = false;
        int evaluatedTotal = 0; // None 제외한 실제 평가 개수
        int satisfied = 0;

        for (int i = 0; i < condition.conditionType.Length; i++)
        {
            var type = condition.conditionType[i];
            if (type == ConditionType.None) continue;

            bool ok;
            if (type == ConditionType.WhenGraveyardCount || type == ConditionType.WhenGraveyardHasTag)
            {
                anyGrave = true;
                ok = CheckGraveyardCondition(condition, type, casterOwner);
            }
            else
            {
                // 비-묘지 조건은 트리거일치 여부로 판단
                ok = (type == triggeredType);
            }
            evaluatedTotal++;
            if (ok) satisfied++;
        }

        bool combinedOk;
        if (evaluatedTotal == 0)
        {
            combinedOk = true;
        }
        else if (condition.combination == ConditionCombination.AND)
        {
            combinedOk = (satisfied == evaluatedTotal);
        }
        else // Any (OR)
        {
            combinedOk = (satisfied > 0);
        }

        if (!combinedOk)
            return false;

        // 3) 추가 매개 체크
        if (!string.IsNullOrEmpty(condition.targetCardId) && condition.targetCardId != currentCardId)
            return false;

        // 기존 의미 유지: currentValue는 비-묘지 매개로만 사용
        if (!anyGrave && condition.intValue > 0 && currentValue < condition.intValue)
            return false;

        return true;
    }

    /// <summary>
    /// 묘지 조건 체크
    /// </summary>
    private static bool CheckGraveyardCondition(EffectCondition condition, ConditionType conditionType, OwnerType casterOwner)
    {
        if (DuelZoneManager.Instance == null) return false;

        // 태그 없으면 평가 불가
        if (string.IsNullOrEmpty(condition.requiredTag)) return false;

        var dz = DuelZoneManager.Instance;
        int playerCount = dz.GetGraveyardCardCountByTag(OwnerType.Player, condition.requiredTag);
        int opponentCount = dz.GetGraveyardCardCountByTag(OwnerType.Opponent, condition.requiredTag);

        switch (conditionType)
        {
            case ConditionType.WhenGraveyardCount:
            {
                int total = 0;
                switch (condition.ownerScope)
                {
                    case OwnerScope.Self:
                        total = (casterOwner == OwnerType.Player) ? playerCount : opponentCount;
                        break;
                    case OwnerScope.Opponent:
                        total = (casterOwner == OwnerType.Player) ? opponentCount : playerCount;
                        break;
                    case OwnerScope.Both:
                        total = playerCount + opponentCount;
                        break;
                }
                return total >= condition.intValue;
            }

            case ConditionType.WhenGraveyardHasTag:
                if (string.IsNullOrEmpty(condition.requiredTag)) return false;
                bool hasPlayer = false;
                bool hasEnemy = false;
                if (condition.ownerScope == OwnerScope.Self || condition.ownerScope == OwnerScope.Both)
                {
                    var selfOwner = casterOwner;
                    hasPlayer = DuelZoneManager.Instance.GetGraveyardCardCountByTag(selfOwner, condition.requiredTag) > 0;
                }
                if (condition.ownerScope == OwnerScope.Opponent || condition.ownerScope == OwnerScope.Both)
                {
                    var oppOwner = (casterOwner == OwnerType.Player) ? OwnerType.Opponent : OwnerType.Player;
                    hasEnemy = DuelZoneManager.Instance.GetGraveyardCardCountByTag(oppOwner, condition.requiredTag) > 0;
                }
                return hasPlayer || hasEnemy;

            default:
                return false;
        }
    }
}
