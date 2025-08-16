using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

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
    private bool isGameInputLocked = false;

    public bool IsInputLocked() => isGameInputLocked;
    private bool rpsDecided = false;
    private void InitGame()
    {
        // 초기 상태
        currentPhase = GamePhase.None; // 아직 턴 시작 X
        isPlayerTurn = true; // 기본값
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
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        // === 가위바위보로 선후공 결정 ===
        isGameInputLocked = true;
        ShowRPSPanel();
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
    private bool isDiscardSelectionActive = false;
    public bool IsDiscardSelectionActive => isDiscardSelectionActive;
    private int cardsToDiscardCount = 0;
    private List<GameObject> selectedCardsToDiscard = new List<GameObject>();

    [Header("RPS - Rock Paper Scissors")]
    public GameObject rpsPanel;
    public CanvasGroup rpsCanvasGroup;
    public Button rockButton;
    public Button paperButton;
    public Button scissorsButton;
    public TextMeshProUGUI rpsResultText;

    [Header("Win/Lose UI")]
    public GameObject winPanel;
    public GameObject losePanel;
    private enum RPSChoice { Rock, Paper, Scissors }

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
        if (turnTimer <= 0)
        {
            isTimerRunning = false;
            ShowLoseScreen();
        }
        if ((winPanel != null && winPanel.activeSelf) || (losePanel != null && losePanel.activeSelf))
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                ReturnToLobby();
            }
        }
    }
    void ShowRPSPanel()
    {
        if (rpsPanel != null)
        {
            rpsPanel.SetActive(true);
            if (rpsCanvasGroup != null)
            {
                rpsCanvasGroup.alpha = 0f;
                rpsCanvasGroup.DOFade(1f, 0.5f);
            }
        }

        rockButton.onClick.RemoveAllListeners();
        paperButton.onClick.RemoveAllListeners();
        scissorsButton.onClick.RemoveAllListeners();

        rockButton.onClick.AddListener(() => PlayerChooseRPS(RPSChoice.Rock));
        paperButton.onClick.AddListener(() => PlayerChooseRPS(RPSChoice.Paper));
        scissorsButton.onClick.AddListener(() => PlayerChooseRPS(RPSChoice.Scissors));

        AnimateRPSButtons();
    }

    void AnimateRPSButtons()
    {
        float startY = -300f;
        SetButtonStartPos(rockButton, startY);
        SetButtonStartPos(paperButton, startY);
        SetButtonStartPos(scissorsButton, startY);

        AnimateButtonIn(rockButton, 0f);
        AnimateButtonIn(paperButton, 0.1f);
        AnimateButtonIn(scissorsButton, 0.2f);
    }

    void SetButtonStartPos(Button btn, float offsetY)
    {
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, offsetY);
    }


    void AnimateButtonIn(Button btn, float delay)
    {
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutBack).SetDelay(delay);
    }
    void PlayerChooseRPS(RPSChoice playerChoice)
    {
        
        if (rpsDecided) return;

        // AI 무작위 선택
        RPSChoice enemyChoice = (RPSChoice)Random.Range(0, 3);

        string playerStr = playerChoice.ToString();
        string enemyStr = enemyChoice.ToString();

        // 승패 판정
        int result = GetRPSResult(playerChoice, enemyChoice);

        if (result == 1)
        {
            rpsResultText.text = $"플레이어: {playerStr}\n상대: {enemyStr}\n\n승리! 당신이 선공입니다.";
            isPlayerTurn = true;
        }
        else if (result == -1)
        {
            rpsResultText.text = $"플레이어: {playerStr}\n상대: {enemyStr}\n\n패배! 상대가 선공입니다.";
            isPlayerTurn = false;
        }
        else
        {
            rpsResultText.text = $"플레이어: {playerStr}\n상대: {enemyStr}\n\n무승부! 다시 선택하세요.";
            return; // 무승부면 다시 선택
        }

      
        rockButton.interactable = false;
        paperButton.interactable = false;
        scissorsButton.interactable = false;
        rpsDecided = true;

        // 1.5초 후 RPS 패널 닫고 게임 시작
        StartCoroutine(CloseRPSAndStartGame());
    }
    int GetRPSResult(RPSChoice player, RPSChoice enemy)
    {
        if (player == enemy) return 0; // 무승부

        if ((player == RPSChoice.Rock && enemy == RPSChoice.Scissors) ||
            (player == RPSChoice.Scissors && enemy == RPSChoice.Paper) ||
            (player == RPSChoice.Paper && enemy == RPSChoice.Rock))
            return 1; // 플레이어 승리

        return -1; // 플레이어 패배
    }
    IEnumerator CloseRPSAndStartGame()
    {
        yield return new WaitForSeconds(1.5f);

        if (rpsCanvasGroup != null)
        {
            rpsCanvasGroup.DOFade(0f, 0.4f);
            yield return new WaitForSeconds(0.4f);
        }

        if (rpsPanel != null)
            rpsPanel.SetActive(false);
        if (rockButton != null) rockButton.gameObject.SetActive(false);
        if (paperButton != null) paperButton.gameObject.SetActive(false);
        if (scissorsButton != null) scissorsButton.gameObject.SetActive(false);
        if (rpsResultText != null) rpsResultText.gameObject.SetActive(false);

        isGameInputLocked = false;

        // 선공이면 FirstPhase, 후공이면 상대 턴 MainPhase
        if (isPlayerTurn)
        {
            currentPhase = GamePhase.FirstPhase;
            StartPlayerTurn(currentPhase);
        }
        else
        {
            StartEnemyTurn();
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
        try
        {
            GameObject aiGO = enemyAIObject != null ? enemyAIObject : GameObject.Find("EnemyAI");
            if (aiGO != null)
            {
                aiGO.SendMessage("StartAITurn", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning("[GameManager] EnemyAI 오브젝트를 찾을 수 없습니다. AI 턴 건너뜀.");
                StartCoroutine(EnemyTurnFallback());
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] AI 턴 시작 오류: {e.Message}");
            StartCoroutine(EnemyTurnFallback());
        }

    }
    IEnumerator EnemyTurnFallback()
    {
        
        yield return new WaitForSeconds(0.8f);
        EndPlayerTurn(); // 적 턴 종료 → 플레이어 턴으로 전환
    }

    public void EndPlayerTurn()
    {
        isTimerRunning = false;
        turnButton.interactable = false;

        turnCount++;

        if (isPlayerTurn)
            StartCoroutine(EnemyTurnCoroutine());
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
        if (IsInputLocked()) return;

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
        int handCount = PlayerCardManager.Instance.playerHandZone.childCount;
        if (handCount > 6)
        {
            cardsToDiscardCount = handCount - 6;
            isDiscardSelectionActive = true;
            selectedCardsToDiscard.Clear();
            Debug.Log($"핸드 카드가 {handCount}장 초과! {cardsToDiscardCount}장 버리기 선택 필요.");
            // 여기서 UI 또는 카드 클릭 이벤트를 통해 선택 진행
            return;
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
    private void ConfirmDiscard()
    {
        foreach (var card in selectedCardsToDiscard)
        {
            // PlayerCardManager에서 카드 묘지 처리 가능
            card.transform.SetParent(PlayerCardManager.Instance.graveyardZone, false);
            card.transform.localPosition = Vector3.zero;
            card.GetComponent<CardUI>().isInHand = false;
            card.GetComponent<CardUI>()?.SetOutline(false);
        }

        selectedCardsToDiscard.Clear();
        isDiscardSelectionActive = false;

        // 턴 종료 진행
        StartCoroutine(EndPhaseAndAutoTurnCoroutine());
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

    public bool CanAttack(bool isPlayer)
    {
        // 1턴에는 누구도 공격 불가
        if (turnCount == 1) return false;

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
    // 코스트 복원: 취소/실패 시 되돌릴 때 사용
    public void RefundPlayerCost(int amount)
    {
        if (amount <= 0) return;
        playerCurrentCost = Mathf.Min(playerCurrentCost + amount, playerMaxCost);
        UpdateCostUI();
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
    // 적 코스트 복원: 취소/실패 시 되돌릴 때 사용
    public void RefundEnemyCost(int amount)
    {
        if (amount <= 0) return;
        enemyCurrentCost = Mathf.Min(enemyCurrentCost + amount, enemyMaxCost);
        UpdateCostUI();
    }
    public void SelectCardForDiscard(GameObject card)
    {
        if (!selectedCardsToDiscard.Contains(card))
        {
            selectedCardsToDiscard.Add(card);
            card.GetComponent<CardUI>()?.SetOutline(true); // 선택 표시
            if (selectedCardsToDiscard.Count >= cardsToDiscardCount)
            {
                ConfirmDiscard();
            }
        }
    }

    public void DeselectCardForDiscard(GameObject card)
    {
        if (selectedCardsToDiscard.Contains(card))
        {
            selectedCardsToDiscard.Remove(card);
            card.GetComponent<CardUI>()?.SetOutline(false);
        }
    }

    public bool IsCardSelectedForDiscard(GameObject card)
    {
        return selectedCardsToDiscard.Contains(card);
    }
    public void TakeDamageToPlayer(int amount)
    {
        playerHealth -= amount;
        if (playerHealth < 0) playerHealth = 0;
        UpdateHealthUI();

        if (playerHealth <= 0)
        {
            ShowLoseScreen();
        }
    }

    public void TakeDamageToEnemy(int amount)
    {
        enemyHealth -= amount;
        if (enemyHealth < 0) enemyHealth = 0;
        UpdateHealthUI();

        if (enemyHealth <= 0)
        {
            ShowWinScreen();
        }
    }
    public void ShowWinScreen()
    {
        isGameInputLocked = true; // 입력 차단
        turnButton.interactable = false; // 턴 버튼 비활성화
        if (winPanel != null) winPanel.SetActive(true);
    }

    public void ShowLoseScreen()
    {
        isGameInputLocked = true;
        turnButton.interactable = false;
        if (losePanel != null) losePanel.SetActive(true);
    }
    public void ReturnToLobby()
    {
        // 입력 차단
        isGameInputLocked = true;

        // 씬 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }
}
