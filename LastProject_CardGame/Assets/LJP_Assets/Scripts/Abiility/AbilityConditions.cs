using System.Collections.Generic;
using UnityEngine;

public enum LogicalOperator
{
    And,
    Or
}

[System.Serializable]
public enum NumericCompareOp
{
    Equal,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual
}

[System.Serializable]
public class SearchCondition
{
    public SearchType searchType;
    public CardType cardType;
    public Race race;
    public int cost;
    public NumericCompareOp costOp = NumericCompareOp.Equal;
    public string cardID;
    public string tag;
}

public static class AbilityConditionUtils
{
    public static bool MatchesAll(List<SearchCondition> conditions, LogicalOperator op, CardUI ui)
    {
        if (conditions == null || conditions.Count == 0) return true;
        if (ui == null || ui.cardData == null) return false;

        if (op == LogicalOperator.And)
        {
            foreach (var c in conditions)
            {
                if (!Matches(c, ui)) return false;
            }
            return true;
        }
        else
        {
            foreach (var c in conditions)
            {
                if (Matches(c, ui)) return true;
            }
            return false;
        }
    }

    public static bool MatchesAll(List<SearchCondition> conditions, LogicalOperator op, BaseCardData data)
    {
        if (conditions == null || conditions.Count == 0) return true;
        if (data == null) return false;

        if (op == LogicalOperator.And)
        {
            foreach (var c in conditions)
            {
                if (!Matches(c, data)) return false;
            }
            return true;
        }
        else
        {
            foreach (var c in conditions)
            {
                if (Matches(c, data)) return true;
            }
            return false;
        }
    }

    public static bool Matches(SearchCondition c, CardUI ui)
    {
        switch (c.searchType)
        {
            case SearchType.CardType:
                return ui.cardData.cardType == c.cardType;
            case SearchType.Cost:
                return CompareInt(ui.cardData.cost, c.cost, c.costOp);
            case SearchType.Race:
                return ui.monsterCardData != null && ui.monsterCardData.race == c.race;
            case SearchType.CardID:
                return ui.cardData.cardId == c.cardID;
            case SearchType.Tag:
                return ui.cardData.tags != null && ui.cardData.tags.Contains(c.tag);
            default:
                return false;
        }
    }

    public static bool Matches(SearchCondition c, BaseCardData data)
    {
        switch (c.searchType)
        {
            case SearchType.CardType:
                return data.cardType == c.cardType;
            case SearchType.Cost:
                return CompareInt(data.cost, c.cost, c.costOp);
            case SearchType.Race:
                var m = data as MonsterCardData;
                return m != null && m.race == c.race;
            case SearchType.CardID:
                return data.cardId == c.cardID;
            case SearchType.Tag:
                return data.tags != null && data.tags.Contains(c.tag);
            default:
                return false;
        }
    }

    public static bool CompareInt(int left, int right, NumericCompareOp op)
    {
        switch (op)
        {
            case NumericCompareOp.Equal:          return left == right;
            case NumericCompareOp.Less:           return left <  right;
            case NumericCompareOp.LessOrEqual:    return left <= right;
            case NumericCompareOp.Greater:        return left >  right;
            case NumericCompareOp.GreaterOrEqual: return left >= right;
            default: return left == right;
        }
    }
}


