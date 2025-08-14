public static class EffectConditionEvaluator
{
    public static bool IsConditionMet(EffectCondition condition, GamePhase currentPhase, ConditionType triggeredType, string currentCardId, int currentValue)
    {
        if (condition == null || condition.conditionType == null || condition.conditionType.Length == 0)
            return true; // 조건 없으면 항상 발동

        // 1. 게임 페이즈 체크
        if (condition.gamePhase != GamePhase.None && condition.gamePhase != currentPhase)
            return false;

        // 2. 트리거 타입 체크 (하나라도 맞으면 발동)
        bool typeMatch = false;
        bool matchedByGraveyard = false;
        foreach (var type in condition.conditionType)
        {
            if (type == triggeredType)
            {
                typeMatch = true;
                break;
            }
            
            // 묘지 조건은 언제든지 체크 가능
            if (type == ConditionType.WhenGraveyardCount || type == ConditionType.WhenGraveyardHasTag)
            {
                if (CheckGraveyardCondition(condition, type))
                {
                    typeMatch = true;
                    matchedByGraveyard = true;
                    break;
                }
            }
        }
        if (!typeMatch)
            return false;

        // 3. 추가 값 체크
        if (!string.IsNullOrEmpty(condition.targetCardId))
        {
            if (condition.targetCardId != currentCardId)
                return false;
        }

        // 묘지 조건으로 매칭된 경우에는 currentValue 비교를 건너뛴다
        if (!matchedByGraveyard && condition.intValue > 0)
        {
            if (currentValue < condition.intValue)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 묘지 조건 체크
    /// </summary>
    private static bool CheckGraveyardCondition(EffectCondition condition, ConditionType conditionType)
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
                        total = playerCount;
                        break;
                    case OwnerScope.Opponent:
                        total = opponentCount;
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
                    hasPlayer = DuelZoneManager.Instance.GetGraveyardCardCountByTag(OwnerType.Player, condition.requiredTag) > 0;
                }
                if (condition.ownerScope == OwnerScope.Opponent || condition.ownerScope == OwnerScope.Both)
                {
                    hasEnemy = DuelZoneManager.Instance.GetGraveyardCardCountByTag(OwnerType.Opponent, condition.requiredTag) > 0;
                }
                return hasPlayer || hasEnemy;

            default:
                return false;
        }
    }
}
