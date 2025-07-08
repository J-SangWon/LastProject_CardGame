using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DeckCardEntry
{
    public BaseCardData card;
    public int count;

    // JSON 저장용
    public string cardId; // BaseCardData의 고유 ID
}

[System.Serializable]
public class DeckData
{
    public string deckName;
    public List<DeckCardEntry> mainDeck = new List<DeckCardEntry>();
    public List<DeckCardEntry> extraDeck = new List<DeckCardEntry>();
}


