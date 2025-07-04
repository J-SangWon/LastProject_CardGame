using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckMakingUI : MonoBehaviour
{
    public DeckBuilder deckBuilder;
    public Transform allCardsContent; // 전체 카드 리스트 Content
    public Transform deckCardsContent; // 덱 카드 리스트 Content
    public GameObject cardUIPrefab;
    public InputField deckNameInput;
    public Button saveButton;

    void Start()
    {
        // 덱 이름 입력 후 덱 생성
        deckBuilder.CreateDeck(deckNameInput.text);

        // 전체 카드 리스트 표시
        List<BaseCardData> allCards = deckBuilder.cardManager.GetAllCards();
        foreach (var card in allCards)
        {
            GameObject cardObj = Instantiate(cardUIPrefab, allCardsContent);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(card);

            // 카드 클릭 시 덱에 추가
            cardObj.GetComponent<Button>().onClick.AddListener(() => {
                if (deckBuilder.AddCard(card))
                    RefreshDeckList();
            });
        }

        saveButton.onClick.AddListener(() => {
            // 덱 저장 로직 등
            Debug.Log("덱 저장됨: " + deckNameInput.text);
        });
    }

    // 덱 리스트 UI 갱신
    void RefreshDeckList()
    {
        foreach (Transform child in deckCardsContent)
            Destroy(child.gameObject);

        foreach (var card in deckBuilder.currentDeck.cards)
        {
            GameObject cardObj = Instantiate(cardUIPrefab, deckCardsContent);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            cardUI.SetCard(card);

            // 덱 카드 클릭 시 제거
            cardObj.GetComponent<Button>().onClick.AddListener(() => {
                deckBuilder.RemoveCard(card);
                RefreshDeckList();
            });
        }
    }
}
