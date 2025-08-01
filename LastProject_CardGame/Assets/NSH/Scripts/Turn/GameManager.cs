using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI phaseText;
    public Button turnButton;
    public Button attackButton;
    public GameObject cardObject;  // 플레이어 카드 오브젝트
    public GameObject enemyCardObject;  // 적 카드 오브젝트

    public GameObject battleUI;
    public GameObject projectilePrefab;  // 발사체 프리팹

    private bool isBattleActive = false;
    private GamePhase currentPhase;
    private bool isPlayerTurn = true;

    private MonsterCardData playerCard;
    private MonsterCardData enemyCard;

    void Start()
    {
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        turnButton.onClick.AddListener(OnTurnButtonClicked);
        attackButton.onClick.AddListener(OnAttackButtonClicked);

        attackButton.interactable = false;
        SetAttackButtonPositionToCardCenter();

        battleUI.SetActive(false);
    }

    // 공격 버튼을 카드 중앙에 위치시키는 메서드
    void SetAttackButtonPositionToCardCenter()
    {
        if (cardObject != null && attackButton != null)
        {
            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            attackButton.GetComponent<RectTransform>().anchoredPosition = cardRect.anchoredPosition;
        }
    }

    void OnTurnButtonClicked()
    {
        ChangePhase();
    }

    // 공격 버튼 클릭 시 호출되는 메서드
    void OnAttackButtonClicked()
    {
        if (currentPhase == GamePhase.BattlePhase && isBattleActive)
        {
            PerformAttack(playerCard, enemyCard);
            EndPlayerTurn();
        }
    }

    // 전투 시작
    public void StartBattle(MonsterCardData player, MonsterCardData enemy)
    {
        playerCard = player;
        enemyCard = enemy;

        battleUI.SetActive(true);
        isBattleActive = true;

        attackButton.interactable = true;
        currentPhase = GamePhase.BattlePhase;
        UpdatePhaseText();
    }

    // 공격 처리
    void PerformAttack(MonsterCardData attacker, MonsterCardData target)
    {
        if (attacker == null || target == null)
            return;

        Debug.Log($"{attacker.cardName} attacks {target.cardName} for {attacker.attack} damage!");

        // 발사체 생성 (발사체 프리팹을 인스턴스화)
        GameObject projectile = Instantiate(projectilePrefab, cardObject.transform.position, Quaternion.identity);

        // 발사체 초기화
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.Initialize(enemyCardObject.transform, attacker.attack);  // 목표는 적 카드
    }

    void ChangePhase()
    {
        if (currentPhase == GamePhase.MainPhase)
        {
            currentPhase = GamePhase.BattlePhase;
            attackButton.gameObject.SetActive(true);  // 배틀 페이즈에서 공격 버튼 활성화
        }
        else if (currentPhase == GamePhase.BattlePhase)
        {
            currentPhase = GamePhase.EndPhase;
            attackButton.gameObject.SetActive(false);  // 엔드 페이즈에서는 공격 버튼 비활성화
        }
        else if (currentPhase == GamePhase.EndPhase)
        {
            currentPhase = GamePhase.MainPhase;
            attackButton.gameObject.SetActive(false);  // 메인 페이즈에서는 공격 버튼 비활성화
        }

        // 텍스트 업데이트
        UpdatePhaseText();
    }

    void UpdatePhaseText()
    {
        // 현재 페이즈에 따라 텍스트 변경
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

        // 텍스트가 변경된 후 확인용 로그 추가 (디버깅용)
        Debug.Log("Current Phase: " + phaseText.text);
    }

    void EndPlayerTurn()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            phaseText.text = "Opponent's Turn";
            Debug.Log("Opponent's turn begins...");
        }
        else
        {
            isPlayerTurn = true;
            currentPhase = GamePhase.MainPhase;
            UpdatePhaseText();
            battleUI.SetActive(false);  // 배틀 종료 후 배틀 UI 비활성화
            isBattleActive = false;
        }
    }
}
