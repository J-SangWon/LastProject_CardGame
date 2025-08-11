using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ===== 싱글턴 =====
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ===== Enum & 상태 변수 =====
    public GamePhase CurrentPhase => currentPhase;
    public bool IsPlayerTurn() => isPlayerTurn;
    public bool IsEnemyTurn() => !isPlayerTurn;

    private GamePhase currentPhase = GamePhase.None;
    private bool isPlayerTurn = true;
    private int turnCount = 1;

    private int playerHealth = 40;
    private int enemyHealth = 40;

    private float turnTimer = 300f;
    private bool isTimerRunning = false;

    // ===== UI 요소 =====
    [Header("UI - Text")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI turnText;          // "내 턴"/"적 턴"
    public TextMeshProUGUI turnNumberText;    // "턴 1" 등

    [Header("UI - Button")]
    public Button turnButton;

    [Header("UI - Turn Background")]
    public Image turnBackground;
    public Color playerTurnColor = Color.green;
    public Color enemyTurnColor = Color.red;

    [Header("Cards")]
    public GameObject cardObject;
    public GameObject enemyCardObject;

    // ===== 초기화 =====
    void Start()
    {
        currentPhase = GamePhase.FirstPhase;
        UpdateAllUI();
        turnButton.onClick.AddListener(OnTurnButtonClicked);
    }

    // ===== 매 프레임 업데이트 =====
    void Update()
    {
        if (isTimerRunning)
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer < 0) turnTimer = 0;
            UpdateTimerUI();
        }
    }

    // ===== 공개 메서드 =====
    public void DealDamageToPlayer(bool isPlayer, int dmg)
    {
        if (isPlayer)
            playerHealth = Mathf.Max(0, playerHealth - dmg);
        else
            enemyHealth = Mathf.Max(0, enemyHealth - dmg);

        UpdateHealthUI();

        if (playerHealth <= 0)
        {
            Debug.Log("플레이어가 패배했습니다.");
            // TODO: 게임 오버 처리
        }
        if (enemyHealth <= 0)
        {
            Debug.Log("적이 패배했습니다.");
            // TODO: 승리 처리
        }
    }

    void StartPlayerTurn(GamePhase startPhase = GamePhase.MainPhase)
    {
        isPlayerTurn = true;
        currentPhase = startPhase;

        ResetPlayerCardAttacks();

        isTimerRunning = true;

        turnTimer = Mathf.Min(turnTimer + 30f, 400f);

        // 코스트 매니저 호출
        CostManager.Instance.StartPlayerTurn();

        UpdateAllUI();

        if (currentPhase == GamePhase.FirstPhase)
        {
            // 버튼 비활성화
            turnButton.interactable = false;

            StartCoroutine(FirstPhaseDrawAndGoMainPhase());
        }
        else
        {
            // 메인페이즈 이상부터는 버튼 활성화
            turnButton.interactable = true;
        }
    }

    IEnumerator FirstPhaseDrawAndGoMainPhase()
    {
        yield return new WaitForSeconds(2f);

        // 플레이어 카드 매니저에서 카드 드로우
        PlayerCardManager.Instance.DrawCards(1);

        // 메인페이즈로 전환
        currentPhase = GamePhase.MainPhase;

        // 버튼 활성화
        turnButton.interactable = true;

        UpdateAllUI();
    }


    void StartEnemyTurn()
    {
        isPlayerTurn = false;
        currentPhase = GamePhase.MainPhase;

        isTimerRunning = true;
        turnButton.interactable = false;

        //  코스트 매니저 호출
        CostManager.Instance.StartEnemyTurn();

        UpdateAllUI();
    }

    void EndPlayerTurn()
    {
        isTimerRunning = false;
        turnButton.interactable = false;

        turnCount++;

        if (isPlayerTurn)
        {
            StartCoroutine(EnemyTurnCoroutine());
        }
        else
        {
            currentPhase = GamePhase.FirstPhase;
            UpdateAllUI();
            StartPlayerTurn(currentPhase);
        }
    }

    IEnumerator EnemyTurnCoroutine()
    {
        StartEnemyTurn();

        // TODO: 적 AI 처리

        yield return new WaitForSeconds(2f);

        EndPlayerTurn();
    }

    void OnTurnButtonClicked()
    {
        if (!isPlayerTurn)
            return;

        switch (currentPhase)
        {
            case GamePhase.None:
                break;

            case GamePhase.FirstPhase:
                currentPhase = GamePhase.MainPhase; // 페이즈 전환
                UpdateAllUI();
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
        UpdateAllUI();
    }

    void StartEndPhase()
    {
        currentPhase = GamePhase.EndPhase;

        // 버튼 비활성화
        turnButton.interactable = false;

        UpdateAllUI();
        StartCoroutine(EndPhaseAndAutoTurnCoroutine());
    }

    IEnumerator EndPhaseAndAutoTurnCoroutine()
    {
        yield return new WaitForSeconds(2f);
        EndPlayerTurn();
    }

    // ===== UI 갱신 함수들 =====
    void UpdateAllUI()
    {
        UpdatePhaseText();
        UpdateHealthUI();
        UpdateTimerUI();
        UpdateTurnColor();
        UpdateTurnUI();

        //  코스트 UI 갱신은 CostManager에서 처리
        CostManager.Instance.UpdateCostUI();
    }

    void UpdatePhaseText()
    {
        switch (currentPhase)
        {
            case GamePhase.None:
                phaseText.text = "";
                break;
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

    void UpdateTurnColor()
    {
        if (turnBackground != null)
            turnBackground.color = isPlayerTurn ? playerTurnColor : enemyTurnColor;
    }

    void UpdateTurnUI()
    {
        turnNumberText.text = $"Turn {turnCount}";

        if (isPlayerTurn)
            turnText.text = "Player Turn";
        else
            turnText.text = "Enemy Turn";
    }

    // ===== 기타 =====
    void ResetPlayerCardAttacks()
    {
        var cards = FindObjectsByType<CardUI>(FindObjectsSortMode.None);
        foreach (var cardUI in cards)
        {
            if (cardUI.isOnField && cardUI.cardData != null && isPlayerTurn)
            {
                cardUI.ResetAttackFlag();
            }
        }
    }
}
