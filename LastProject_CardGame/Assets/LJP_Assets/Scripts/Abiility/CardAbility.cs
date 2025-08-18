using System.Collections.Generic;
using UnityEngine;

public class AbilityParameter
{
	public int value;              // 예: 데미지량, 힐량 등
	public CardUI target;     // 단일 대상
	public List<CardUI> targets; // 복수 대상
}

public enum TargetType
{
	None,
	Single,
	Fild,
	Hand,
	Deck,
	Self
}

public enum TargetOwner
{
	None,
	Player,
	Enemy,
	All
}

public abstract class CardAbility : ScriptableObject
{
	public abstract void Activate(CardUI card, AbilityParameter param);

	public TargetType targetType;
	public TargetOwner targetOwner;

    [Header("발동 조건")]
    public EffectCondition condition;

}
