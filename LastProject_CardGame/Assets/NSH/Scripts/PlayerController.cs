using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    public string playerName = "�÷��̾�";
    public int lifePoints = 40;

    private InGameManager inGameManager;

    public void Initialize()
    {
        inGameManager = FindFirstObjectByType<InGameManager>();

        lifePoints = 40;
        Debug.Log($"{playerName} �ʱ�ȭ �Ϸ�. LP: {lifePoints}");
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
            Debug.Log($"{playerName}�� ���� �ƴϹǷ� ��ο��� �� �����ϴ�.");
            return;
        }

        Debug.Log($"{playerName} ��ο� ����!");
    }

    public void Battle()
    {
        if (!IsMyTurn())
        {
            Debug.Log($"{playerName}�� ���� �ƴϹǷ� ��Ʋ�� �� �����ϴ�.");
            return;
        }

        Debug.Log($"{playerName} ��Ʋ ������ ����!");
    }

    public void TakeDamage(int damage)
    {
        lifePoints -= damage;
        Debug.Log($"{playerName} ������ {damage} ����. ���� LP: {lifePoints}");

        if (lifePoints <= 0)
        {
            Debug.Log($"{playerName} �й�!");
            inGameManager.ChangeState(InGameManager.GameState.GameOver);
        }
    }
}
