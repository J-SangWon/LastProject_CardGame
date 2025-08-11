using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/CostDown")]

public class Ability_CostDown : CardAbility
{
	private int beforeCost;
	private bool isUsed = false;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.target.GetComponent<BaseCardData>() == null) return;

		isUsed = true;

		beforeCost = param.target.GetComponent<BaseCardData>().cost;
		param.target.GetComponent<BaseCardData>().cost -= param.value;
	}

	public void ReturnCost(CardUI card, AbilityParameter param)
	{
		if(isUsed && param.target.GetComponent<BaseCardData>() != null)
		{
			param.target.GetComponent<BaseCardData>().cost = beforeCost;
			isUsed = false;
		}
	}
}
