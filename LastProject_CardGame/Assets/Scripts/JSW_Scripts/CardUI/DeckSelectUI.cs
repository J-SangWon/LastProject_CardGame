using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckSelectUI : MonoBehaviour
{
    public Transform deckSelectContent; // 덱 리스트 Content
    public GameObject deckButtonPrefab; // 덱 하나를 표시할 프리팹
    public Button createDeckButton;

    // 추가: 덱 수정 패널과 UI 연결
    public GameObject deckEditPanel;
    public DeckMakingUI deckMakingUI;

    void Start()
    {
        SetupUI();
        RefreshDeckList();
    }
    
    void SetupUI()
    {
        // 새 덱 만들기 버튼
        if (createDeckButton != null)
            createDeckButton.onClick.AddListener(CreateNewDeck);
    }

    public void RefreshDeckList()
    {
        foreach (Transform child in deckSelectContent)
            Destroy(child.gameObject);

        foreach (var deck in CardManager.Instance.allDecks)
        {
            GameObject deckBtnObj = Instantiate(deckButtonPrefab, deckSelectContent);
            DeckButtonUI deckBtn = deckBtnObj.GetComponent<DeckButtonUI>();
            deckBtn.SetDeck(deck);

            // 덱 선택(수정) 버튼
            deckBtn.selectButton.onClick.RemoveAllListeners();
            deckBtn.selectButton.onClick.AddListener(() => {
                // 마지막 선택 덱으로 설정
                CardManager.Instance.SetCurrentDeck(deck);
                
                // 실제 동작: 덱 수정 패널 활성화 및 덱 데이터 전달
                if (deckEditPanel != null) deckEditPanel.SetActive(true);
                if (deckMakingUI != null) deckMakingUI.OpenWithDeck(deck);
                // 덱 선택 화면 비활성화
                gameObject.SetActive(false);
            });

            // 덱 삭제 버튼
            deckBtn.deleteButton.onClick.RemoveAllListeners();
            deckBtn.deleteButton.onClick.AddListener(() => {
                CardManager.Instance.DeleteDeck(deck);
                RefreshDeckList();
            });
        }
    }
    
    // 새 덱 생성
    private void CreateNewDeck()
    {
        // 덱 이름을 자동으로 생성 (덱 개수+1)
        string newDeckName = "새 덱 " + (CardManager.Instance.allDecks.Count + 1);
        
        // 새 덱 생성
        DeckData newDeck = CardManager.Instance.CreateDeck(newDeckName);
        
        // 새로 생성된 덱을 현재 덱으로 설정
        CardManager.Instance.SetCurrentDeck(newDeck);
        
        // 덱 수정 화면으로 이동
        if (deckEditPanel != null) deckEditPanel.SetActive(true);
        if (deckMakingUI != null) deckMakingUI.OpenWithDeck(newDeck);
        gameObject.SetActive(false);
        
        Debug.Log($"새 덱 '{newDeckName}'을 생성했습니다.");
    }
}
