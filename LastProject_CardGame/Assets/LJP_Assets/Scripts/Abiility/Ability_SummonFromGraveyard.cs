using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/SummonFromGraveyard")]
public class Ability_SummonFromGraveyard : CardAbility
{
    public SearchType searchType;
    public CardType cardType;
    public Race race;
    public int cost;
    public NumericCompareOp costOp = NumericCompareOp.Equal;
    public string cardID;
    public string tag;

    [Header("다중 효과 조건")]
    public bool useCompositeConditions = false;
    public LogicalOperator compositeOperator = LogicalOperator.And;
    public List<SearchCondition> conditions = new List<SearchCondition>();

    public override void Activate(CardUI card, AbilityParameter param)
    {
        var duel = DuelZoneManager.Instance;
        if (duel == null || duel.graveyardZone == null)
        {
            Debug.LogWarning("[Ability_SummonFromGraveyard] Graveyard zone not found");
            return;
        }

        int count = Mathf.Max(1, param != null ? param.value : 1);
        var entries = duel.graveyardZone.GetAllGraveyardCards();
        foreach (var e in entries)
        {
            if (count <= 0) break;
            if (e.card != null && (useCompositeConditions ? AbilityConditionUtils.MatchesAll(conditions, compositeOperator, e.card) : MatchByData(e.card)))
            {
                if (duel.graveyardZone.RemoveFromGraveyard(e.card))
                {
                    // 플레이어 몬스터존 빈 슬롯으로 소환
                    var summoned = PlayerCardManager.Instance.SummonFromDataToMonsterZone(e.card, OwnerType.Player);
                    if (summoned == null)
                    {
                    }
                    count--;
                }
            }
        }
    }

    private bool MatchByData(BaseCardData data)
    {
        switch (searchType)
        {
            case SearchType.CardType:
                return data.cardType == cardType;
            case SearchType.Cost:
                return AbilityConditionUtils.CompareInt(data.cost, cost, costOp);
            case SearchType.Race:
                var m = data as MonsterCardData;
                return m != null && m.race == race;
            case SearchType.CardID:
                return data.cardId == cardID;
            case SearchType.Tag:
                return data.tags != null && data.tags.Contains(tag);
        }
        return false;
    }
}


