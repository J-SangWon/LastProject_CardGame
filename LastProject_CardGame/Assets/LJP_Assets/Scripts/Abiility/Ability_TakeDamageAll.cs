using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/TakeDamageAll")]
public class Ability_TakeDamageAll : CardAbility
{

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.targets != null)
		{
			for(int i = 0; i < param.targets.Count; i++)
			{
				param.targets[i].ReduceHealth(param.value);
				Debug.Log($"{card.name}가 {param.targets[i].name}에게 {param.value} 피해를 줌");
			}
		}
	}
}
