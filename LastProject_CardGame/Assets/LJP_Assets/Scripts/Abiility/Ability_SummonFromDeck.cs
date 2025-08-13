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
        // 소환 주체 판단 (내 카드/적 카드)
        bool isPlayer = card != null ? card.ownerType == OwnerType.Player : true;

        if (isPlayer)
        {
            if (PlayerCardManager.Instance == null)
            {
                Debug.LogWarning("[Ability_SummonFromDeck] PlayerCardManager.Instance is null");
                return;
            }
        }
        else
        {
            if (OpponentCardManager.Instance == null)
            {
                Debug.LogWarning("[Ability_SummonFromDeck] OpponentCardManager.Instance is null");
                return;
            }
        }

        int count = Mathf.Max(1, param != null ? param.value : 1);
        
        Debug.Log($"[Ability_SummonFromDeck] 소환 시도: count={count}, param.value={(param != null ? param.value.ToString() : "null")}");

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

        var hand = isPlayer ? PlayerCardManager.Instance.playerHandZone : OpponentCardManager.Instance.handZone;
        var before = new HashSet<Transform>();
        foreach (Transform t in hand)
        {
            before.Add(t);
        }

        if (isPlayer)
        {
            PlayerCardManager.Instance.SearchCard(condition, count);
        }
        else
        {
            OpponentCardManager.Instance.SearchCard(condition, count);
        }

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
            var owner = isPlayer ? OwnerType.Player : OwnerType.Opponent;
            if (!PlayerCardManager.Instance.PlaceExistingCardToMonsterSlot(go, owner))
            {
                if (isPlayer)
                {
                    PlayerCardManager.Instance.PlayCardToField(go);
                }
                else
                {
                    // 적 소환 폴백: 적 몬스터 슬롯 배치 시도 실패시, 일단 부모 유지
                }
            }
        }
    }
}


