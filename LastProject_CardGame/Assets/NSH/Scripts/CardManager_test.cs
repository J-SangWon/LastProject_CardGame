using System.Collections.Generic;
using UnityEngine;

public class CardManager_test : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform deckZone;
    public Transform handZone;

    private List<GameObject> deck = new List<GameObject>();
    public DeckData currentDeckData;

    void Start()
    {
        LoadDeckFromTransfer();
    }

    void LoadDeckFromTransfer()
    {
        currentDeckData = DeckTransferManager.Instance?.GetDeck();

        if (currentDeckData == null)
        {
            Debug.LogWarning("DeckTransferManager�κ��� �� �����͸� �������� ���߽��ϴ�.");
            return;
        }

        // Resources���� ��� ī�� �ҷ����� �� cardId ���� (������ ����)
        BaseCardData[] allCards = Resources.LoadAll<BaseCardData>("CardData");
        foreach (var entry in currentDeckData.mainDeck)
        {
            if (entry.card == null)
            {
                foreach (var c in allCards)
                {
                    if (c.cardId == entry.cardId)
                    {
                        entry.card = c;
                        break;
                    }
                }
                if (entry.card == null)
                    Debug.LogWarning($"cardId {entry.cardId}�� ���� ī�� �����͸� ã�� �� �����ϴ�.");
            }
        }

        ClearDeck();

        int zIndex = 0;

        foreach (var cardEntry in currentDeckData.mainDeck)
        {
            for (int i = 0; i < cardEntry.count; i++)
            {
                GameObject card = Instantiate(cardPrefab, deckZone);
                card.transform.localScale = Vector3.one;

                var cardUI = card.GetComponent<CardUI_N>();
                if (cardUI != null)
                {
                    cardUI.SetCard(cardEntry.card);
                }
                else
                {
                    Debug.LogWarning("CardUI_N ������Ʈ�� ã�� �� �����ϴ�.");
                }

                card.transform.localPosition = new Vector3(0, 0, -zIndex * 0.01f);
                zIndex++;

                var effect = card.GetComponent<MonsterEffectOnSummon>();
                if (effect != null)
                {
                    effect.cardManager = this;
                }

                deck.Add(card);
            }
        }

        DrawCards(5);
    }

    private void ClearDeck()
    {
        foreach (var card in deck)
        {
            Destroy(card);
        }
        deck.Clear();
    }

    // ī�� �� ���� ��ο�
    public void DrawCard()
    {
        DrawCards(1);
    }

    // ī�� ���� ���� ��ο�
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

    private void UpdateHandLayout()
    {
        float spacing = 150f;
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
