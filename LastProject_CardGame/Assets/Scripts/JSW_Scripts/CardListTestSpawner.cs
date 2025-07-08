using UnityEngine;

public class CardListTestSpawner : MonoBehaviour
{
    public Transform cardListContent; // CardListPanel의 Content
    public GameObject cardThumbnailPrefab; // CardThumbnail 프리팹

    void Start()
    {
        var allCards = CardManager.Instance.GetAllCards(); // 카드 데이터 리스트
        foreach (var card in allCards)
        {
            GameObject obj = Instantiate(cardThumbnailPrefab, cardListContent);
            var thumbnail = obj.GetComponent<CardThumbnail>();
            thumbnail.SetCard(card, 1); // 수량은 1로 테스트
        }
    }

}
