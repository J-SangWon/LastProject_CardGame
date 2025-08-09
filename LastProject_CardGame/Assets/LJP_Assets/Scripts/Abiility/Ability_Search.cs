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

	private void InitConditions()
	{
		searchConditions = new Dictionary<SearchType, Func<GameObject, bool>>()
		{
			{ SearchType.CardType, card => card.GetComponent<BaseCardData>().cardType == cardType },
			{ SearchType.Cost, card => card.GetComponent<BaseCardData>().cost == cost },
			{ SearchType.Race, card =>
				{
					var data = card.GetComponent<MonsterCardData>();
					return data != null && data.race == race;
				}
			},
			{ SearchType.CardID, card => card.GetComponent<BaseCardData>().cardId == cardID }
		};
	}

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (searchConditions == null) InitConditions();

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
