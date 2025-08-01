using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Text phaseText;  // 화면에 표시될 텍스트
    public Button turnButton;  // 턴 전환 버튼
    private GamePhase currentPhase;  // 현재 게임 페이즈
    private bool isPlayerTurn = true;  // 플레이어 턴 여부 (상대 턴으로 넘어가는 기능을 추가할 예정)

    void Start()
    {
        // 게임 시작 시 'Main Phase'로 시작
        currentPhase = GamePhase.MainPhase;
        UpdatePhaseText();

        // 턴 전환 버튼 클릭 이벤트 설정
        turnButton.onClick.AddListener(OnTurnButtonClicked);
    }

    // 턴 전환 버튼 클릭 시 호출되는 메서드
    void OnTurnButtonClicked()
    {
        // 텍스트 슬라이드 아웃 애니메이션
        StartCoroutine(SlideTextOut(() =>
        {
            // 현재 페이즈에 맞는 행동을 처리하고 다음 페이즈로 변경
            ChangePhase();

            // 새로운 페이즈 텍스트 슬라이드 인
            StartCoroutine(SlideTextIn());
        }));
    }

    // 텍스트가 왼쪽으로 슬라이드 아웃하는 애니메이션
    IEnumerator SlideTextOut(System.Action onComplete)
    {
        Vector3 startPos = phaseText.rectTransform.localPosition;
        Vector3 endPos = new Vector3(-2000, startPos.y, startPos.z);  // 왼쪽으로 슬라이드 아웃

        float time = 0f;
        float duration = 0.5f;  // 애니메이션 시간

        while (time < duration)
        {
            time += Time.deltaTime;
            phaseText.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, time / duration);
            yield return null;
        }

        phaseText.rectTransform.localPosition = endPos;
        onComplete?.Invoke();  // 애니메이션 완료 후 호출될 콜백
    }

    // 텍스트가 오른쪽에서 중앙으로 슬라이드 인하는 애니메이션
    IEnumerator SlideTextIn()
    {
        Vector3 startPos = new Vector3(2000, phaseText.rectTransform.localPosition.y, phaseText.rectTransform.localPosition.z);  // 오른쪽에서 시작
        Vector3 endPos = new Vector3(0, phaseText.rectTransform.localPosition.y, phaseText.rectTransform.localPosition.z);  // 중앙으로 이동

        phaseText.rectTransform.localPosition = startPos;

        float time = 0f;
        float duration = 0.5f;  // 애니메이션 시간

        while (time < duration)
        {
            time += Time.deltaTime;
            phaseText.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, time / duration);
            yield return null;
        }

        phaseText.rectTransform.localPosition = endPos;
    }

    // 현재 페이즈를 변경하고 텍스트를 업데이트하는 메서드
    void ChangePhase()
    {
        switch (currentPhase)
        {
            case GamePhase.MainPhase:
                currentPhase = GamePhase.BattlePhase;
                break;
            case GamePhase.BattlePhase:
                currentPhase = GamePhase.EndPhase;
                break;
            case GamePhase.EndPhase:
                // 엔드 페이즈가 끝나면 상대방 턴으로 넘어가는 로직을 추가할 수 있습니다.
                EndPlayerTurn();
                break;
        }

        UpdatePhaseText();
    }

    // 게임 페이즈에 맞는 텍스트 업데이트
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
    }

    // 플레이어 턴이 끝나면 상대 턴으로 넘어가는 메서드 (추후 상대 턴 로직 추가 가능)
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
}
