using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyGraveyardZone : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI graveyardCountText;
    public InGameCardListPanel graveyardListPanel;
    public GameObject cardPrefab;
    public int maxVisibleCards = 5;
    public float cardSpacing = 0.8f;

    public List<DeckCardEntry> graveyard = new List<DeckCardEntry>();
    private List<GameObject> visualCardObjs = new List<GameObject>();

    void Start()
    {
        UpdateVisual();
        cardPrefab = PlayerCardManager.Instance.cardPrefab;
    }

    public void SendToGraveyard(BaseCardData card)
    {
        if (card != null)
        {
            Debug.Log($"적 묘지로 보내기: {card.cardName}");
            var existingEntry = graveyard.FirstOrDefault(entry => entry.card.cardName == card.cardName);
            if (existingEntry != null) existingEntry.count++;
            else graveyard.Add(new DeckCardEntry { card = card, count = 1, cardId = card.cardId });
            UpdateVisual();
            Debug.Log($"적 묘지로 보냄: {card.cardName} (총 {GetGraveyardCount()}장)");
        }
    }

    public bool RemoveFromGraveyard(BaseCardData card)
    {
        var entry = graveyard.FirstOrDefault(e => e.card.cardName == card.cardName);
        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0) graveyard.Remove(entry);
            UpdateVisual();
            return true;
        }
        return false;
    }

    public int GetGraveyardCount() => graveyard.Sum(entry => entry.count);
    public bool IsEmpty() => graveyard.Count == 0;

    public void ClearGraveyard()
    {
        graveyard.Clear();
        UpdateVisual();
    }
    public List<DeckCardEntry> GetAllGraveyardCards()
    {
        return new List<DeckCardEntry>(graveyard);
    }

    public void ShowGraveyard()
    {
        graveyardListPanel.Show(GetAllGraveyardCards(), "적 묘지", false);
    }
    private void UpdateVisual()
    {
        // 카드 수 텍스트 갱신
        if (graveyardCountText != null)
            graveyardCountText.text = GetGraveyardCount().ToString();

        // 기존 카드 오브젝트들 제거
        foreach (var cardObj in visualCardObjs)
        {
            if (cardObj != null)
                Destroy(cardObj);
        }
        visualCardObjs.Clear();

        // 최근 카드들만 시각적으로 표시 (묘지는 최근 순서로)
        if (graveyard.Count > 0 && cardPrefab != null)
        {
            int cardIndex = 0;

            // 최근 카드부터 표시 (역순)
            for (int i = graveyard.Count - 1; i >= 0 && cardIndex < maxVisibleCards; i--)
            {
                var entry = graveyard[i];

                // 각 카드 타입별로 1장씩만 표시 (개수는 텍스트로)
                GameObject cardObj = Instantiate(cardPrefab, transform);
                cardObj.transform.localScale = Vector3.one;

                // 카드 위치 설정 (가로로 나열)
                float xPos = cardIndex * cardSpacing;
                cardObj.transform.localPosition = new Vector3(xPos, 0, -cardIndex * 0.01f);

                // 카드 UI 설정
                var cardUI = cardObj.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetCard(entry.card);
                    cardUI.EnableCardFlip = false; //카드 플립 비활성화
                    cardUI.GetComponent<Image>().raycastTarget = false; // 클릭 방지

                }
                cardObj.GetComponent<CanvasGroup>().blocksRaycasts = false; // 클릭 방지


                visualCardObjs.Add(cardObj);
                cardIndex++;
            }
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (graveyardListPanel == null)
        {
            Debug.LogError("Graveyard List Panel is not assigned!");
            return;
        }
        ShowGraveyard();
    }

}