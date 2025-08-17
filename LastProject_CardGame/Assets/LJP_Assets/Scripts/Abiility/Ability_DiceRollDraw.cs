using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/DiceRollDraw")]
public class Ability_DiceRollDraw : CardAbility
{
    [Header("드로우 설정")]
    public int drawCountDefault = 2; // 기본 드로우 장 수 (2장)

    public override void Activate(CardUI card, AbilityParameter param)
    {
        if (Dice.Instance == null)
        {
            Debug.LogError("[Ability_DiceRollDraw] Dice.Instance가 없습니다. 씬에 Dice 프리팹을 배치하세요.");
            return;
        }

        // 주사위 표시 위치 (카메라 기준)
        Vector3 pos = Camera.main.transform.position;
        Dice.Instance.SetDice(pos, Camera.main.transform);

        // 주사위 멈추고 결과 받기
        Dice.Instance.StopAndGetDiceValue((value) =>
        {
            Debug.Log($"[Ability_DiceRollDraw] 주사위 결과: {value}");

            // 1~3 실패
            if (value <= 3)
            {
                Debug.Log("[Ability_DiceRollDraw] 실패! 드로우 발동 안 됨");
                return;
            }

            // 4~6 성공 → 드로우 실행
            bool isPlayer = card != null ? card.ownerType == OwnerType.Player : true;
            int drawCount = (param != null && param.value > 0) ? param.value : drawCountDefault;

            Debug.Log($"[Ability_DiceRollDraw] 성공! {drawCount}장 드로우, 플레이어: {isPlayer}");

            if (isPlayer)
            {
                if (PlayerCardManager.Instance != null)
                {
                    PlayerCardManager.Instance.DrawCards(drawCount);
                }
                else
                {
                    Debug.LogError("[Ability_DiceRollDraw] PlayerCardManager.Instance is null");
                }
            }
            else
            {
                if (OpponentCardManager.Instance != null)
                {
                    OpponentCardManager.Instance.DrawCards(drawCount);
                }
                else
                {
                    Debug.LogError("[Ability_DiceRollDraw] OpponentCardManager.Instance is null");
                }
            }
        });
    }
}
