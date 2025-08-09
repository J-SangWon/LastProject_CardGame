using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public enum SearchType
{
	CardType, 
	Cost,
	Race,
	CardID
}

[CreateAssetMenu(menuName = "CardAbilities/Search")]
public class Ability_Search : CardAbility
{
	public SearchType searchType;

	public CardType cardType;
	public Race race;
	public int cost;
	public string cardID;

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
                    return ui != null && ui.cardData != null && ui.cardData.cost == cost;
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
            }
        };
	}

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (searchConditions == null) InitConditions(card);

		for (int i = 0; i < param.value; i++)
		{
			if (searchConditions.TryGetValue(searchType, out var condition))
			{
				PlayerCardManager.Instance.SearchCard(condition);
			}
			else
			{
				Debug.LogWarning($"[Ability_Search] SearchType {searchType}�� ���� ������ �����ϴ�.");
			}
		}
	}
}
