using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// 묘지 존을 관리하는 스크립트
/// </summary>
public class GraveyardZone : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 요소")]
    public TextMeshProUGUI graveyardCountText;
    public InGameCardListPanel graveyardListPanel;
    [Header("카드 프리팹")]
    public GameObject cardPrefab;
    
    [Header("묘지 설정")]
    public int maxVisibleCards = 5;  // 시각적으로 표시할 최대 카드 수
    public float cardSpacing = 0.8f; // 카드 간격
    
    // DeckCardEntry를 사용하여 카드 개수별로 관리
    public List<DeckCardEntry> graveyard = new List<DeckCardEntry>();
    private List<GameObject> visualCardObjs = new List<GameObject>();

    void Start()
    {
        UpdateVisual();
        cardPrefab = PlayerCardManager.Instance.cardPrefab;
    }

    /// <summary>
    /// 카드를 묘지로 보내기
    /// </summary>
    public void SendToGraveyard(BaseCardData card)
    {
        if (card != null)
        {
            // 이미 같은 카드가 있는지 확인
            var existingEntry = graveyard.FirstOrDefault(entry => entry.card.cardName == card.cardName);
            
            if (existingEntry != null)
            {
                // 기존 카드 개수 증가
                existingEntry.count++;
            }
            else
            {
                // 새로운 카드 추가
                graveyard.Add(new DeckCardEntry { card = card, count = 1, cardId = card.cardId });
            }
            
            UpdateVisual();
            Debug.Log($"묘지로 보냄: {card.cardName} (총 {GetGraveyardCount()}장)");
        }
    }

    /// <summary>
    /// 묘지에서 특정 카드 제거 (부활 등)
    /// </summary>
    public bool RemoveFromGraveyard(BaseCardData card)
    {
        var entry = graveyard.FirstOrDefault(e => e.card.cardName == card.cardName);
        
        if (entry != null)
        {
            entry.count--;
            
            if (entry.count <= 0)
            {
                graveyard.Remove(entry);
            }
            
            UpdateVisual();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 묘지에서 특정 카드 개수만큼 제거
    /// </summary>
    public bool RemoveFromGraveyard(BaseCardData card, int count)
    {
        var entry = graveyard.FirstOrDefault(e => e.card.cardName == card.cardName);
        
        if (entry != null && entry.count >= count)
        {
            entry.count -= count;
            
            if (entry.count <= 0)
            {
                graveyard.Remove(entry);
            }
            
            UpdateVisual();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 묘지 카드 수 반환
    /// </summary>
    public int GetGraveyardCount()
    {
        return graveyard.Sum(entry => entry.count);
    }

    /// <summary>
    /// 묘지의 모든 카드 반환 (개수 포함)
    /// </summary>
    public List<DeckCardEntry> GetAllGraveyardCards()
    {
        return new List<DeckCardEntry>(graveyard);
    }

    /// <summary>
    /// 묘지가 비어있는지 확인
    /// </summary>
    public bool IsEmpty()
    {
        return graveyard.Count == 0;
    }

    /// <summary>
    /// 묘지 초기화
    /// </summary>
    public void ClearGraveyard()
    {
        graveyard.Clear();
        UpdateVisual();
    }

    /// <summary>
    /// 묘지에서 카드 확인 (UI 표시)
    /// </summary>
    public void ShowGraveyard()
    {
        graveyardListPanel.Show(GetAllGraveyardCards(), "묘지", true);
    }

    /// <summary>
    /// 시각적 UI 갱신
    /// </summary>
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
                cardObj.transform.localScale = Vector3.one * 0.8f; // 묘지는 작게
                
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

    /// <summary>
    /// 클릭 시 묘지 패널 표시
    /// </summary>
    public void OnGraveyardClicked()
    {
        if (graveyardListPanel != null)
        {
            ShowGraveyard();
        }
        else
        {
            Debug.LogWarning("graveyardListPanel이 할당되지 않았습니다!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnGraveyardClicked();
    }
}