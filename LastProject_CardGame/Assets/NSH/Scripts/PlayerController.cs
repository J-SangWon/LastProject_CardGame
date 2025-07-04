using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("플레이어 정보")]
    public string playerName = "플레이어";
    public int lifePoints = 40;

    private InGameManager inGameManager;

    public void Initialize()
    {
        inGameManager = FindFirstObjectByType<InGameManager>();

        lifePoints = 40;
        Debug.Log($"{playerName} 초기화 완료. LP: {lifePoints}");
    }

    public bool IsMyTurn()
    {
        return inGameManager != null && inGameManager.CurrentState != InGameManager.GameState.GameOver &&
               inGameManager.GetCurrentPlayer() == this;
    }

    public void DrawCard()
    {
        if (!IsMyTurn())
        {
            Debug.Log($"{playerName}의 턴이 아니므로 드로우할 수 없습니다.");
            return;
        }

        Debug.Log($"{playerName} 드로우 실행!");
    }

    public void Battle()
    {
        if (!IsMyTurn())
        {
            Debug.Log($"{playerName}의 턴이 아니므로 배틀할 수 없습니다.");
            return;
        }

        Debug.Log($"{playerName} 배틀 페이즈 실행!");
    }

    public void TakeDamage(int damage)
    {
        lifePoints -= damage;
        Debug.Log($"{playerName} 데미지 {damage} 입음. 현재 LP: {lifePoints}");

        if (lifePoints <= 0)
        {
            Debug.Log($"{playerName} 패배!");
            inGameManager.ChangeState(InGameManager.GameState.GameOver);
        }
    }
}
