using UnityEngine;

public enum BuffType
{
	AttackBuff,
	HealthBuff,
	AllBuff
}

public enum BuffTarget
{
	Single,
	Fild,
	Hand,
	Deck
}

[CreateAssetMenu(menuName = "CardAbilities/Buff")]
public class Ability_Buff : CardAbility
{
	[SerializeField] private BuffType bufftype;
	public BuffTarget buffTarget;

	private int beforeAttack;
	private int beforeHealth;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if(buffTarget == BuffTarget.Single)
		{
			if (bufftype == BuffType.AttackBuff)
			{ 
				beforeAttack = param.target.attack;
				param.target.AddAttack(param.value);
			}
		}
		else
		{

		}
	}
}
