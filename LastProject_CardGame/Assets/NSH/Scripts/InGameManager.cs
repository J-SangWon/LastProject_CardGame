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
    public InGameUIManager uiManager;  // Inspector에서 직접 연결

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

        uiManager.LogAction("게임을 시작합니다.");
        ChangeState(GameState.DrawPhase);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        uiManager.LogAction($"{newState} 시작");

        switch (newState)
        {
            case GameState.DrawPhase:
                currentPlayer.DrawCard();
                ChangeState(GameState.MainPhase);
                break;

            case GameState.MainPhase:
                // 수동 입력 대기
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
                uiManager.LogAction("게임 종료!");
                uiManager.ToggleSettingsPanel(); // 자동으로 설정창 열기
                break;
        }
    }

    void SwitchTurn()
    {
        currentPlayer = (currentPlayer == player1) ? player2 : player1;
        uiManager.LogAction($"{currentPlayer.playerName}의 턴입니다.");
    }
    public PlayerController GetCurrentPlayer()
    {
        return currentPlayer;
    }
}
