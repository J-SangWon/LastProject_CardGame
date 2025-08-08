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
    public Button turnButton;

    [Header("Turn Indicator")]
    public GameObject playerTurnIndicator;
    public GameObject enemyTurnIndicator;
    public Image turnBackground;
    public Color playerTurnColor;
    public Color enemyTurnColor;

    [Header("Cards")]
    public GameObject cardObject;
    public GameObject enemyCardObject;

    [Header("Cost System")]
    public TextMeshProUGUI playerCostText;
    public TextMeshProUGUI enemyCostText;

    private int playerCurrentCost = 0;
    private int playerMaxCost = 0;
    private int enemyCurrentCost = 0;
    private int enemyMaxCost = 0;
    private const int MAX_COST_LIMIT = 10;

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

    private float turnTimer = 300f;
    private bool isTimerRunning = false;

    public TextMeshProUGUI turnCounterText; 
    private int turnCount = 1; // 턴 시작은 1부터

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

    public bool IsPlayerTurn() => isPlayerTurn;

    void Start()
    {
        currentPhase = GamePhase.FirstPhase;
        UpdatePhaseText();
        UpdateHealthUI();
        UpdateCostUI();
        UpdateTimerUI();
        UpdateTurnIndicators();
        UpdateTurnColor();
        turnButton.onClick.AddListener(OnTurnButtonClicked);
    }

    void Update()
    {
        if (isTimerRunning)
        {
            turnTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
    }

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

    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentPhase = GamePhase.MainPhase;
        phaseText.text = "Player's Turn";
        UpdatePhaseText();

        ResetPlayerCardAttacks();

        isTimerRunning = true;
        turnButton.interactable = true;
        turnTimer += 30f;
        if (turnTimer > 400f) turnTimer = 400f;

        if (playerMaxCost < MAX_COST_LIMIT)
            playerMaxCost++;
        playerCurrentCost = playerMaxCost;
        UpdateCostUI();

        UpdateTurnIndicators();
        UpdateTurnColor();
        //PlayTurnSound();
    }

    void StartEnemyTurn()
    {
        isPlayerTurn = false;
        currentPhase = GamePhase.MainPhase;
        phaseText.text = "Opponent's Turn";
        UpdatePhaseText();

        UpdateTurnIndicators();
        UpdateTurnColor();
        //PlayTurnSound();
    }

    void EndPlayerTurn()
    {
        isTimerRunning = false;
        turnButton.interactable = false;

        if (isPlayerTurn)
        {
            StartCoroutine(EnemyTurn());
        }
        else
        {
            isPlayerTurn = true;
            currentPhase = GamePhase.FirstPhase;
            UpdatePhaseText();
            turnTimer += 30f;
            if (turnTimer > 400f) turnTimer = 400f;
            isTimerRunning = true;
            turnButton.interactable = true;

            if (playerMaxCost < MAX_COST_LIMIT)
                playerMaxCost++;
            playerCurrentCost = playerMaxCost;
            UpdateCostUI();

            UpdateTurnIndicators();
            UpdateTurnColor();
        }
    }

    IEnumerator EnemyTurn()
    {
        StartEnemyTurn();

        if (enemyMaxCost < MAX_COST_LIMIT)
            enemyMaxCost++;
        enemyCurrentCost = enemyMaxCost;
        UpdateCostUI();

        yield return new WaitForSeconds(2f);
        EndPlayerTurn();
    }
    private void UpdateTurnUI()
    {
        string currentPlayer = isPlayerTurn ? "내 턴" : "적 턴";
        turnCounterText.text = $"턴 {turnCount} ({currentPlayer})";
        phaseText.text = currentPhase.ToString();
        UpdateTimerUI();
    }
    void OnTurnButtonClicked()
    {
        if (!isPlayerTurn) return;

        switch (currentPhase)
        {
            case GamePhase.FirstPhase:
                StartPlayerTurn();
                break;
            case GamePhase.MainPhase:
                StartBattlePhase();
                break;
            case GamePhase.BattlePhase:
                StartEndPhase();
                break;
            case GamePhase.EndPhase:
                EndPlayerTurn();
                break;
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

    void UpdatePhaseText()
    {
        switch (currentPhase)
        {
            case GamePhase.FirstPhase:
                phaseText.text = "First Phase";
                break;
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

    void UpdateCostUI()
    {
        playerCostText.text = $"Player Cost: {playerCurrentCost}/{playerMaxCost}";
        enemyCostText.text = $"Enemy Cost: {enemyCurrentCost}/{enemyMaxCost}";
    }

    void UpdateTurnIndicators()
    {
        if (playerTurnIndicator != null)
            playerTurnIndicator.SetActive(isPlayerTurn);
        if (enemyTurnIndicator != null)
            enemyTurnIndicator.SetActive(!isPlayerTurn);
    }

    void UpdateTurnColor()
    {
        if (turnBackground != null)
            turnBackground.color = isPlayerTurn ? playerTurnColor : enemyTurnColor;
    }

    void ResetPlayerCardAttacks()
    {
        var cards = FindObjectsByType<CardUI>(FindObjectsSortMode.None);
        foreach (var cardUI in cards)
        {
            if (cardUI.isOnField && cardUI.cardData != null &&
                GameManager.Instance.isPlayerTurn)
            {
                cardUI.ResetAttackFlag();
            }
        }
    }
}
