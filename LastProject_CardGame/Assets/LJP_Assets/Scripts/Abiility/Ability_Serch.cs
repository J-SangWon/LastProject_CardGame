using UnityEngine;

public enum SerchType
{
	CardType, 
	Cost,
	Race,
	CardID
}

[CreateAssetMenu(menuName = "CardAbilities/Serch")]
public class Ability_Serch : CardAbility
{
	public SerchType SerchType;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		switch (SerchType)
		{
			case SerchType.CardType:
				break;
			case SerchType.Cost:
				break;
			case SerchType.Race:
				break;
			case SerchType.CardID:
				break;
		}
	}
}
