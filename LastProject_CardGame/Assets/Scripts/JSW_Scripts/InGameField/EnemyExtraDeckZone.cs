using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 적 엑스트라 덱 존을 관리하는 스크립트
/// 플레이어의 엑스트라 덱 존(ExtraDeckZone.cs)을 참고하여 동일한 API 제공
/// </summary>
public class EnemyExtraDeckZone : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 요소")]
    public TextMeshProUGUI extraDeckCountText; // 엑스트라 덱 카드 수 표시
    public GameObject cardPrefab;              // 카드 프리팹
    public InGameCardListPanel extraDeckListPanel; // 클릭 시 보여줄 패널(선택적)

    [Header("시각적 설정")]
    [Tooltip("화면 실제 픽셀 기준의 아주 미세한 간격(px). Canvas Scaler를 보정하여 적용합니다.")]
    public float pixelSpacing = 0.15f; // 기본 0.15px 정도의 극미세 간격
    [Tooltip("X축 간격 배수(양수면 오른쪽 방향)")]
    public float xSpacingMultiplier = 1f;
    [Tooltip("Y축 간격 배수(음수면 아래 방향)")]
    public float ySpacingMultiplier = -0.6f;
    public int maxVisibleCards = 20; // 겹쳐 보여줄 최대 카드 수

    // 실제 엑스트라 덱 데이터 (DeckCardEntry 리스트)
    public List<DeckCardEntry> extraDeck = new List<DeckCardEntry>();

    // 시각적으로 표시되는 카드 오브젝트들
    private readonly List<GameObject> visualCardObjs = new List<GameObject>();

    void Start()
    {
        cardPrefab = OpponentCardManager.Instance.cardPrefab;
        UpdateVisual();

        // Image 컴포넌트가 없으면 추가(디버그용 시각 박스)
        if (GetComponent<Image>() == null)
        {
            var image = gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (extraDeckListPanel != null)
        {
            extraDeckListPanel.Show(GetAllEntries(), "적 엑스트라 덱");
        }
    }

    /// <summary>
    /// 적 엑스트라 덱을 외부에서 초기화
    /// </summary>
    public void InitializeExtraDeck(List<DeckCardEntry> entries)
    {
        extraDeck.Clear();
        if (entries != null)
        {
            foreach (var entry in entries)
            {
                var newEntry = new DeckCardEntry
                {
                    card = entry.card,
                    count = entry.count,
                    cardId = entry.cardId
                };
                extraDeck.Add(newEntry);
            }
        }
        UpdateVisual();
    }

    /// <summary>
    /// 엑스트라 덱에서 카드 한 장 꺼내기
    /// </summary>
    public BaseCardData RemoveFromExtraDeck()
    {
        for (int i = 0; i < extraDeck.Count; i++)
        {
            if (extraDeck[i].count > 0)
            {
                extraDeck[i].count--;
                BaseCardData card = extraDeck[i].card;
                if (extraDeck[i].count <= 0) extraDeck.RemoveAt(i);
                UpdateVisual();
                return card;
            }
        }
        return null;
    }

    /// <summary>
    /// 엑스트라 덱에 카드 추가
    /// </summary>
    public void AddToExtraDeck(BaseCardData card)
    {
        if (card == null) return;
        var entry = extraDeck.Find(e => e.card == card);
        if (entry != null) entry.count++;
        else extraDeck.Add(new DeckCardEntry { card = card, count = 1, cardId = card.cardId });
        UpdateVisual();
    }

    /// <summary>
    /// 특정 카드 제거(1장)
    /// </summary>
    public bool RemoveSpecificCard(BaseCardData card)
    {
        var entry = extraDeck.Find(e => e.card == card && e.count > 0);
        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0) extraDeck.Remove(entry);
            UpdateVisual();
            return true;
        }
        return false;
    }

    public int GetCount()
    {
        int sum = 0;
        foreach (var entry in extraDeck) sum += entry.count;
        return sum;
    }

    public List<DeckCardEntry> GetAllEntries()
    {
        var list = new List<DeckCardEntry>();
        foreach (var entry in extraDeck)
        {
            list.Add(new DeckCardEntry
            {
                card = entry.card,
                count = entry.count,
                cardId = entry.cardId
            });
        }
        return list;
    }

    private void UpdateVisual()
    {
        if (extraDeckCountText != null)
            extraDeckCountText.text = GetCount().ToString();

        foreach (var cardObj in visualCardObjs)
        {
            if (cardObj != null)
                Destroy(cardObj);
        }
        visualCardObjs.Clear();

        if (extraDeck.Count > 0 && cardPrefab != null)
        {
            int cardIndex = 0;
            foreach (var entry in extraDeck)
            {
                for (int i = 0; i < entry.count && cardIndex < maxVisibleCards; i++)
                {
                    GameObject cardObj = Instantiate(cardPrefab, transform);
                    cardObj.transform.localScale = Vector3.one;
                    cardObj.GetComponent<CanvasGroup>().blocksRaycasts = false;

                    // 카드 위치: 극미세 대각 겹침 (Canvas 스케일 보정)
                    float scale = 1f;
                    var canvas = GetComponentInParent<Canvas>();
                    if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                    {
                        scale = Mathf.Max(0.01f, canvas.scaleFactor);
                    }
                    float eff = pixelSpacing / scale; // 실제 화면 px 기준 보정 값
                    float xPos = cardIndex * eff * xSpacingMultiplier;
                    float yPos = cardIndex * eff * ySpacingMultiplier;
                    var rt = cardObj.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.anchoredPosition = new Vector2(xPos, yPos);
                    else
                        cardObj.transform.localPosition = new Vector3(xPos, yPos, 0f);
                    // 최신 카드가 위로 오도록 정렬
                    cardObj.transform.SetAsLastSibling();

                    var cardUI = cardObj.GetComponent<CardUI>();
                    if (cardUI != null)
                    {
                        cardUI.SetCard(entry.card);
                        cardUI.EnableCardFlip = false;
                        cardUI.GetComponent<Image>().raycastTarget = false;
                        // 적 엑스트라 덱 표시: 뒷면
                        cardUI.FlipCard(false);
                        cardUI.SetFace(false);
                    }

                    visualCardObjs.Add(cardObj);
                    cardIndex++;
                }
            }
        }
    }
}


