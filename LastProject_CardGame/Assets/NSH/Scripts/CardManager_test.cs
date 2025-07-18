using System.Collections.Generic;
using UnityEngine;

public class CardManager_test : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform deckZone;
    public Transform handZone;

    private List<GameObject> deck = new List<GameObject>();

    public DeckData currentDeckData;

    public DeckSaveManager deckSaveManager;

    void Start()
    {
        string lastDeckName = deckSaveManager.GetLastSelectedDeckName();

        if (!string.IsNullOrEmpty(lastDeckName))
        {
            LoadDeckFromSave(lastDeckName);
        }
        else
        {
            CreateDeck(10);
            DrawCards(5);
        }
    }

    public void LoadDeckFromSave(string deckFileName)
    {
        currentDeckData = deckSaveManager.LoadDeck(deckFileName);

        if (currentDeckData == null)
        {
            Debug.LogWarning("�� �����͸� �ҷ����� ���߽��ϴ�: " + deckFileName);
            return;
        }

        ClearDeck();

        int zIndex = 0;

        foreach (var cardEntry in currentDeckData.mainDeck)
        {
            for (int i = 0; i < cardEntry.count; i++)
            {
                GameObject card = Instantiate(cardPrefab, deckZone);
                card.transform.localScale = Vector3.one;

                var cardComponent = card.GetComponent<CardComponent>();
                if (cardComponent != null)
                {
                    cardComponent.SetCardData(cardEntry.card);
                }

                card.transform.localPosition = new Vector3(0, 0, -zIndex * 0.01f);
                zIndex++;

                var clickHandler = card.GetComponent<CardClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.handZone = handZone;
                }

                // �� ���⼭ cardManager �Ҵ� �߰� ��
                var effect = card.GetComponent<MonsterEffectOnSummon>();
                if (effect != null)
                {
                    effect.cardManager = this;
                }

                deck.Add(card);
            }
        }
    }

    private void ClearDeck()
    {
        foreach (var card in deck)
        {
            Destroy(card);
        }
        deck.Clear();
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

        UpdateHandLayout();
    }

    void CreateDeck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(cardPrefab, deckZone);
            card.transform.localScale = Vector3.one;

            var clickHandler = card.GetComponent<CardClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.handZone = handZone;
            }

            var effect = card.GetComponent<MonsterEffectOnSummon>();
            if (effect != null)
            {
                effect.cardManager = this;
            }

            deck.Add(card);
        }
    }

    // �ڵ��� ī�� ��ġ �ڵ� ���� (��: ���� ��ġ)
    private void UpdateHandLayout()
    {
        float spacing = 150f; // ī�� ����(px)
        for (int i = 0; i < handZone.childCount; i++)
        {
            RectTransform rt = handZone.GetChild(i).GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * spacing, 0);
            }
        }
    }
}
