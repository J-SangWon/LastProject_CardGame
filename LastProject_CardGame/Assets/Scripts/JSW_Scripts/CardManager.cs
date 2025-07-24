using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    // 모든 카드 데이터 저장
    public List<BaseCardData> allCards = new List<BaseCardData>();

    // 카드 이름으로 빠르게 찾기 위한 Dictionary
    public Dictionary<string, BaseCardData> cardDict = new Dictionary<string, BaseCardData>();
    // 카드 ID로 빠르게 찾기 위한 Dictionary
    public Dictionary<string, BaseCardData> cardIdDict = new Dictionary<string, BaseCardData>();

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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadAllCards();
        RefreshOwnedCardCounts();
        if (isTestMode) GiveTestCardsToUser();

        // DeckSaveManager 찾기
        deckSaveManager = FindAnyObjectByType<DeckSaveManager>();
        if (deckSaveManager == null)
        {
            GameObject saveManagerObj = new GameObject("DeckSaveManager");
            deckSaveManager = saveManagerObj.AddComponent<DeckSaveManager>();
            DontDestroyOnLoad(saveManagerObj);
        }

        // 저장된 덱들 불러오기 (null 체크)
        if (deckSaveManager != null)
            LoadAllDecks();
        else
            Debug.LogWarning("DeckSaveManager가 생성되지 않았습니다!");
    }

    public void RefreshOwnedCardCounts()
    {
        ownedCardCounts.Clear();
        var ownedList = PlayerCardCollectionManager.Instance.GetOwnedCardDataList();
        foreach (var (cardData, count) in ownedList)
        {
            if (cardData != null)
                ownedCardCounts[cardData] = count;
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
        cardIdDict.Clear();

        BaseCardData[] cards = Resources.LoadAll<BaseCardData>("CardData");
        foreach (var card in cards)
        {
            allCards.Add(card);
            cardDict[card.cardName] = card;
            cardIdDict[card.cardId] = card;
        }
    }
    
    // 카드 이름으로 카드 데이터 가져오기
    public BaseCardData GetCardByName(string name)
    {
        cardDict.TryGetValue(name, out var card);
        return card;
    }
    // 카드 ID로 카드 데이터 가져오기
    public BaseCardData GetCardById(string cardId)
    {
        cardIdDict.TryGetValue(cardId, out var card);
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

    // 테스트용: 모든 카드를 3장씩 보유하게 한다
    public void GiveTestCardsToUser()
    {
        foreach (var card in allCards)
            ownedCardCounts[card] = 3;
    }

    // 카드의 전체 소유 개수 반환
    public int GetOwnedCardCount(BaseCardData card)
    {
        return PlayerCardCollectionManager.Instance.GetCardCount(card.cardId);
    }

    // 디버깅용: 현재 소유 카드 정보 출력
    [ContextMenu("소유 카드 정보 출력")]
    public void PrintOwnedCards()
    {
        var pcm = PlayerCardCollectionManager.Instance;
        Debug.Log("=== 현재 소유 카드 정보 ===");
        Debug.Log($"크래프트 포인트: {pcm.collection.craftPoint}");
        Debug.Log($"소유 카드 수: {pcm.collection.ownedCards.Count}개");
        
        foreach (var entry in pcm.collection.ownedCards)
        {
            var card = PlayerCardCollectionManager.FindCardDataById(entry.cardId);
            string cardName = card != null ? card.cardName : "알 수 없는 카드";
            Debug.Log($"- {cardName} (ID: {entry.cardId}): {entry.count}장");
        }
    }

    // 카드의 추가 가능 개수(전체 소유 - 현재 덱에 들어간 개수)
    public int GetAvailableCardCount(BaseCardData card)
    {
        int owned = GetOwnedCardCount(card);
        int inMainDeck = 0;
        int inExtraDeck = 0;
        if (currentDeck != null)
        {
            inMainDeck = currentDeck.mainDeck
                .FindAll(e => e.card != null && e.card.cardId == card.cardId)
                .Sum(e => e.count);
            inExtraDeck = currentDeck.extraDeck
                .FindAll(e => e.card != null && e.card.cardId == card.cardId)
                .Sum(e => e.count);
        }
        return owned - (inMainDeck + inExtraDeck);
    }

    public bool TryCraftCard(string cardId)
    {
        if (!cardIdDict.TryGetValue(cardId, out var card)) return false;
        var pcm = PlayerCardCollectionManager.Instance;
        if (!card.canCraft || pcm.collection.craftPoint < card.craftCost) return false;

        // 포인트 차감
        pcm.collection.craftPoint -= card.craftCost;

        // 카드 소유 정보 갱신
        var entry = pcm.collection.ownedCards.Find(e => e.cardId == cardId);
        if (entry != null)
            entry.count += 1;
        else
            pcm.collection.ownedCards.Add(new PlayerCardEntry { cardId = cardId, count = 1 });

        pcm.SaveCollection();
        return true;
    }

    public bool TryDisenchantCard(string cardId)
    {
        Debug.Log($"TryDisenchantCard 호출됨 - cardId: {cardId}");
        
        if (!cardIdDict.TryGetValue(cardId, out var card)) 
        {
            Debug.Log($"카드를 찾을 수 없음 - cardId: {cardId}");
            return false;
        }
        
        Debug.Log($"카드 찾음 - {card.cardName}");
        
        var pcm = PlayerCardCollectionManager.Instance;
        var entry = pcm.collection.ownedCards.Find(e => e.cardId == cardId);
        
        Debug.Log($"소유 카드 엔트리: {(entry != null ? $"count={entry.count}" : "null")}");
        Debug.Log($"분해 가능: {card.canDisenchant}");
        
        if (!card.canDisenchant || entry == null || entry.count <= 0) 
        {
            Debug.Log($"분해 조건 불만족 - canDisenchant: {card.canDisenchant}, entry: {(entry != null ? "있음" : "없음")}, count: {(entry != null ? entry.count : 0)}");
            return false;
        }

        // 카드 소유 개수 차감
        entry.count -= 1;
        // 포인트 증가
        pcm.collection.craftPoint += card.disenchantReward;

        Debug.Log($"분해 완료 - 남은 개수: {entry.count}, 획득 포인트: {card.disenchantReward}");

        // 0장 이하가 되면 리스트에서 제거
        if (entry.count <= 0)
            pcm.collection.ownedCards.Remove(entry);

        // --- 덱에서 초과분 자동 제거 ---
        int totalInDecks = 0;
        foreach (var deck in allDecks)
        {
            int inMain = deck.mainDeck.FindAll(e => e.card != null && e.card.cardId == cardId).Sum(e => e.count);
            int inExtra = deck.extraDeck.FindAll(e => e.card != null && e.card.cardId == cardId).Sum(e => e.count);
            totalInDecks += inMain + inExtra;
        }
        int ownedNow = pcm.GetCardCount(cardId); // 분해 후 소유 수
        if (totalInDecks > ownedNow)
        {
            int toRemove = totalInDecks - ownedNow;
            foreach (var deck in allDecks)
            {
                // 메인 덱에서 제거
                foreach (var entryDeck in deck.mainDeck.ToList())
                {
                    if (entryDeck.card != null && entryDeck.card.cardId == cardId && toRemove > 0)
                    {
                        int removeCount = Math.Min(entryDeck.count, toRemove);
                        entryDeck.count -= removeCount;
                        toRemove -= removeCount;
                        if (entryDeck.count <= 0)
                            deck.mainDeck.Remove(entryDeck);
                    }
                }
                // 엑스트라 덱에서 제거
                foreach (var entryDeck in deck.extraDeck.ToList())
                {
                    if (entryDeck.card != null && entryDeck.card.cardId == cardId && toRemove > 0)
                    {
                        int removeCount = Math.Min(entryDeck.count, toRemove);
                        entryDeck.count -= removeCount;
                        toRemove -= removeCount;
                        if (entryDeck.count <= 0)
                            deck.extraDeck.Remove(entryDeck);
                    }
                }
                DeckBuilder.Instance?.OnDeckChanged?.Invoke();
                if (toRemove <= 0) break;
            }
        }
        // --- 덱에서 초과분 자동 제거 끝 ---

        pcm.SaveCollection();
        return true;
    }
}
