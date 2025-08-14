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
    [Header("선택 필터")]
    public bool excludeSelfId = true; // 발동한 카드의 cardId와 동일한 카드는 부활 대상에서 제외

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
            Debug.LogWarning("[Ability_SummonFromGraveyard] DuelZoneManager.Instance is null");
            return;
        }
        
        if (isPlayer && duel.graveyardZone == null)
        {
            Debug.LogWarning("[Ability_SummonFromGraveyard] Graveyard zone not found");
            return;
        }
        
        if (!isPlayer && duel.enemyGraveyardZone == null)
        {
            Debug.LogWarning("[Ability_SummonFromGraveyard] Enemy Graveyard zone not found");
            return;
        }
        
        int count = Mathf.Max(1, param != null ? param.value : 1);
        var entries = isPlayer ? duel.graveyardZone.GetAllGraveyardCards() : duel.enemyGraveyardZone.GetAllGraveyardCards();
        
        foreach (var e in entries)
        {
            if (count <= 0) break;
            // 본인 카드ID 제외 조건
            if (excludeSelfId && card != null && card.cardData != null && e.card != null && e.card.cardId == card.cardData.cardId)
                continue;
            if (e.card != null && (useCompositeConditions ? AbilityConditionUtils.MatchesAll(conditions, compositeOperator, e.card) : MatchByData(e.card)))
            {
                bool removed = isPlayer ? duel.graveyardZone.RemoveFromGraveyard(e.card) : duel.enemyGraveyardZone.RemoveFromGraveyard(e.card);
                if (removed)
                {
                    var owner = isPlayer ? OwnerType.Player : OwnerType.Opponent;
                    if (PlayerCardManager.Instance != null)
                    {
                        var summoned = PlayerCardManager.Instance.SummonFromDataToMonsterZone(e.card, owner);
                        if (summoned == null)
                        {
                            Debug.LogWarning("[Ability_SummonFromGraveyard] Failed to summon card to monster zone");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Ability_SummonFromGraveyard] PlayerCardManager.Instance is null");
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


