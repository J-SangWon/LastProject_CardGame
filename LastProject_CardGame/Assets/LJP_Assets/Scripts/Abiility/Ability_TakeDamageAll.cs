using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/TakeDamageAll")]
public class Ability_TakeDamageAll : CardAbility
{

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.targets != null)
		{			
			foreach (var target in param.targets)
			{
				param.target.ReduceHealth(param.value);
				Debug.Log($"{card.name}가 {param.target.name}에게 {param.value} 피해를 줌");
			}
		}
	}
}
