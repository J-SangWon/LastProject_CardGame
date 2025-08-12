using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/TokenSunmmon")]

public class Ability_TokenSummon : CardAbility
{
	[SerializeField] private BaseCardData Token;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		PlayerCardManager.Instance.SummonFromDataToMonsterZone(Token, card.ownerType);
		Debug.Log("토큰 소환");
	}
}
