using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    // 모든 카드 데이터 저장
    public List<BaseCardData> allCards = new List<BaseCardData>();

    // 카드 이름으로 빠르게 찾기 위한 Dictionary
    public Dictionary<string, BaseCardData> cardDict = new Dictionary<string, BaseCardData>();

    public List<DeckData> allDecks = new List<DeckData>();
    public DeckData currentDeck;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllCards();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Resources 폴더에서 모든 카드 데이터 불러오기
    private void LoadAllCards()
    {
        allCards.Clear();
        cardDict.Clear();

        BaseCardData[] cards = Resources.LoadAll<BaseCardData>("CardData");
        foreach (var card in cards)
        {
            allCards.Add(card);
            cardDict[card.cardName] = card;
        }
    }

    // 카드 이름으로 카드 데이터 가져오기
    public BaseCardData GetCardByName(string name)
    {
        cardDict.TryGetValue(name, out var card);
        return card;
    }

    // 희귀도 등으로 카드 리스트 가져오기
    public List<BaseCardData> GetCardsByRarity(CardRarity rarity)
    {
        return allCards.FindAll(card => card.rarity == rarity);
    }

    // 덱 생성
    public DeckData CreateDeck(string name)
    {
        DeckData newDeck = new DeckData { deckName = name };
        allDecks.Add(newDeck);
        return newDeck;
    }

    // 덱 삭제
    public void DeleteDeck(DeckData deck)
    {
        allDecks.Remove(deck);
    }

    // 덱 불러오기
    public DeckData GetDeckByName(string name)
    {
        return allDecks.Find(d => d.deckName == name);
    }

    // 덱에 카드 추가
    public void AddCardToDeck(DeckData deck, BaseCardData card)
    {
        deck.cards.Add(card);
    }

    // 덱에서 카드 제거
    public void RemoveCardFromDeck(DeckData deck, BaseCardData card)
    {
        deck.cards.Remove(card);
    }

    // 모든 카드 리스트 반환 (예시)
    public List<BaseCardData> GetAllCards()
    {
        return allCards;
    }
}
