using UnityEngine;
using System.Collections.Generic;

public class DeckBuilder : MonoBehaviour
{
    public CardManager cardManager;
    public DeckData currentDeck;

    // 덱 생성
    public void CreateDeck(string deckName)
    {
        currentDeck = cardManager.CreateDeck(deckName);
    }

    // 카드 추가
    public bool AddCard(BaseCardData card)
    {
        //중복/최대 수 제한 등 체크 가능
        if (currentDeck.cards.Count >= 50) return false;
        currentDeck.cards.Add(card);
        return true;
    }

    // 카드 제거
    public void RemoveCard(BaseCardData card)
    {
        currentDeck.cards.Remove(card);
    }

    // 덱 비우기
    public void ClearDeck()
    {
        currentDeck.cards.Clear();
    }

    // 덱 저장/불러오기 등 추가 가능
}
