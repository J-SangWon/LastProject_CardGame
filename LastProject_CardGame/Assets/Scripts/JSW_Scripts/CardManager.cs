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

    public Dictionary<BaseCardData, int> ownedCardCounts = new Dictionary<BaseCardData, int>();

    public bool isTestMode = false;
    
    private DeckSaveManager deckSaveManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllCards();
            InitOwnedCards();
            if (isTestMode) GiveTestCardsToUser(); // 테스트용 카드 지급
            
            // DeckSaveManager 찾기
            deckSaveManager = FindObjectOfType<DeckSaveManager>();
            if (deckSaveManager == null)
            {
                // DeckSaveManager가 없다면 생성
                GameObject saveManagerObj = new GameObject("DeckSaveManager");
                deckSaveManager = saveManagerObj.AddComponent<DeckSaveManager>();
                DontDestroyOnLoad(saveManagerObj);
            }
            
            // 저장된 덱들 불러오기
            LoadAllDecks();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 저장된 모든 덱 불러오기
    private void LoadAllDecks()
    {
        if (deckSaveManager == null) return;
        
        List<string> deckNames = deckSaveManager.GetAllDeckNames();
        allDecks.Clear();
        
        foreach (string deckName in deckNames)
        {
            DeckData loadedDeck = deckSaveManager.LoadDeck(deckName);
            if (loadedDeck != null)
            {
                allDecks.Add(loadedDeck);
            }
        }
        
        // 마지막 선택된 덱 불러오기
        string lastDeckName = deckSaveManager.GetLastSelectedDeckName();
        if (!string.IsNullOrEmpty(lastDeckName))
        {
            currentDeck = GetDeckByName(lastDeckName);
        }
    }
    
    // 덱 저장
    public void SaveDeck(DeckData deck)
    {
        if (deckSaveManager == null || deck == null) return;
        
        deckSaveManager.SaveDeck(deck, deck.deckName);
        
        // 로컬 리스트에도 추가 (중복 방지)
        if (!allDecks.Contains(deck))
        {
            allDecks.Add(deck);
        }
    }
    
    // 현재 덱을 마지막 선택 덱으로 설정
    public void SetCurrentDeck(DeckData deck)
    {
        currentDeck = deck;
        if (deckSaveManager != null && deck != null)
        {
            deckSaveManager.SelectDeck(deck.deckName);
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
        
        // 새로 생성된 덱도 저장
        SaveDeck(newDeck);
        
        return newDeck;
    }

    // 덱 삭제
    public void DeleteDeck(DeckData deck)
    {
        if (deckSaveManager != null && deck != null)
        {
            deckSaveManager.DeleteDeck(deck.deckName);
        }
        
        allDecks.Remove(deck);
        
        // 삭제된 덱이 현재 덱이었다면 초기화
        if (currentDeck == deck)
        {
            currentDeck = null;
        }
    }

    // 덱 불러오기
    public DeckData GetDeckByName(string name)
    {
        return allDecks.Find(d => d.deckName == name);
    }

    // 모든 카드 리스트 반환 (예시)
    public List<BaseCardData> GetAllCards()
    {
        return allCards;
    }

    void InitOwnedCards()
    {
        foreach (var card in allCards)
            ownedCardCounts[card] = 0; // 기본 0장
        // 실제 보유 카드 데이터로 ownedCardCounts[card] = N; 세팅
    }

    // 테스트용: 모든 카드를 3장씩 보유하게 한다
    public void GiveTestCardsToUser()
    {
        foreach (var card in allCards)
            ownedCardCounts[card] = 3;
    }
}
