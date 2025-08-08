using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/Create")]

public class Ability_Create : CardAbility
{
	[SerializeField] private GameObject cardPrefab;
	public override void Activate(CardUI card, AbilityParameter param)
	{
		GameObject cardGo = Instantiate(cardPrefab);

		cardGo.transform.SetParent(PlayerCardManager.Instance.handZone, false);
		cardGo.transform.localScale = Vector3.one;

		PlayerCardManager.Instance.UpdateHandLayout();
	}
}
