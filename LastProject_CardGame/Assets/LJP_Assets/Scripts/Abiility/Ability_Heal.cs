using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/Heal")]

public class Ability_Heal : CardAbility
{
	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.target != null)
		{
			param.target.Heal(param.value);
			Debug.Log($"{card.name}가 {param.target.name}에게 {param.value} 회복함");
		}
	}
}
