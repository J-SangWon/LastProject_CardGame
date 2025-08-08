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
    public List<DeckCardEntry> enemyGraveyard = new List<DeckCardEntry>();
    public Transform graveyardTransform; // 묘지 위치 
    public Transform enemyGraveyardTransform; // 적 묘지 위치 
    private List<GameObject> visualCardObjs = new List<GameObject>();
    private List<GameObject> enemyVisualCardObjs = new List<GameObject>();

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
            Debug.Log($"[묘지] 카드 추가 시도: {card.cardName}, ownerType: {card.ownerType}");
            // 적 카드와 플레이어 카드 구분
            if (card.ownerType == OwnerType.Player)
            {
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
            else if (card.ownerType == OwnerType.Opponent)
            {
                var existingEntry = enemyGraveyard.FirstOrDefault(entry => entry.card.cardName == card.cardName);

                if (existingEntry != null)
                {
                    // 기존 카드 개수 증가
                    existingEntry.count++;
                }
                else
                {
                    // 새로운 카드 추가
                    enemyGraveyard.Add(new DeckCardEntry { card = card, count = 1, cardId = card.cardId });
                }

                UpdateVisual();
                Debug.Log($"적 묘지로 보냄: {card.cardName} (총 {GetEnemyGraveyardCount()}장)");
            }
            else
            {
                Debug.LogWarning("카드의 소유자가 지정되지 않았습니다!");
            }

            // 이미 같은 카드가 있는지 확인
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
    public int GetGraveyardCount() => graveyard.Sum(entry => entry.count);
    public int GetEnemyGraveyardCount() => enemyGraveyard.Sum(entry => entry.count);

    /// <summary>
    /// 묘지의 모든 카드 반환 (개수 포함)
    /// </summary>
    public List<DeckCardEntry> GetAllGraveyardCards()
    {
        return new List<DeckCardEntry>(graveyard);
    }
    public List<DeckCardEntry> GetAllEnemyGraveyardCards()
    {
        return new List<DeckCardEntry>(enemyGraveyard);
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
    public void ShowEnemyGraveyard()
    {
        graveyardListPanel.Show(GetAllEnemyGraveyardCards(), "적 묘지", true); // 패널이 하나라면 공유
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
                GameObject cardObj = Instantiate(cardPrefab, graveyardTransform);
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

        foreach (var cardObj in enemyVisualCardObjs)
            if (cardObj != null) Destroy(cardObj);
        enemyVisualCardObjs.Clear();

        if (enemyGraveyard.Count > 0 && cardPrefab != null)
        {
            int cardIndex = 0;
            for (int i = enemyGraveyard.Count - 1; i >= 0 && cardIndex < maxVisibleCards; i--)
            {
                var entry = enemyGraveyard[i];
                GameObject cardObj = Instantiate(cardPrefab, enemyGraveyardTransform);
                cardObj.transform.localScale = Vector3.one;
                float xPos = cardIndex * cardSpacing;
                // 적 카드 위치는 y축 등으로 분리해서 배치 가능 (예: new Vector3(xPos, -100, ...))
                cardObj.transform.localPosition = new Vector3(xPos, 0, -cardIndex * 0.01f);

                var cardUI = cardObj.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetCard(entry.card);
                    cardUI.EnableCardFlip = false;
                    cardUI.GetComponent<Image>().raycastTarget = false;
                }
                cardObj.GetComponent<CanvasGroup>().blocksRaycasts = false;
                enemyVisualCardObjs.Add(cardObj);
                cardIndex++;
            }
        }
    }

    /// <summary>
    /// 클릭 시 묘지 패널 표시
    /// </summary>

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} 클릭됨! 태그: {gameObject.tag}");
        string clickedTag = eventData.pointerPress?.tag ?? gameObject.tag;
        if (clickedTag == "PlayerGraveyard")
            ShowGraveyard();
        else if (clickedTag == "EnemyGraveyard")
            ShowEnemyGraveyard();
    }
}