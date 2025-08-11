using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/Stun")]
public class Ability_Stun : CardAbility
{
	[SerializeField] private TargetingType targetingType;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (targetingType == TargetingType.Single)
		{
			param.target.SetStun(true);
		}
		else if (targetingType == TargetingType.Fild)
		{
			for (int i = 0; i < param.targets.Count; i++)
			{
				param.targets[i].SetStun(true);
			}
		}
	}
}
