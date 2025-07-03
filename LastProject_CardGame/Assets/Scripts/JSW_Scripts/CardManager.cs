using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    // 모든 카드 데이터 저장
    public List<BaseCardData> allCards = new List<BaseCardData>();

    // 카드 이름으로 빠르게 찾기 위한 Dictionary
    public Dictionary<string, BaseCardData> cardDict = new Dictionary<string, BaseCardData>();

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


}
