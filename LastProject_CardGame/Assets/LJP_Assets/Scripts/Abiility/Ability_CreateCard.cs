using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/CreateCard")]
public class Ability_CreateCardToHnad : CardAbility
{
	[SerializeField] private BaseCardData cardData;

	public override void Activate(CardUI card, AbilityParameter param)
	{
		switch (targetType)
		{
			case TargetType.Deck:
				CrateCardToDeckLogic();
				break;
			case TargetType.Hand:
				CrateCardToHandLogic();
				break;
			default:
				Debug.Log("필드와 덱만 생성가능!!");
				break;
		}
	}

	private void CrateCardToDeckLogic()
	{
		GameObject cardGo = PlayerCardManager.Instance.CreateCard(cardData, PlayerCardManager.Instance.cardPrefab, PlayerCardManager.Instance.playerDeckZone, Quaternion.identity);
		int randIndex = Random.Range(0, PlayerCardManager.Instance.GetDeck().Count);
		Debug.Log("인덱스 : " + randIndex);

		for (int i = randIndex; i < PlayerCardManager.Instance.GetDeck().Count; i++) //램덤생성된 카드 위에 인덱스 카드 뒤로 밀기
		{
			PlayerCardManager.Instance.GetDeck()[i].transform.localPosition += new Vector3(0, 0, -0.01f);
		}

		cardGo.transform.localPosition = new Vector3(0, 0, -randIndex * 0.01f);
		var uiDeck = cardGo.GetComponent<CardUI>();
		if (uiDeck != null)
		{
			uiDeck.ownerType = OwnerType.Player;
			uiDeck.SetFace(false); // 덱은 뒷면
			uiDeck.EnableCardFlip = false;
		}
		cardGo.AddComponent<FildMonster>();

		PlayerCardManager.Instance.GetDeck().Insert(randIndex, cardGo);
	}

	private void CrateCardToHandLogic()
	{
		GameObject cardGo = PlayerCardManager.Instance.CreateCard(cardData, PlayerCardManager.Instance.cardPrefab, PlayerCardManager.Instance.playerHandZone, Quaternion.identity);

		var uiHand = cardGo.GetComponent<CardUI>();
		if (uiHand != null)
		{
			uiHand.ownerType = OwnerType.Player;
			uiHand.SetFace(true);      // 손패는 앞면
			uiHand.EnableCardFlip = false; // 손패 클릭 플립 비활성화
		}
		cardGo.AddComponent<FildMonster>();

	}
}
