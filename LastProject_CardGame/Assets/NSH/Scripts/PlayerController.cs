using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("기본 설정")]
    public string playerName;
    public int maxLife = 40;
    private int currentLife;

    [Header("카드 관리")]
    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> hand = new List<GameObject>();
    public List<GameObject> graveyard = new List<GameObject>();

    [Header("핸드 위치 (UI 상자)")]
    public Transform handZone;
    public GameObject cardPrefab; // 카드 앞면 Prefab
    public GameObject cardBackPrefab; // 상대 손패용 뒷면 카드

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
            Destroy(card); // UI에서 제거
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
        Debug.Log($"{playerName}의 배틀 실행 (예시 구현)");
        // 실제 전투 로직은 카드에 따라 달라짐
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
