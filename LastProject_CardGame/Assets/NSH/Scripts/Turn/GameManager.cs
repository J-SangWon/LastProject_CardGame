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

    //[Header("RPS (Rock Paper Scissors)")]
    //public GameObject rpsUI;         // 가위 바위 보 선택 UI
    //public Button rockButton;
    //public Button paperButton;
    //public Button scissorsButton;
    //public TextMeshProUGUI resultText;

    private enum RPSChoice { Rock, Paper, Scissors } // 가위, 바위, 보 선택을 위한 열거형
    private RPSChoice playerChoice;  // 플레이어의 선택
    private RPSChoice enemyChoice;   // 적의 선택

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
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public GamePhase CurrentPhase => currentPhase;

    void Start()
    {
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();
        turnButton.onClick.AddListener(OnTurnButtonClicked);

        UpdateHealthUI();
        UpdateTimerUI();
        UpdateCostUI();  // 코스트 UI 업데이트
        //rpsUI.SetActive(true);  // 게임 시작 시 가위 바위 보 UI 활성화
        //resultText.text = "Choose Rock, Paper, or Scissors!";

        //// 상대 랜덤 선택
        //enemyChoice = (RPSChoice)Random.Range(0, 3);  // 상대는 게임 시작 시 랜덤으로 선택

        //overlayPanel.SetActive(true);
    }

    void Update()
    {
        if (isTimerRunning)
        {
            turnTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
    }
    // 데미지를 처리하고 UI 갱신
    public void DealDamageToPlayer(bool isPlayer, int dmg)
    {
        if (isPlayer)
        {
            playerHealth -= dmg;
            if (playerHealth < 0) playerHealth = 0;
        }
        else
        {
            enemyHealth -= dmg;
            if (enemyHealth < 0) enemyHealth = 0;
        }

        UpdateHealthUI();

        // 사망 처리
        if (playerHealth <= 0)
        {
            Debug.Log("플레이어가 패배했습니다.");
            // 게임 오버 처리
        }

        if (enemyHealth <= 0)
        {
            Debug.Log("적이 패배했습니다.");
            // 승리 처리
        }
    }


    void PlayerSelect(RPSChoice choice)
    {
        playerChoice = choice;

        // 버튼 비활성화 (선택 후 더 이상 선택할 수 없게)
        //rockButton.interactable = false;
        //paperButton.interactable = false;
        //scissorsButton.interactable = false;

        //// 결과 계산 및 표시
        //string result = DetermineWinner(playerChoice, enemyChoice);
        //resultText.text = $"Player: {playerChoice}\nEnemy: {enemyChoice}\n{result}";

        //// 선후공 결정
        //if (result == "Player Wins")
        //{
        //    StartPlayerTurn();
        //}
        //else if (result == "Enemy Wins")
        //{
        //    StartEnemyTurn();
        //}
        //else
        //{
        //    // 비겼을 경우, 다시 선택 가능
        //    RestartRPS();
        //}
        //overlayPanel.SetActive(false);
    }

    void StartPlayerTurn()
    {
        phaseText.text = "Player's Turn";
        isPlayerTurn = true;
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        // 플레이어 카드들 공격 초기화
        ResetPlayerCardAttacks();

        // 타이머 & 버튼 활성화
        isTimerRunning = true;
        turnButton.interactable = true;
        turnTimer += 30f;
        if (turnTimer > 400f) turnTimer = 400f;

        if (playerMaxCost < MAX_COST_LIMIT)
            playerMaxCost++;
        playerCurrentCost = playerMaxCost;
        UpdateCostUI();
    }

    void ResetPlayerCardAttacks()
    {
        var cards = FindObjectsByType<CardUI>(FindObjectsSortMode.None);
        foreach (var cardUI in cards)
        {
            if (cardUI.isOnField && cardUI.cardData != null &&
                GameManager.Instance.isPlayerTurn) // 플레이어 몬스터만
            {
                cardUI.ResetAttackFlag();
            }
        }
    }


    void RestartRPS()
    {
        //rpsUI.SetActive(true);  // 다시 UI 활성화
        //resultText.text = "Choose Rock, Paper, or Scissors!";

        //// 버튼 활성화
        //rockButton.interactable = true;
        //paperButton.interactable = true;
        //scissorsButton.interactable = true;

        //overlayPanel.SetActive(true);
    }

    void StartEnemyTurn()
    {
        phaseText.text = "Opponent's Turn";
        isPlayerTurn = false;
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        //// 적 턴 시작
        //rpsUI.SetActive(false);  // 가위 바위 보 UI 비활성화
        //overlayPanel.SetActive(false);
    }

    string DetermineWinner(RPSChoice player, RPSChoice enemy)
    {
        // 가위 바위 보 규칙에 따라 승패를 결정하는 로직
        if (player == enemy)
        {
            return "It's a Tie";  // 비겼을 경우
        }
        else if ((player == RPSChoice.Rock && enemy == RPSChoice.Scissors) ||
                 (player == RPSChoice.Paper && enemy == RPSChoice.Rock) ||
                 (player == RPSChoice.Scissors && enemy == RPSChoice.Paper))
        {
            return "Player Wins";  // 플레이어 승리
        }
        else
        {
            return "Enemy Wins";  // 적 승리
        }
    }

    void OnTurnButtonClicked()
    {
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
            EndPlayerTurn();
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
        StartCoroutine(EndPhaseAndAutoTurn());
    }

    IEnumerator EndPhaseAndAutoTurn()
    {
        yield return new WaitForSeconds(1f);
        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        isTimerRunning = false;
        turnButton.interactable = false;

        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            phaseText.text = "Opponent's Turn";
            StartCoroutine(EnemyTurn());
        }
        else
        {
            isPlayerTurn = true;
            currentPhase = GamePhase.MainPhase;
            UpdatePhaseText();
            turnTimer += 30f;
            if (turnTimer > 400f) turnTimer = 400f;
            isTimerRunning = true;
            turnButton.interactable = true;

            if (playerMaxCost < MAX_COST_LIMIT)
                playerMaxCost++;
            playerCurrentCost = playerMaxCost;
            UpdateCostUI();  // 코스트 UI 업데이트
        }
    }

    IEnumerator EnemyTurn()
    {
        if (enemyMaxCost < MAX_COST_LIMIT)
            enemyMaxCost++;
        enemyCurrentCost = enemyMaxCost;
        UpdateCostUI();  // 코스트 UI 업데이트
        yield return new WaitForSeconds(2f);
        EndPlayerTurn();
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
        timerText.text = $"Time: {Mathf.FloorToInt(turnTimer)}s";
    }

    // 코스트 UI를 갱신하는 함수
    void UpdateCostUI()
    {
        playerCostText.text = $"Player Cost: {playerCurrentCost}/{playerMaxCost}";
        enemyCostText.text = $"Enemy Cost: {enemyCurrentCost}/{enemyMaxCost}";
    }
}
