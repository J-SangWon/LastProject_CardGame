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

        // 검색 주체 판단 (내 카드/적 카드)
        bool isPlayer = card != null ? card.ownerType == OwnerType.Player : true;

        // param이 null이거나 value가 0 이하인 경우 기본값 1 사용
        int count = (param != null && param.value > 0) ? param.value : 1;
        
        Debug.Log($"[Ability_Search] 서치 시도: {count}장, 플레이어: {isPlayer}, 검색타입: {searchType}");
        
        if (useCompositeConditions)
        {
            if (isPlayer)
            {
                if (PlayerCardManager.Instance != null)
                {
                    PlayerCardManager.Instance.SearchCard(
                        go => AbilityConditionUtils.MatchesAll(conditions, compositeOperator, go.GetComponent<CardUI>()),
                        count);
                    Debug.Log($"[Ability_Search] 플레이어 복합조건 서치 완료: {count}장");
                }
                else
                {
                    Debug.LogWarning("[Ability_Search] PlayerCardManager.Instance is null");
                }
            }
            else
            {
                if (OpponentCardManager.Instance != null)
                {
                    OpponentCardManager.Instance.SearchCard(
                        go => AbilityConditionUtils.MatchesAll(conditions, compositeOperator, go.GetComponent<CardUI>()),
                        count);
                }
                else
                {
                    Debug.LogWarning("[Ability_Search] OpponentCardManager.Instance is null");
                }
            }
        }
        else
        {
            if (searchConditions.TryGetValue(searchType, out var condition))
            {
                if (isPlayer)
                {
                    if (PlayerCardManager.Instance != null)
                    {
                        PlayerCardManager.Instance.SearchCard(condition, count);
                    }
                    else
                    {
                        Debug.LogWarning("[Ability_Search] PlayerCardManager.Instance is null");
                    }
                }
                else
                {
                    if (OpponentCardManager.Instance != null)
                    {
                        OpponentCardManager.Instance.SearchCard(condition, count);
                    }
                    else
                    {
                        Debug.LogWarning("[Ability_Search] OpponentCardManager.Instance is null");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Ability_Search] SearchType {searchType} 이(가) 유효하지 않습니다.");
            }
        }
	}
}
