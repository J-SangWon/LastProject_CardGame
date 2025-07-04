using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public enum GameState
    {
        Waiting,
        DrawPhase,
        MainPhase,
        BattlePhase,
        EndPhase,
        GameOver
    }

    public GameState CurrentState { get; private set; }

    public PlayerController player1;
    public PlayerController player2;
    public InGameUIManager uiManager;  // Inspector���� ���� ����

    private PlayerController currentPlayer;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        player1.Initialize();
        player2.Initialize();
        currentPlayer = player1;

        uiManager.LogAction("������ �����մϴ�.");
        ChangeState(GameState.DrawPhase);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        uiManager.LogAction($"{newState} ����");

        switch (newState)
        {
            case GameState.DrawPhase:
                currentPlayer.DrawCard();
                ChangeState(GameState.MainPhase);
                break;

            case GameState.MainPhase:
                // ���� �Է� ���
                break;

            case GameState.BattlePhase:
                currentPlayer.Battle();
                ChangeState(GameState.EndPhase);
                break;

            case GameState.EndPhase:
                SwitchTurn();
                ChangeState(GameState.DrawPhase);
                break;

            case GameState.GameOver:
                uiManager.LogAction("���� ����!");
                uiManager.ToggleSettingsPanel(); // �ڵ����� ����â ����
                break;
        }
    }

    void SwitchTurn()
    {
        currentPlayer = (currentPlayer == player1) ? player2 : player1;
        uiManager.LogAction($"{currentPlayer.playerName}�� ���Դϴ�.");
    }
    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer;
    }
}
