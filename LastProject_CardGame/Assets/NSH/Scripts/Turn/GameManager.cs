using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI phaseText;  // 화면에 표시될 텍스트
    public Button turnButton;  // 턴 전환 버튼
    public Button attackButton;  // 공격 버튼
    private GamePhase currentPhase;  // 현재 게임 페이즈
    private bool isPlayerTurn = true;  // 플레이어 턴 여부
    private bool isAttackMode = false;  // 공격 모드 여부

    void Start()
    {
        // 게임 시작 시 'Main Phase'로 시작
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        // 턴 전환 버튼 클릭 이벤트 설정
        turnButton.onClick.AddListener(OnTurnButtonClicked);

        // 공격 버튼 클릭 이벤트 설정
        attackButton.onClick.AddListener(OnAttackButtonClicked);

        // 게임 시작 시 공격 버튼을 비활성화 (메인 페이즈에서는 공격 불가)
        attackButton.interactable = false;
    }

    // 턴 전환 버튼 클릭 시 호출되는 메서드
    void OnTurnButtonClicked()
    {
        // 현재 페이즈에 맞는 행동을 처리하고 다음 페이즈로 변경
        ChangePhase();
    }

    // 공격 버튼 클릭 시 호출되는 메서드
    void OnAttackButtonClicked()
    {
        if (currentPhase == GamePhase.BattlePhase)
        {
            // 공격 모드 활성화
            isAttackMode = true;
            Debug.Log("Attack mode enabled. Select a target.");
        }
    }

    // 현재 페이즈를 변경하고 텍스트를 업데이트하는 메서드
    void ChangePhase()
    {
        // 페이즈 변경 로직
        if (currentPhase == GamePhase.MainPhase)
            currentPhase = GamePhase.BattlePhase;
        else if (currentPhase == GamePhase.BattlePhase)
            currentPhase = GamePhase.EndPhase;
        else if (currentPhase == GamePhase.EndPhase)
            currentPhase = GamePhase.MainPhase;

        UpdatePhaseText();
    }

    // 게임 페이즈에 맞는 텍스트 업데이트
    void UpdatePhaseText()
    {
        switch (currentPhase)
        {
            case GamePhase.MainPhase:
                phaseText.text = "Main Phase";
                // 메인 페이즈에서는 공격 버튼 비활성화
                attackButton.interactable = false;
                break;
            case GamePhase.BattlePhase:
                phaseText.text = "Battle Phase";
                // 배틀 페이즈에서는 공격 버튼 활성화
                attackButton.interactable = true;
                break;
            case GamePhase.EndPhase:
                phaseText.text = "End Phase";
                // 엔드 페이즈에서는 공격 버튼 비활성화
                attackButton.interactable = false;
                break;
        }
    }

    // 플레이어 턴이 끝나면 상대 턴으로 넘어가는 메서드
    void EndPlayerTurn()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;  // 플레이어 턴 종료
            phaseText.text = "Opponent's Turn";  // 상대 턴으로 전환되는 텍스트
            Debug.Log("Opponent's turn begins...");
        }
        else
        {
            isPlayerTurn = true;  // 상대 턴이 끝나면 다시 플레이어 턴
            currentPhase = GamePhase.MainPhase;  // 다시 메인 페이즈로 설정
            UpdatePhaseText();
        }
    }

    // 대상을 클릭하면 공격을 수행하는 메서드
    public void OnTargetSelected(GameObject target)
    {
        if (isAttackMode && currentPhase == GamePhase.BattlePhase)
        {
            // 대상에 공격 처리 (단순히 로그를 출력하도록 함)
            Debug.Log("Attacking target: " + target.name);

            // 공격 후 공격 모드 종료
            isAttackMode = false;

            // 공격 후 버튼을 비활성화하여 다른 대상 선택을 방지
            attackButton.interactable = false;
        }
    }
}
