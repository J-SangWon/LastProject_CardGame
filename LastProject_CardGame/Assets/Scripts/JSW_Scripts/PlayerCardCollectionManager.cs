using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerCardEntry
{
    public string cardId; // 카드 고유 ID (BaseCardData의 이름 등)
    public int count;     // 소유 개수
}

[System.Serializable]
public class PlayerCardCollectionData
{
    public List<PlayerCardEntry> ownedCards = new List<PlayerCardEntry>();
    public int craftPoint;
}

public class PlayerCardCollectionManager : MonoBehaviour
{
    private const string COLLECTION_FILE = "player_collection.json";
    public PlayerCardCollectionData collection = new PlayerCardCollectionData();
    public static PlayerCardCollectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCollection();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveCollection()
    {
        string json = JsonUtility.ToJson(collection, true);
        string path = Path.Combine(Application.persistentDataPath, COLLECTION_FILE);
        File.WriteAllText(path, json);
    }

    public void LoadCollection()
    {
        string path = Path.Combine(Application.persistentDataPath, COLLECTION_FILE);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            collection = JsonUtility.FromJson<PlayerCardCollectionData>(json);
        }
        else
        {
            collection = new PlayerCardCollectionData();
        }
    }

    // 카드 획득
    public void AddCard(string cardId, int count = 1)
    {
        var entry = collection.ownedCards.Find(e => e.cardId == cardId);
        if (entry != null)
            entry.count += count;
        else
            collection.ownedCards.Add(new PlayerCardEntry { cardId = cardId, count = count });
        SaveCollection();
    }

    // 카드 소모/삭제
    public void RemoveCard(string cardId, int count = 1)
    {
        var entry = collection.ownedCards.Find(e => e.cardId == cardId);
        if (entry != null)
        {
            entry.count -= count;
            if (entry.count <= 0)
                collection.ownedCards.Remove(entry);
            SaveCollection();
        }
    }

    // 카드 소유 개수 조회
    public int GetCardCount(string cardId)
    {
        var entry = collection.ownedCards.Find(e => e.cardId == cardId);
        return entry != null ? entry.count : 0;
    }

    // 전체 소유 카드 리스트 반환
    public List<PlayerCardEntry> GetAllOwnedCards()
    {
        return new List<PlayerCardEntry>(collection.ownedCards);
    }

    // 카드ID로 카드 데이터 찾기 (Resources 폴더 내 모든 카드에서 탐색)
    public static BaseCardData FindCardDataById(string cardId)
    {
        var allCards = Resources.LoadAll<BaseCardData>("CardData");
        foreach (var card in allCards)
        {
            if (card.cardId == cardId)
                return card;
        }
        return null;
    }

    // (BaseCardData, count) 튜플 리스트 반환
    public List<(BaseCardData cardData, int count)> GetOwnedCardDataList()
    {
        var result = new List<(BaseCardData, int)>();
        foreach (var entry in GetAllOwnedCards())
        {
            var cardData = FindCardDataById(entry.cardId);
            if (cardData != null)
                result.Add((cardData, entry.count));
        }
        return result;
    }

    // cardId를 키로 하는 (BaseCardData, count) 딕셔너리 반환
    public Dictionary<string, (BaseCardData cardData, int count)> GetOwnedCardDataDict()
    {
        var dict = new Dictionary<string, (BaseCardData, int)>();
        foreach (var entry in GetAllOwnedCards())
        {
            var cardData = FindCardDataById(entry.cardId);
            if (cardData != null)
                dict[entry.cardId] = (cardData, entry.count);
        }
        return dict;
    }
    public void AddTestCraftPoint(int amount)
    {
        collection.craftPoint += amount;
        SaveCollection();
        DeckMakingUI.Instance?.RefreshCraftPointUI();
        Debug.Log($"테스트용 크래프트 포인트 {amount}만큼 지급됨. 현재: {collection.craftPoint}");
    }

#if UNITY_EDITOR
    [ContextMenu("테스트용 크래프트포인트 1000 지급")]
    public void AddTestCraftPoint1000()
    {
        AddTestCraftPoint(1000);
    }
#endif
}