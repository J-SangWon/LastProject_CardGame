using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�⺻ ����")]
    public string playerName;
    public int maxLife = 40;
    private int currentLife;

    [Header("ī�� ����")]
    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> hand = new List<GameObject>();
    public List<GameObject> graveyard = new List<GameObject>();

    [Header("�ڵ� ��ġ (UI ����)")]
    public Transform handZone;
    public GameObject cardPrefab; // ī�� �ո� Prefab
    public GameObject cardBackPrefab; // ��� ���п� �޸� ī��

    void Awake()
    {
        currentLife = maxLife;
    }

    public void Initialize()
    {
        currentLife = maxLife;
        hand.Clear();
        graveyard.Clear();
        ShuffleDeck();
    }

    public void DrawCard()
    {
        if (deck.Count == 0) return;

        GameObject card = deck[0];
        deck.RemoveAt(0);

        GameObject cardUI = Instantiate(cardPrefab, handZone);
        hand.Add(cardUI);
    }

    public void DrawMultipleCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0) break;
            DrawCard();
        }
    }

    public int GetHandCount()
    {
        return hand.Count;
    }

    public void DiscardFromHand(int amount)
    {
        for (int i = 0; i < amount && hand.Count > 0; i++)
        {
            GameObject card = hand[hand.Count - 1];
            hand.RemoveAt(hand.Count - 1);
            graveyard.Add(card);
            Destroy(card); // UI���� ����
        }
    }

    public void TakeDamage(int amount)
    {
        currentLife -= amount;
        if (currentLife <= 0)
        {
            InGameManager manager = FindFirstObjectByType<InGameManager>();
            if (manager != null)
                manager.GameOver(this);
        }
    }

    public void Battle()
    {
        Debug.Log($"{playerName}�� ��Ʋ ���� (���� ����)");
        // ���� ���� ������ ī�忡 ���� �޶���
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            GameObject temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
}
