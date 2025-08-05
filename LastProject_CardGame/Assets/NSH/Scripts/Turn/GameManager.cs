using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI timerText;
    public Button turnButton;  // 턴 전환 버튼

    [Header("Cards")]
    public GameObject cardObject;       // 플레이어 카드 오브젝트
    public GameObject enemyCardObject;  // 적 카드 오브젝트

    [Header("Cost System")]
    public TextMeshProUGUI playerCostText;
    public TextMeshProUGUI enemyCostText;

    private int playerCurrentCost = 0;
    private int playerMaxCost = 0;

    private int enemyCurrentCost = 0;
    private int enemyMaxCost = 0;

    private const int MAX_COST_LIMIT = 10;

    [Header("RPS (Rock Paper Scissors)")]
    public GameObject rpsUI;         // 가위 바위 보 선택 UI
    public Button rockButton;
    public Button paperButton;
    public Button scissorsButton;
    public TextMeshProUGUI resultText;

    private enum RPSChoice { Rock, Paper, Scissors }
    private RPSChoice playerChoice;
    private RPSChoice enemyChoice;


    private bool isBattleActive = false;
    private GamePhase currentPhase;
    private bool isPlayerTurn = true;

    private MonsterCardData playerCard;
    private MonsterCardData enemyCard;

    private int playerHealth = 40;
    private int enemyHealth = 40;

    private float turnTimer = 300f;      // 초기 타이머 (초)
    private bool isTimerRunning = false; // 타이머 작동 여부

    public GameObject overlayPanel;

    void Start()
    {
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();
        turnButton.onClick.AddListener(OnTurnButtonClicked);

        turnButton.onClick.AddListener(OnTurnButtonClicked);

        UpdateHealthUI();
        UpdateTimerUI();
        UpdateCostUI();
        // 가위 바위 보 UI 활성화
        rpsUI.SetActive(true);  // 게임 시작 시 가위 바위 보 UI 활성화
        resultText.text = "Choose Rock, Paper, or Scissors!";

        // 상대 랜덤 선택
        enemyChoice = (RPSChoice)Random.Range(0, 3);  // 상대는 게임 시작 시 랜덤으로 선택

        overlayPanel.SetActive(true);
    }

    void Update()
    {
        if (isTimerRunning)
        {
            turnTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
    }
    void PlayerSelect(RPSChoice choice)
    {
        playerChoice = choice;

        // 버튼 비활성화 (선택 후 더 이상 선택할 수 없게)
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;

        // 결과 계산 및 표시
        string result = DetermineWinner(playerChoice, enemyChoice);
        resultText.text = $"Player: {playerChoice}\nEnemy: {enemyChoice}\n{result}";

        // 선후공 결정
        if (result == "Player Wins")
        {
            StartPlayerTurn();
        }
        else if (result == "Enemy Wins")
        {
            StartEnemyTurn();
        }
        else
        {
            // 비겼을 경우, 다시 선택 가능
            RestartRPS();
        }
        overlayPanel.SetActive(false);
    }
    void StartPlayerTurn()
    {
        // 플레이어가 선공
        phaseText.text = "Player's Turn";
        isPlayerTurn = true;
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        // 추가 로직: 플레이어 턴 시작
        rpsUI.SetActive(false);  // 가위 바위 보 UI 비활성화
       
        overlayPanel.SetActive(false);
    }
    void RestartRPS()
    {
        rpsUI.SetActive(true);  // 다시 UI 활성화
        resultText.text = "Choose Rock, Paper, or Scissors!";

        // 버튼 활성화
        rockButton.interactable = true;
        paperButton.interactable = true;
        scissorsButton.interactable = true;

        overlayPanel.SetActive(true);
    }

    void StartEnemyTurn()
    {
        // 적이 선공
        phaseText.text = "Opponent's Turn";
        isPlayerTurn = false;
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        // 추가 로직: 적 턴 시작
        rpsUI.SetActive(false);  // 가위 바위 보 UI 비활성화
       
        overlayPanel.SetActive(false);
    }
    string DetermineWinner(RPSChoice player, RPSChoice enemy)
    {
        if (player == enemy)
        {
            return "It's a Tie";
        }
        else if ((player == RPSChoice.Rock && enemy == RPSChoice.Scissors) ||
                 (player == RPSChoice.Paper && enemy == RPSChoice.Rock) ||
                 (player == RPSChoice.Scissors && enemy == RPSChoice.Paper))
        {
            return "Player Wins";
        }
        else
        {
            return "Enemy Wins";
        }
    }

    void OnTurnButtonClicked()
    {
        // 각 페이즈 진행: 버튼 클릭 시 순차적으로 진행
        if (currentPhase == GamePhase.MainPhase)
        {
            StartBattlePhase();
        }
        else if (currentPhase == GamePhase.BattlePhase)
        {
            StartEndPhase();
        }
        else if (currentPhase == GamePhase.EndPhase)
        {
            EndPlayerTurn();  // 엔드 페이즈 끝나면 턴 전환
        }
    }

    void StartBattlePhase()
    {
        currentPhase = GamePhase.BattlePhase;
        UpdatePhaseText();
    }

    void StartEndPhase()
    {
        currentPhase = GamePhase.EndPhase;
        UpdatePhaseText();

        StartCoroutine(EndPhaseAndAutoTurn());  // 엔드 페이즈 끝난 후 자동으로 상대 턴
    }

    IEnumerator EndPhaseAndAutoTurn()
    {
        // 엔드 페이즈가 끝나면 상대 턴으로 넘어갑니다.
        yield return new WaitForSeconds(1f); // 1초 대기 (엔드 페이즈 후)

        EndPlayerTurn();  // 자동으로 상대 턴 시작
    }

    void EndPlayerTurn()
    {
        isTimerRunning = false;

        // 턴 전환 버튼 비활성화 (상대 턴)
        turnButton.interactable = false;

        if (isPlayerTurn)
        {
            // 플레이어 턴이 끝났으므로 상대 턴으로 전환
            isPlayerTurn = false;
            phaseText.text = "Opponent's Turn";
            Debug.Log("Opponent's turn begins...");
            StartCoroutine(EnemyTurn());
        }
        else
        {
            // 상대 턴이 끝났으면 다시 플레이어 턴으로 전환
            isPlayerTurn = true;
            currentPhase = GamePhase.MainPhase;
            UpdatePhaseText();

            // 내 턴 시작 시 타이머에 30초 추가
            turnTimer += 30f;
            if (turnTimer > 400f) turnTimer = 400f;  // 타이머 상한선 400초
            isTimerRunning = true;

            // 턴 전환 버튼 활성화 (플레이어 턴)
            turnButton.interactable = true;

            // 플레이어 코스트 회복
            if (playerMaxCost < MAX_COST_LIMIT)
                playerMaxCost++;
            playerCurrentCost = playerMaxCost;
            UpdateCostUI();
        }
    }

    IEnumerator EnemyTurn()
    {
        if (enemyMaxCost < MAX_COST_LIMIT)
            enemyMaxCost++;
        enemyCurrentCost = enemyMaxCost;
        UpdateCostUI();
        // 상대 턴 2초 대기 후 플레이어 턴으로 전환
        yield return new WaitForSeconds(2f);
        EndPlayerTurn();  // 상대 턴 끝내고 플레이어 턴 시작
    }
    bool TryUseCost(int amount)
    {
        if (playerCurrentCost >= amount)
        {
            playerCurrentCost -= amount;
            UpdateCostUI();
            return true;
        }
        return false;
    }

    void UpdatePhaseText()
    {
        switch (currentPhase)
        {
            case GamePhase.MainPhase:
                phaseText.text = "Main Phase";
                break;
            case GamePhase.BattlePhase:
                phaseText.text = "Battle Phase";
                break;
            case GamePhase.EndPhase:
                phaseText.text = "End Phase";
                break;
        }

        Debug.Log("Current Phase: " + phaseText.text);
    }

    void UpdateHealthUI()
    {
        playerHealthText.text = $"Player HP: {playerHealth}";
        enemyHealthText.text = $"Enemy HP: {enemyHealth}";
    }

    void UpdateTimerUI()
    {
        // 초 단위로 표시 (분/초 대신)
        timerText.text = $"Time Left: {Mathf.FloorToInt(turnTimer)}s"; // 초만 표시
    }
    void UpdateCostUI()
    {
        playerCostText.text = $"Cost: {playerCurrentCost}/{playerMaxCost}";
        enemyCostText.text = $"Cost: {enemyCurrentCost}/{enemyMaxCost}";
    }


    void OnTimerEnded()
    {
        Debug.Log("시간 종료! 플레이어 패배!");
        OnPlayerDefeat();
    }

    void OnPlayerVictory()
    {
        Debug.Log("플레이어 승리!");
        // TODO: 승리 처리 (게임 종료 UI 표시 등)
    }

    void OnPlayerDefeat()
    {
        Debug.Log("플레이어 패배...");
        // TODO: 패배 처리 (게임 종료 UI 표시 등)
    }
}
