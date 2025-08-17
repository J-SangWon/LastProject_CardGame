using UnityEngine;

[CreateAssetMenu(menuName = "CardAbilities/DiceRollDraw1")]
public class Ability_DiceRollDraw1 : CardAbility
{
    [Header("드로우 설정")]
    public int drawCountDefault = 1; // 기본 드로우 장 수 (1장)

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

            // 짝수 = 플레이어 드로우 / 홀수 = 상대 드로우
            bool isPlayer = (value % 2 == 0);

            int drawCount = (param != null && param.value > 0) ? param.value : drawCountDefault;

            if (isPlayer)
            {
                Debug.Log($"[Ability_DiceRollDraw] 짝수({value}) → 플레이어가 {drawCount}장 드로우");
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
                Debug.Log($"[Ability_DiceRollDraw] 홀수({value}) → 상대가 {drawCount}장 드로우");
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
