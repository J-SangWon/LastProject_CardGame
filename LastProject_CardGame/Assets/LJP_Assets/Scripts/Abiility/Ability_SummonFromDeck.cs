using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/SummonFromDeck")]
public class Ability_SummonFromDeck : CardAbility
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
        if (PlayerCardManager.Instance == null)
        {
            Debug.LogWarning("[Ability_SummonFromDeck] PlayerCardManager.Instance is null");
            return;
        }

        int count = Mathf.Max(1, param != null ? param.value : 1);

        Func<GameObject, bool> condition = go =>
        {
            var ui = go != null ? go.GetComponent<CardUI>() : null;
            if (ui == null || ui.cardData == null) return false;
            if (useCompositeConditions)
            {
                return AbilityConditionUtils.MatchesAll(conditions, compositeOperator, ui);
            }
            switch (searchType)
            {
                case SearchType.CardType:
                    return ui.cardData.cardType == cardType;
                case SearchType.Cost:
                    return AbilityConditionUtils.CompareInt(ui.cardData.cost, cost, costOp);
                case SearchType.Race:
                    return ui.monsterCardData != null && ui.monsterCardData.race == race;
                case SearchType.CardID:
                    return ui.cardData.cardId == cardID;
                case SearchType.Tag:
                    return ui.cardData.tags != null && ui.cardData.tags.Contains(tag);
                default:
                    return false;
            }
        };

        var hand = PlayerCardManager.Instance.handZone;
        var before = new HashSet<Transform>();
        foreach (Transform t in hand)
        {
            before.Add(t);
        }

        PlayerCardManager.Instance.SearchCard(condition, count);

        var moved = new List<GameObject>();
        foreach (Transform t in hand)
        {
            if (!before.Contains(t))
            {
                moved.Add(t.gameObject);
            }
        }

        foreach (var go in moved)
        {
            // 플레이어 몬스터존 빈 슬롯으로 이동
            if (!PlayerCardManager.Instance.PlaceExistingCardToMonsterSlot(go, OwnerType.Player))
            {
            }
        }
    }
}


