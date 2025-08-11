using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/CloneSunmmon")]

public class Ability_CloneSummon : CardAbility
{
	[SerializeField] private GameObject Clone;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		throw new System.NotImplementedException();
	}
}
