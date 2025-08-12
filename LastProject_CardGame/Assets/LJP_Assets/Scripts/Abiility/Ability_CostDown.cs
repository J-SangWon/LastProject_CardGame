using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/CostDown")]

public class Ability_CostDown : CardAbility
{
	[SerializeField] private bool isReset = true;

	private int beforeCost;
	private bool isUsed = false;
	private CardUI targetCard;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (param.target == null) return;

		isUsed = true;
		targetCard = param.target;

		int.TryParse(targetCard.textCost.text, out beforeCost);
		targetCard.textCost.text = (beforeCost - param.value).ToString();
	}

	public void ResetCost(CardUI card, AbilityParameter param)
	{
		if(isUsed && targetCard != null)
		{
			targetCard.textCost.text = beforeCost.ToString();
			isUsed = false;
			targetCard = null;
		}
	}

	public bool GetIsReset()
	{
		return isReset;
	}
}
