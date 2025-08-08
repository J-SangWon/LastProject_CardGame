using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(menuName = "CardAbilities/TakeDamage")]
public class Ability_TakeDamage : CardAbility
{
	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.target != null)
		{
			param.target.ReduceHealth(param.value);
			Debug.Log($"{card.name}가 {param.target.name}에게 {param.value} 피해를 줌");
		}
	}
}