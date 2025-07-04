using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckListUI : MonoBehaviour
{
    public Transform deckListContent; // 덱 리스트 Content
    public GameObject deckButtonPrefab; // 덱 하나를 표시할 프리팹
    public Button createDeckButton;

    void Start()
    {
        RefreshDeckList();

        createDeckButton.onClick.AddListener(() => {
            // 덱 생성 화면으로 이동
            // DeckBuilder/DeckMakingUI로 전환
        });
    }

    void RefreshDeckList()
    {
        foreach (Transform child in deckListContent)
            Destroy(child.gameObject);

        foreach (var deck in CardManager.Instance.allDecks)
        {
            GameObject deckBtnObj = Instantiate(deckButtonPrefab, deckListContent);
            DeckButtonUI deckBtn = deckBtnObj.GetComponent<DeckButtonUI>();
            deckBtn.SetDeck(deck);

            // 덱 선택(수정) 버튼
            deckBtn.selectButton.onClick.AddListener(() => {
                CardManager.Instance.currentDeck = deck;
                // 덱 수정 화면으로 이동
            });

            // 덱 삭제 버튼
            deckBtn.deleteButton.onClick.AddListener(() => {
                CardManager.Instance.DeleteDeck(deck);
                RefreshDeckList();
            });
        }
    }

    void Update()
    {
        
    }
}
