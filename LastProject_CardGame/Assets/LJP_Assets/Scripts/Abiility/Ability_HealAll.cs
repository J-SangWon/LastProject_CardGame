using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/HealAll")]
public class Ability_HealAll : CardAbility
{
	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.targets != null)
		{
			foreach (var target in param.targets)
			{
				param.target.ReduceHealth(param.value);
				Debug.Log($"{card.name}가 {param.target.name}에게 {param.value} 회복함");
			}
		}
	}
}