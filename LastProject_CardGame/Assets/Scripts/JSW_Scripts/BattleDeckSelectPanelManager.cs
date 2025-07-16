using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class BattleDeckSelectPanelManager : MonoBehaviour
{
    public Transform deckListParent; // DeckSelectContent 등 덱 버튼이 들어갈 곳
    public GameObject deckButtonPrefab; // 덱 버튼 프리팹
    public GameObject battleStartCheckPanel; // BattleStartCheckPanel
    public Button startButton;   // 인스펙터에서 할당
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
                selectedDeck = deck;
                DeckTransferManager.Instance.SetDeck(deck); // 덱 정보 임시 저장
                battleStartCheckPanel.SetActive(true);      // 체크 패널 활성화
                if (checkPanelText != null)
                    checkPanelText.text = $"'{deck.deckName}' 덱으로 시작하시겠습니까?";

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
        // 예: battleStartCheckPanel.GetComponent<BattleStartCheckPanel>().SetDeckInfo(deck);
    }

    // BattleStartCheckPanel에서 호출
    public void OnStartBattleButton()
    {
        // 덱 정보는 이미 DeckTransferManager.Instance에 저장됨
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGame");
    }
    
    public void OnCancelBattleButton()
    {
        battleStartCheckPanel.SetActive(false);
    }
}
