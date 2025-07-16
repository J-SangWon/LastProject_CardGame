using System.Collections.Generic;
using UnityEngine;

public class CardManager_test : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform deckZone;
    public Transform handZone;

    private List<GameObject> deck = new List<GameObject>();

    void Start()
    {
        CreateDeck(10);
        DrawCards(5);
    }

    void CreateDeck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(cardPrefab, deckZone);
            card.transform.localScale = Vector3.one;

            // 핸드존 드래그용 연결
            card.GetComponent<CardClickHandler>().handZone = handZone;

            // 드로우 효과 연결
            var effect = card.GetComponent<MonsterEffectOnSummon>();
            if (effect != null)
            {
                effect.cardManager = this;
            }

            deck.Add(card);
        }
    }

    public void DrawCard()
    {
        DrawCards(1);
    }

    public void DrawCards(int count)
    {
        for (int i = 0; i < count && deck.Count > 0; i++)
        {
            GameObject card = deck[0];
            deck.RemoveAt(0);

            card.transform.SetParent(handZone, false);
            card.transform.localScale = Vector3.one;
        }
    }
}
