using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class BattleDeckSelectPanelManager : MonoBehaviour
{
    public Transform deckListParent; // DeckSelectContent 등 덱 버튼이 들어갈 곳
    public GameObject deckButtonPrefab; // 덱 버튼 프리팹
    public GameObject battleStartCheckPanel; // BattleStartCheckPanel
    public Button startButton;   // 인스펙터에서 할당
    public TextMeshProUGUI startButtonText;
    public Button cancelButton;  // 인스펙터에서 할당
    public TextMeshProUGUI checkPanelText; // 체크 패널에 표시할 텍스트
    private DeckData selectedDeck;

    void Start()
    {
        // 덱 리스트 생성
        CreateDeckList();

        // 시작 패널 비활성화
        battleStartCheckPanel.SetActive(false);

        if (startButton != null)
            startButton.onClick.AddListener(OnStartBattleButton);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelBattleButton);
    }

    void CreateDeckList()
    {
        List<DeckData> allDecks = CardManager.Instance.allDecks;

        foreach (Transform child in deckListParent)
            Destroy(child.gameObject);

        foreach (var deck in allDecks)
        {
            GameObject btnObj = Instantiate(deckButtonPrefab, deckListParent);
            BattleDeckSelectButtonUI btnUI = btnObj.GetComponent<BattleDeckSelectButtonUI>();
            btnUI.SetDeck(deck);

            btnUI.selectButton.onClick.RemoveAllListeners();
            btnUI.selectButton.onClick.AddListener(() => {
                int totalCardCount = deck.mainDeck.Sum(entry => entry.count);

                if (totalCardCount < 30)
                {
                    SoundManager.Instance.PlaySFX("MENUSELECT_ERROR");

                    // 덱이 30장 미만일 때 경고 메시지 출력
                    battleStartCheckPanel.SetActive(true);
                    if (checkPanelText != null)
                        checkPanelText.text = "덱은 최소 30장이 필요합니다.";
                    startButton.interactable = false; // 시작 버튼 비활성화
                    startButtonText.text = "시작 불가";
                    return;
                }



                selectedDeck = deck;
                DeckTransferManager.Instance.SetDeck(deck); // 덱 정보 임시 저장

                SoundManager.Instance.PlaySFX("MENUSELECT_01");

                battleStartCheckPanel.SetActive(true);      // 체크 패널 활성화
                if (checkPanelText != null)
                    checkPanelText.text = $"'{deck.deckName}' 덱으로 시작하시겠습니까?";
                startButton.interactable = true; // 시작 버튼 활성화
                startButtonText.text = "시작"; 

                // battleStartCheckPanel.GetComponent<BattleStartCheckPanel>().SetDeckInfo(deck);
            });
        }
    }

    void OnDeckSelected(DeckData deck)
    {
        selectedDeck = deck;
        DeckTransferManager.Instance.SetDeck(deck);

        // 덱 정보 확인 패널 활성화
        battleStartCheckPanel.SetActive(true);

        // 여기서 battleStartCheckPanel에 덱 정보 표시(이름, 카드 수 등) 갱신
        // battleStartCheckPanel.GetComponent<BattleStartCheckPanel>().SetDeckInfo(deck);
    }

    public void OnStartBattleButton()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySFX("MENUSELECT_01");
        // 덱 정보는 이미 DeckTransferManager.Instance에 저장됨
        SceneManager.LoadScene("InGame");
    }
    
    public void OnCancelBattleButton()
    {
        SoundManager.Instance.PlaySFX("MENUSELECT_02");
        battleStartCheckPanel.SetActive(false);
    }
}
