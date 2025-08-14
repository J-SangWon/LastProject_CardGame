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
        {
            Destroy(gameObject);
            return;
        }

        // UI 참조 누락 방지 (에디터에서 안 넣어도 경고)
        if (turnButton != null)
            turnButton.interactable = false; // 시작 시 버튼 비활성화
    }

    void Start()
    {
        InitGame();
    }

    private void InitGame()
    {
        // 초기 상태
        currentPhase = GamePhase.FirstPhase;
        isPlayerTurn = true;
        turnCount = 1;

        playerHealth = 40;
        enemyHealth = 40;

        playerCurrentCost = 0;
        playerMaxCost = 0;
        enemyCurrentCost = 0;
        enemyMaxCost = 0;

        turnTimer = 300f;
        isTimerRunning = false;

        UpdateAllUI();

        // 버튼 리스너 등록
        if (turnButton != null)
            turnButton.onClick.AddListener(OnTurnButtonClicked);

        // 플레이어 첫 턴 시작
        StartPlayerTurn(currentPhase);
    }

    // ===== Enum & 상태 변수 =====
    public GamePhase CurrentPhase => currentPhase;
    public bool IsPlayerTurn() => isPlayerTurn;
    public bool IsEnemyTurn() => !isPlayerTurn;
    public int TurnNumber => turnCount;

    private GamePhase currentPhase = GamePhase.None;
    private bool isPlayerTurn = true;
    private int turnCount = 1;

    private int playerHealth;
    private int enemyHealth;

    public int playerCurrentCost;
    private int playerMaxCost;
    public int enemyCurrentCost;
    private int enemyMaxCost;
    private const int MAX_COST_LIMIT = 10;

    private float turnTimer;
    private bool isTimerRunning;

    // ===== UI 요소 =====
    [Header("UI - Text")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI enemyHealthText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI playerCostText;
    public TextMeshProUGUI enemyCostText;
    public TextMeshProUGUI playerMaxCostText;
    public TextMeshProUGUI enemyMaxCostText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI turnNumberText;

    [Header("UI - Button")]
    public Button turnButton;

    [Header("UI - Turn Background")]
    public Image turnBackground;
    public Color playerTurnColor = Color.green;
    public Color enemyTurnColor = Color.red;

    [Header("Cards")]
    public GameObject cardObject;
    public GameObject enemyCardObject;
    
    [Header("AI")]
    public GameObject enemyAIObject; // 씬에 있는 EnemyAI 오브젝트 참조 (선택)

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

    // ===== 턴 로직 =====
    void StartPlayerTurn(GamePhase startPhase = GamePhase.MainPhase)
    {
        isPlayerTurn = true;
        currentPhase = startPhase;

        ResetPlayerCardAttacks();

        isTimerRunning = true;
        turnTimer = Mathf.Min(turnTimer + 30f, 400f);

        // 코스트 갱신
        if (playerMaxCost < MAX_COST_LIMIT)
            playerMaxCost++;
        playerCurrentCost = playerMaxCost;

        UpdateAllUI();

        if (currentPhase == GamePhase.FirstPhase)
        {
            // 턴 시작 시 지속 효과 트리거
            if (AuraManager.Instance != null)
            {
                AuraManager.Instance.TriggerTurnStartEffects();
            }
            
            turnButton.interactable = false;
            StartCoroutine(FirstPhaseDrawAndGoMainPhase());
        }
        else
        {
            turnButton.interactable = true;
        }
    }

    IEnumerator FirstPhaseDrawAndGoMainPhase()
    {
        yield return new WaitForSeconds(0.5f);

        PlayerCardManager.Instance?.DrawCards(1);

        currentPhase = GamePhase.MainPhase;
        turnButton.interactable = true;
        UpdateAllUI();
    }

    void StartEnemyTurn()
    {
        isPlayerTurn = false;
        currentPhase = GamePhase.MainPhase;

        ResetEnemyCardAttacks(); // 적 카드 공격 플래그 초기화

        isTimerRunning = true;
        turnButton.interactable = false; // AI 턴에서는 버튼 비활성화

        if (enemyMaxCost < MAX_COST_LIMIT)
            enemyMaxCost++;
        enemyCurrentCost = enemyMaxCost;

		// 적 턴 시작 시 1장 드로우
		OpponentCardManager.Instance?.DrawCards(1);

        UpdateAllUI();

        // AI 턴 시작 (타입 의존 제거: SendMessage 사용)
        try
        {
            GameObject aiGO = enemyAIObject != null ? enemyAIObject : GameObject.Find("EnemyAI");
            if (aiGO != null)
            {
                aiGO.SendMessage("StartAITurn", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[GameManager] EnemyAI 오브젝트를 찾을 수 없습니다. (이름: 'EnemyAI' 또는 인스펙터 참조 설정 필요) AI 턴을 건너뜁니다.");
                EndPlayerTurn();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] AI 턴 시작 중 오류 발생: {e.Message}");
            EndPlayerTurn();
        }
    }

    public void EndPlayerTurn()
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
    //ai 턴 처리 - AI가 자동으로 처리하므로 코루틴 제거
    IEnumerator EnemyTurnCoroutine()
    {
        StartEnemyTurn();
        // AI가 자동으로 턴을 진행하므로 여기서는 아무것도 하지 않음
        yield break;
    }

    // ===== 버튼 처리 =====
    void OnTurnButtonClicked()
    {
        // AI 턴 중에는 버튼 클릭 무시
        if (!isPlayerTurn)
            return;

        switch (currentPhase)
        {
            case GamePhase.FirstPhase:
                currentPhase = GamePhase.MainPhase;
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



    // ===== 페이즈 전환 =====
    void StartBattlePhase()
    {
        currentPhase = GamePhase.BattlePhase;
        UpdateAllUI();
    }

    void StartEndPhase()
    {
        currentPhase = GamePhase.EndPhase;
        turnButton.interactable = false;
        UpdateAllUI();
        
        // 턴 종료 시 지속 효과 트리거
        if (AuraManager.Instance != null)
        {
            AuraManager.Instance.TriggerTurnEndEffects();
        }
        
        StartCoroutine(EndPhaseAndAutoTurnCoroutine());
    }

    // === 적 턴용 배틀 페이즈 전환 ===
    public void StartEnemyBattlePhase()
    {
        if (!isPlayerTurn)
        {
            currentPhase = GamePhase.BattlePhase;
            turnButton.interactable = false;
            UpdateAllUI();
        }
    }

    IEnumerator EndPhaseAndAutoTurnCoroutine()
    {
        yield return new WaitForSeconds(2f);
        EndPlayerTurn();
    }

    // ===== UI 갱신 =====
    void UpdateAllUI()
    {
        UpdatePhaseText();
        UpdateHealthUI();
        UpdateTimerUI();
        UpdateCostUI();
        UpdateTurnColor();
        UpdateTurnUI();
    }

    void UpdatePhaseText()
    {
        string phaseName = currentPhase switch
        {
            GamePhase.FirstPhase => "First Phase",
            GamePhase.MainPhase => "Main Phase",
            GamePhase.BattlePhase => "Battle Phase",
            GamePhase.EndPhase => "End Phase",
            _ => ""
        };

        // AI 턴일 때는 턴 소유자 표시 추가
        if (!isPlayerTurn)
        {
            phaseText.text = $"Enemy {phaseName}";
        }
        else
        {
            phaseText.text = phaseName;
        }
    }

    void UpdateHealthUI()
    {
        playerHealthText.text = playerHealth.ToString();
        enemyHealthText.text = enemyHealth.ToString();
    }

    void UpdateTimerUI()
    {
        timerText.text = $"{Mathf.FloorToInt(turnTimer)}s";
    }

    public void UpdateCostUI()
    {
        playerCostText.text = playerCurrentCost.ToString();
        playerMaxCostText.text = playerMaxCost.ToString();
        enemyCostText.text = enemyCurrentCost.ToString();
        enemyMaxCostText.text = enemyMaxCost.ToString();
    }

    void UpdateTurnColor()
    {
        if (turnBackground != null)
            turnBackground.color = isPlayerTurn ? playerTurnColor : enemyTurnColor;
    }

    void UpdateTurnUI()
    {
        turnNumberText.text = $"Turn {turnCount}";
        turnText.text = isPlayerTurn ? "Player Turn" : "Enemy Turn";
    }

    // ===== 카드 공격 초기화 =====
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

    void ResetEnemyCardAttacks()
    {
        var cards = FindObjectsByType<CardUI>(FindObjectsSortMode.None);
        foreach (var cardUI in cards)
        {
            if (cardUI.isOnField && cardUI.cardData != null && !isPlayerTurn)
            {
                cardUI.ResetAttackFlag();
            }
        }
    }
    public bool CanSummonCard()
    {
        // Main Phase에서만 소환 가능
        return currentPhase == GamePhase.MainPhase && isPlayerTurn;
    }

    public bool CanAttack()
    {
        // Battle Phase에서만 공격 가능
        return currentPhase == GamePhase.BattlePhase && isPlayerTurn;
    }

    public bool CanSpendPlayerCost(int amount)
    {
        return playerCurrentCost >= amount;
    }

    public bool SpendPlayerCost(int amount)
    {
        if (playerCurrentCost >= amount)
        {
            playerCurrentCost -= amount;
            UpdateCostUI();
            return true;
        }
        return false;
    }
    public bool TrySpendPlayerCost(int amount)
    {
        if (playerCurrentCost < amount)
        {
            return false; // 코스트 부족
        }

        playerCurrentCost -= amount;
        UpdateCostUI();
        return true;
    }
    public bool CanSpendEnemyCost(int amount)
    {
        return enemyCurrentCost >= amount;
    }

    public bool SpendEnemyCost(int amount)
    {
        if (enemyCurrentCost >= amount)
        {
            enemyCurrentCost -= amount;
            UpdateCostUI();
            return true;
        }
        return false;
    }
    public void TakeDamageToPlayer(int amount)
    {
        playerHealth -= amount;
        if (playerHealth < 0) playerHealth = 0;
        UpdateHealthUI();
    }

    public void TakeDamageToEnemy(int amount)
    {
        enemyHealth -= amount;
        if (enemyHealth < 0) enemyHealth = 0;
        UpdateHealthUI();
    }

}
