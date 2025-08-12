using UnityEngine;

public enum DestroyType { 
	Target,
	Random
}

[CreateAssetMenu(menuName = "CardAbilities/Destroy")]
public class Ability_Destroy : CardAbility
{
	[SerializeField] private DestroyType destroyType;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		switch(destroyType)
		{
			case DestroyType.Target:
				param.target.IsDeadFlag = true;
				param.target.DeathResolved = false;
				param.target.ResolveDeath();
				break;
			case DestroyType.Random:
				if (param.targets == null) return;
				int index = Random.Range(0, param.targets.Count);
				param.targets[index].IsDeadFlag = true;
				param.targets[index].DeathResolved = false;
				param.targets[index].ResolveDeath();
				break;
		}
	}
}
