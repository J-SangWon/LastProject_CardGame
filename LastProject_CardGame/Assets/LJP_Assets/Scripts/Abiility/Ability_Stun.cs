using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/Stun")]
public class Ability_Stun : CardAbility
{

	public override void Activate(CardUI card, AbilityParameter param)
	{
		if (targetType == TargetType.Single)
		{
			param.target.SetStun(true);
		}
		else if (targetType == TargetType.Fild)
		{
			for (int i = 0; i < param.targets.Count; i++)
			{
				param.targets[i].SetStun(true);
			}
		}
		else 
		{
			Debug.Log("Ability_Stun에 targetType을 Single혹은 Fild로 바꿔주세요!!");
		}
	}
}
