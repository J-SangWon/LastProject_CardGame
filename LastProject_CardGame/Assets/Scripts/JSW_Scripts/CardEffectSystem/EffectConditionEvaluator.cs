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
        foreach (var type in condition.conditionType)
        {
            if (type == triggeredType)
            {
                typeMatch = true;
                break;
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

        if (condition.intValue > 0)
        {
            if (currentValue < condition.intValue)
                return false;
        }

        return true;
    }
}
