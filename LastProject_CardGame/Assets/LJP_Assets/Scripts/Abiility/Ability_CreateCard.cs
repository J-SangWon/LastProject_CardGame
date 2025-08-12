using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/CreateCard")]
public class Ability_CreateCardToHnad : CardAbility
{
	[SerializeField] private BaseCardData cardData;
	[SerializeField] private TargetingType creatLocation;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		switch (creatLocation)
		{
			case TargetingType.Deck:
				CrateCardToDeckLogic();
				break;
			case TargetingType.Hand:
				CrateCardToHandLogic();
				break;
			default:
				Debug.Log("필드와 덱만 생성가능!!");
				break;

		}


		PlayerCardManager.Instance.UpdateHandLayout();
	}

	private void CrateCardToDeckLogic()
	{
		GameObject cardGo = PlayerCardManager.Instance.CreateCard(cardData, PlayerCardManager.Instance.cardPrefab, PlayerCardManager.Instance.deckZone, Quaternion.identity);
		int randIndex = Random.Range(0, PlayerCardManager.Instance.GetDeck().Count);
		Debug.Log("인덱스 : " + randIndex);

		for (int i = randIndex; i < PlayerCardManager.Instance.GetDeck().Count; i++) //램덤생성된 카드 위에 인덱스 카드 뒤로 밀기
		{
			PlayerCardManager.Instance.GetDeck()[i].transform.localPosition += new Vector3(0, 0, -0.01f);
		}

		cardGo.transform.localPosition = new Vector3(0, 0, -randIndex * 0.01f);
		cardGo.GetComponent<CardUI>().EnableCardFlip = false;
		cardGo.AddComponent<FildMonster>();

		PlayerCardManager.Instance.GetDeck().Insert(randIndex, cardGo);
	}

	private void CrateCardToHandLogic()
	{
		GameObject cardGo = PlayerCardManager.Instance.CreateCard(cardData, PlayerCardManager.Instance.cardPrefab, PlayerCardManager.Instance.handZone, Quaternion.identity);

		cardGo.GetComponent<CardUI>().EnableCardFlip = false;
		cardGo.AddComponent<FildMonster>();

	}
}
