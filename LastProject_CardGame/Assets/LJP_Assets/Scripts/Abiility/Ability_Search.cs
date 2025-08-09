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
			{ SearchType.CardType, card => cardUI.cardData.cardType == cardType },
			{ SearchType.Cost, card => cardUI.cardData.cost == cost },
			{ SearchType.Race, card =>
				{
					var data = cardUI.monsterCardData;
					return data != null && data.race == race;
				}
			},
			{ SearchType.CardID, card => cardUI.cardData.cardId == cardID }
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
				Debug.LogWarning($"[Ability_Search] SearchType {searchType}에 대한 조건이 없습니다.");
			}
		}
	}
}
