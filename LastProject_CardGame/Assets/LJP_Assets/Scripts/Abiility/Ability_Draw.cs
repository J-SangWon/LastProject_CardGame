using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/Draw")]

public class Ability_Draw : CardAbility
{
	public override void Activate(CardUI card, AbilityParameter param)
	{
		// 드로우 주체 판단 (내 카드/적 카드)
		bool isPlayer = card != null ? card.ownerType == OwnerType.Player : true;
		
		// param이 null이거나 value가 0 이하인 경우 기본값 1 사용
		int drawCount = (param != null && param.value > 0) ? param.value : 1;
		
		Debug.Log($"[Ability_Draw] 드로우 시도: {drawCount}장, 플레이어: {isPlayer}");
		
		if (isPlayer)
		{
			if (PlayerCardManager.Instance != null)
			{
				PlayerCardManager.Instance.DrawCards(drawCount);
				Debug.Log($"[Ability_Draw] 플레이어 드로우 완료: {drawCount}장");
			}
			else
			{
				Debug.LogError("[Ability_Draw] PlayerCardManager.Instance is null");
			}
		}
		else
		{
			if (OpponentCardManager.Instance != null)
			{
				OpponentCardManager.Instance.DrawCards(drawCount);
				Debug.Log($"[Ability_Draw] 상대방 드로우 완료: {drawCount}장");
			}
			else
			{
				Debug.LogError("[Ability_Draw] OpponentCardManager.Instance is null");
			}
		}
	}
}
