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

    [Header("�÷��̾� ����")]
    public PlayerController player1;
    public PlayerController player2;

    [Header("UI �� Ÿ�̸�")]
    public InGameUIManager uiManager;
    public float turnTimeLimit = 300f;

    private PlayerController currentPlayer;
    private PlayerController opponentPlayer;
    private GameState currentState;

    private float currentTurnTimer;
    private int turnCount = 0;
    private bool battlePhaseAllowed = false;

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (currentState == GameState.MainPhase || currentState == GameState.BattlePhase)
        {
            currentTurnTimer -= Time.deltaTime;
            uiManager.UpdateTurnTimer(currentTurnTimer);

            if (currentTurnTimer <= 0)
            {
                uiManager.LogAction("�� ���� �ð� �ʰ�! �ڵ� ���������� ����.");
                ChangeState(GameState.EndPhase);
            }
        }
    }

    void StartGame()
    {
        player1.Initialize();
        player2.Initialize();

        currentPlayer = player1;
        opponentPlayer = player2;

        player1.DrawMultipleCards(5);
        player2.DrawMultipleCards(6);

        uiManager.LogAction($"{currentPlayer.playerName} ���� ����!");
        battlePhaseAllowed = false;

        ChangeState(GameState.DrawPhase);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        uiManager.LogAction($"[Phase] {newState}");

        switch (newState)
        {
            case GameState.DrawPhase:
                if (currentPlayer.deck.Count == 0)
                {
                    GameOver(opponentPlayer);
                    return;
                }
                currentPlayer.DrawCard();
                ChangeState(GameState.MainPhase);
                break;

            case GameState.MainPhase:
                currentTurnTimer = turnTimeLimit;
                break;

            case GameState.BattlePhase:
                if (!battlePhaseAllowed)
                {
                    uiManager.LogAction("ù ���� ��Ʋ����� �ǳʶݴϴ�.");
                    ChangeState(GameState.EndPhase);
                }
                else
                {
                    currentPlayer.Battle();
                    ChangeState(GameState.EndPhase);
                }
                break;

            case GameState.EndPhase:
                HandleHandOverflow();
                SwitchTurn();
                ChangeState(GameState.DrawPhase);
                break;

            case GameState.GameOver:
                uiManager.LogAction("���� ����!");
                break;
        }
    }

    void HandleHandOverflow()
    {
        int overflow = currentPlayer.GetHandCount() - 7;
        if (overflow > 0)
        {
            uiManager.LogAction($"���� �ʰ�: {overflow}�� �����ϴ�.");
            currentPlayer.DiscardFromHand(overflow);
        }
    }

    void SwitchTurn()
    {
        (currentPlayer, opponentPlayer) = (opponentPlayer, currentPlayer);
        turnCount++;
        battlePhaseAllowed = true;
        uiManager.LogAction($"{currentPlayer.playerName}�� �� ����!");
    }

    public void GameOver(PlayerController loser)
    {
        currentState = GameState.GameOver;
        uiManager.LogAction($"{loser.playerName} �й�! ���� ����.");
    }

    public PlayerController GetCurrentPlayer() => currentPlayer;
    public GameState GetCurrentState() => currentState;
}
