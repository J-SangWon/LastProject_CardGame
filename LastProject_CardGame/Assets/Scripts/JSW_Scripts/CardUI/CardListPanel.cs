using System.Collections.Generic;
using UnityEngine;

public class CardListPanel : MonoBehaviour
{
    public Transform cardListContent; // CardListPanel의 Content에 연결
    public GameObject cardThumbnailPrefab; // CardThumbnail 프리팹

    void OnEnable()
    {
        if (DeckBuilder.Instance != null)
            DeckBuilder.Instance.OnDeckChanged += RefreshList;
    }
    void OnDisable()
    {
        if (DeckBuilder.Instance != null)
            DeckBuilder.Instance.OnDeckChanged -= RefreshList;
    }
    void RefreshList()
    {
        ShowAllCards(CardManager.Instance.allCards);
    }

    public void ShowAllCards(List<BaseCardData> allCards)
    {
        foreach (Transform child in cardListContent)
            Destroy(child.gameObject);

        foreach (var card in allCards)
        {
            GameObject obj = Instantiate(cardThumbnailPrefab, cardListContent);
            CardThumbnail thumbnail = obj.GetComponent<CardThumbnail>();
            int count = CardManager.Instance.ownedCardCounts.ContainsKey(card) ? CardManager.Instance.ownedCardCounts[card] : 0;
            thumbnail.SetCard(card, count);

            if (count == 0)
                thumbnail.SetUnavailableVisual();
        }
    }
}
