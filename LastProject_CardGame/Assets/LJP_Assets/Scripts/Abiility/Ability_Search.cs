using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum SearchType
{
    CardType, 
	Cost,
	Race,
    CardID,
    Tag
}

[CreateAssetMenu(menuName = "CardAbilities/Search")]
public class Ability_Search : CardAbility
{
	public SearchType searchType;

	public CardType cardType;
	public Race race;
	public int cost;
    public NumericCompareOp costOp = NumericCompareOp.Equal;
	public string cardID;
    public string tag; // BaseCardData.tags에 포함 여부로 매칭

    [Header("다중 효과 조건")]
    public bool useCompositeConditions = false;
    public LogicalOperator compositeOperator = LogicalOperator.And;
    public List<SearchCondition> conditions = new List<SearchCondition>();

	private Dictionary<SearchType, Func<GameObject, bool>> searchConditions;

	private void InitConditions(CardUI cardUI)
	{
        searchConditions = new Dictionary<SearchType, Func<GameObject, bool>>()
        {
            { SearchType.CardType, card =>
                {
                    var ui = card.GetComponent<CardUI>();
                    return ui != null && ui.cardData != null && ui.cardData.cardType == cardType;
                }
            },
            { SearchType.Cost, card =>
                {
                    var ui = card.GetComponent<CardUI>();
                    return ui != null && ui.cardData != null && AbilityConditionUtils.CompareInt(ui.cardData.cost, cost, costOp);
                }
            },
            { SearchType.Race, card =>
                {
                    var ui = card.GetComponent<CardUI>();
                    var data = ui != null ? ui.monsterCardData : null;
                    return data != null && data.race == race;
                }
            },
            { SearchType.CardID, card =>
                {
                    var ui = card.GetComponent<CardUI>();
                    return ui != null && ui.cardData != null && ui.cardData.cardId == cardID;
                }
            },
            { SearchType.Tag, card =>
                {
                    var ui = card.GetComponent<CardUI>();
                    return ui != null && ui.cardData != null && ui.cardData.tags != null && ui.cardData.tags.Contains(tag);
                }
            }
        };
	}

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (searchConditions == null) InitConditions(card);

        int count = Mathf.Max(1, param.value);
        if (useCompositeConditions)
        {
            PlayerCardManager.Instance.SearchCard(
                go => AbilityConditionUtils.MatchesAll(conditions, compositeOperator, go.GetComponent<CardUI>()),
                count);
        }
        else
        {
            if (searchConditions.TryGetValue(searchType, out var condition))
            {
                PlayerCardManager.Instance.SearchCard(condition, count);
            }
            else
            {
                Debug.LogWarning($"[Ability_Search] SearchType {searchType} 이(가) 유효하지 않습니다.");
            }
        }
	}
}
