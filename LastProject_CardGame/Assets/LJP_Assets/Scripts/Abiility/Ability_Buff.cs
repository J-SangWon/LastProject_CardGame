using System.Collections.Generic;
using UnityEngine;


public interface IBuffHandler
{
	void Apply(CardUI target, int value, List<int> beforeAttackList, List<int> beforeHealthList);
	void Reset(CardUI target, int index, List<int> beforeAttackList, List<int> beforeHealthList);
}

public enum BuffType
{
	AttackBuff,
	HealthBuff,
	AllBuff
}

[CreateAssetMenu(menuName = "CardAbilities/Buff")]
public class Ability_Buff : CardAbility
{
	[SerializeField] private BuffType buffType;

	[SerializeField] private bool isReset = true;

	private readonly List<int> beforeAttackList = new();
	private readonly List<int> beforeHealthList = new();
	private readonly List<CardUI> targetList = new();

	private static readonly Dictionary<BuffType, IBuffHandler> BuffHandlers = new()
	{
		{ BuffType.AttackBuff, new AttackBuffHandler() },
		{ BuffType.HealthBuff, new HealthBuffHandler() },
		{ BuffType.AllBuff, new AllBuffHandler() }
	};

	public override void Activate(CardUI card, AbilityParameter param)
	{
		var handler = BuffHandlers[buffType];

		if (targetType == TargetType.Single)
		{
			targetList.Add(param.target);
			handler.Apply(param.target, param.value, beforeAttackList, beforeHealthList);
		}
		else if(targetType == TargetType.Self)
		{
			targetList.Add(card);
            handler.Apply(card, param.value, beforeAttackList, beforeHealthList);
        }

		else
		{
			foreach (var t in param.targets)
			{
				targetList.Add(t);
				handler.Apply(t, param.value, beforeAttackList, beforeHealthList);
			}
		}
	}

	public void ResetStat()
	{
		var handler = BuffHandlers[buffType];

		for (int i = 0; i < targetList.Count; i++)
		{
			handler.Reset(targetList[i], i, beforeAttackList, beforeHealthList);
		}

		targetList.Clear();
		beforeAttackList.Clear();
		beforeHealthList.Clear();
	}

	public bool GetIsReset()
	{
		return isReset;
	}
}

public class AttackBuffHandler : IBuffHandler
{
	public void Apply(CardUI target, int value, List<int> beforeAttackList, List<int> beforeHealthList)
	{
		beforeAttackList.Add(target.attack);
		target.AddAttack(value);
	}

	public void Reset(CardUI target, int index, List<int> beforeAttackList, List<int> beforeHealthList)
	{
		target.SetAttack(beforeAttackList[index]);
	}
}

public class HealthBuffHandler : IBuffHandler
{
	public void Apply(CardUI target, int value, List<int> beforeAttackList, List<int> beforeHealthList)
	{
		beforeHealthList.Add(target.maxHealth);
		target.AddHealth(value);
	}

	public void Reset(CardUI target, int index, List<int> beforeAttackList, List<int> beforeHealthList)
	{
		target.AddHealth(beforeHealthList[index] - target.maxHealth);
	}
}

public class AllBuffHandler : IBuffHandler
{
	public void Apply(CardUI target, int value, List<int> beforeAttackList, List<int> beforeHealthList)
	{
		beforeAttackList.Add(target.attack);
		beforeHealthList.Add(target.maxHealth);
		target.AddAttack(value);
		target.AddHealth(value);
	}

	public void Reset(CardUI target, int index, List<int> beforeAttackList, List<int> beforeHealthList)
	{
		target.SetAttack(beforeAttackList[index]);
		target.AddHealth(beforeHealthList[index] - target.maxHealth);
	}
}
