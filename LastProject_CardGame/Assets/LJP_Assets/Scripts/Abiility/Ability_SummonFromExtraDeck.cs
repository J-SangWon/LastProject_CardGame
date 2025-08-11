using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/SummonFromExtraDeck")]
public class Ability_SummonFromExtraDeck : CardAbility
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
        // 소환 주체 판단 (내 카드/적 카드)
        bool isPlayer = card != null ? card.ownerType == OwnerType.Player : true;
        
        var duel = DuelZoneManager.Instance;
        if (duel == null)
        {
            Debug.LogWarning("[Ability_SummonFromExtraDeck] DuelZoneManager.Instance is null");
            return;
        }
        
        if (isPlayer && duel.extraDeckZone == null)
        {
            Debug.LogWarning("[Ability_SummonFromExtraDeck] ExtraDeck zone not found");
            return;
        }
        
        if (!isPlayer && duel.enemyExtraDeckZone == null)
        {
            Debug.LogWarning("[Ability_SummonFromExtraDeck] Enemy ExtraDeck zone not found");
            return;
        }
        
        int count = Mathf.Max(1, param != null ? param.value : 1);
        var entries = isPlayer ? duel.extraDeckZone.GetAllEntries() : duel.enemyExtraDeckZone.GetAllEntries();
        
        foreach (var e in entries)
        {
            if (count <= 0) break;
            if (e.card != null && (useCompositeConditions ? AbilityConditionUtils.MatchesAll(conditions, compositeOperator, e.card) : MatchByData(e.card)))
            {
                bool removed = isPlayer ? duel.extraDeckZone.RemoveSpecificCard(e.card) : duel.enemyExtraDeckZone.RemoveSpecificCard(e.card);
                if (removed)
                {
                    var owner = isPlayer ? OwnerType.Player : OwnerType.Opponent;
                    if (PlayerCardManager.Instance != null)
                    {
                        var summoned = PlayerCardManager.Instance.SummonFromDataToMonsterZone(e.card, owner);
                        if (summoned == null)
                        {
                            Debug.LogWarning("[Ability_SummonFromExtraDeck] Failed to summon card to monster zone");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Ability_SummonFromExtraDeck] PlayerCardManager.Instance is null");
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


