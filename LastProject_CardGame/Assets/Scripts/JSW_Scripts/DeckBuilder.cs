using UnityEngine;
using System.Collections.Generic;
using System;

public class DeckBuilder : MonoBehaviour
{
    public int maxCardCount = 3;
    public DeckData currentDeck;

    public static DeckBuilder Instance { get; private set; }

    public Action OnDeckChanged; // 덱 변경 시 호출되는 콜백

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // 덱 로드
    public void LoadDeck(DeckData deck)
    {
        currentDeck = deck;
        OnDeckChanged?.Invoke();
    }
    
    // 덱 저장
    public void SaveCurrentDeck()
    {
        if (currentDeck != null)
        {
            CardManager.Instance.SaveDeck(currentDeck);
            Debug.Log($"덱 '{currentDeck.deckName}'을 저장했습니다.");
        }
    }

    // 덱 생성
    public void CreateDeck(string deckName)
    {
        currentDeck = new DeckData();
        currentDeck.deckName = deckName;
        currentDeck.mainDeck = new List<DeckCardEntry>();
        currentDeck.extraDeck = new List<DeckCardEntry>();
        
        // 새로 생성된 덱 저장
        SaveCurrentDeck();
    }

    // 카드 추가
    public bool AddCardToMain(BaseCardData card)
    {
        int owned = CardManager.Instance.GetOwnedCardCount(card);
        var entry = currentDeck.mainDeck.Find(e => IsSameCard(e.card, card));
        int inDeck = entry != null ? entry.count : 0;
        if (inDeck + 1 > owned)
            return false; // 소유 개수 초과
        if(inDeck >= maxCardCount)
            return false; // 동일 카드 제한

        // 몬스터 카드일 때
        if (card.cardType == CardType.Monster)
        {
            MonsterCardData monster = card as MonsterCardData;
            if (monster == null) return false; // 안전장치

            // Normal, Effect만 메인덱 가능
            if (monster.monsterType != MonsterType.Normal && monster.monsterType != MonsterType.Effect)
                return false;
        }

        if (entry != null)
        {
            entry.count++;
        }
        else
        {
            if (currentDeck.mainDeck.Count >= 40) return false;
            currentDeck.mainDeck.Add(new DeckCardEntry { card = card, count = 1 });
        }
        OnDeckChanged?.Invoke();
        return true;
    }

    public bool AddCardToExtra(BaseCardData card)
    {
        int owned = CardManager.Instance.GetOwnedCardCount(card);
        var entry = currentDeck.extraDeck.Find(e => IsSameCard(e.card, card));
        int inDeck = entry != null ? entry.count : 0;
        if (inDeck + 1 > owned)
            return false; // 소유 개수 초과
        if (inDeck >= maxCardCount)
            return false; // 동일 카드 제한

        // 몬스터 카드만 엑스트라덱 가능
        if (card.cardType != CardType.Monster)
            return false;

        MonsterCardData monster = card as MonsterCardData;
        if (monster == null) return false; // 안전장치

        // Normal, Effect는 엑스트라덱 불가
        if (monster.monsterType == MonsterType.Normal || monster.monsterType == MonsterType.Effect)
            return false;

        if (entry != null)
        {
            entry.count++;
        }
        else
        {
            if (currentDeck.extraDeck.Count >= 15) return false;
            currentDeck.extraDeck.Add(new DeckCardEntry { card = card, count = 1 });
        }
        OnDeckChanged?.Invoke();
        return true;
    }

    // 카드 제거
    public void RemoveCardFromMain(BaseCardData card)
    {
        var entry = currentDeck.mainDeck.Find(e => IsSameCard(e.card, card));
        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0)
                currentDeck.mainDeck.Remove(entry);
        }
        OnDeckChanged?.Invoke();
        
    }

    public void RemoveCardFromExtra(BaseCardData card)
    {
        var entry = currentDeck.extraDeck.Find(e => IsSameCard(e.card, card));
        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0)
                currentDeck.extraDeck.Remove(entry);
        }
        OnDeckChanged?.Invoke();
        
    }

    // 덱 비우기
    public void ClearDeck()
    {
        currentDeck.mainDeck.Clear();
        currentDeck.extraDeck.Clear();
        
    }

    private bool IsSameCard(BaseCardData a, BaseCardData b)
    {
        // cardId로 통일하여 비교
        return a != null && b != null && a.cardId == b.cardId;
    }
}
