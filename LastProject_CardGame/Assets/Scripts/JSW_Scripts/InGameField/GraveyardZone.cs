using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 묘지 존을 관리하는 스크립트
/// </summary>
public class GraveyardZone : MonoBehaviour
{
    [Header("UI 요소")]
    public TextMeshProUGUI graveyardCountText;
    public GameObject graveyardCardObject;
    
    [Header("카드 프리팹")]
    public GameObject cardPrefab;
    
    [Header("묘지 설정")]
    public int maxVisualCards = 5;  // 시각적으로 표시할 최대 카드 수
    
    private List<BaseCardData> graveyard = new List<BaseCardData>();
    private List<GameObject> graveyardVisualCards = new List<GameObject>();

    void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// 카드를 묘지로 보내기
    /// </summary>
    public void SendToGraveyard(BaseCardData card)
    {
        if (card != null)
        {
            graveyard.Add(card);
            CreateGraveyardVisual(card);
            UpdateUI();
            
            Debug.Log($"묘지로 보냄: {card.cardName}");
        }
    }

    /// <summary>
    /// 묘지 시각적 표현 생성
    /// </summary>
    void CreateGraveyardVisual(BaseCardData card)
    {
        if (cardPrefab != null)
        {
            GameObject cardObj = Instantiate(cardPrefab, transform);
            cardObj.transform.localScale = Vector3.one * 0.8f; // 묘지는 작게
            cardObj.transform.localPosition = Vector3.zero;
            
            var cardUI = cardObj.GetComponent<CardUI_N>();
            if (cardUI != null)
            {
                cardUI.SetCard(card);
            }
            
            graveyardVisualCards.Add(cardObj);
            
            // 묘지 카드가 많아지면 정리
            if (graveyardVisualCards.Count > maxVisualCards)
            {
                // 오래된 카드들 제거 (시각적 표현만)
                for (int i = 0; i < graveyardVisualCards.Count - maxVisualCards; i++)
                {
                    Destroy(graveyardVisualCards[0]);
                    graveyardVisualCards.RemoveAt(0);
                }
            }
        }
    }

    /// <summary>
    /// 묘지에서 카드 확인 (UI 표시)
    /// </summary>
    public void ShowGraveyard()
    {
        Debug.Log($"묘지 카드 수: {graveyard.Count}");
        foreach (var card in graveyard)
        {
            Debug.Log($"- {card.cardName}");
        }
    }

    /// <summary>
    /// 묘지에서 특정 카드 제거 (부활 등)
    /// </summary>
    public bool RemoveFromGraveyard(BaseCardData card)
    {
        if (graveyard.Contains(card))
        {
            graveyard.Remove(card);
            UpdateGraveyardVisual();
            UpdateUI();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 묘지 시각적 업데이트
    /// </summary>
    void UpdateGraveyardVisual()
    {
        // 모든 시각적 카드 제거
        foreach (var card in graveyardVisualCards)
        {
            Destroy(card);
        }
        graveyardVisualCards.Clear();

        // 최근 카드들로 다시 생성
        int startIndex = Mathf.Max(0, graveyard.Count - maxVisualCards);
        for (int i = startIndex; i < graveyard.Count; i++)
        {
            CreateGraveyardVisual(graveyard[i]);
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    void UpdateUI()
    {
        if (graveyardCountText != null)
            graveyardCountText.text = graveyard.Count.ToString();
    }

    /// <summary>
    /// 묘지 카드 수 반환
    /// </summary>
    public int GetGraveyardCount()
    {
        return graveyard.Count;
    }

    /// <summary>
    /// 묘지의 모든 카드 반환
    /// </summary>
    public List<BaseCardData> GetAllGraveyardCards()
    {
        return new List<BaseCardData>(graveyard);
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
        UpdateGraveyardVisual();
        UpdateUI();
    }
}