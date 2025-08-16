using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine.EventSystems;

/// <summary>
/// 엑스트라 덱 존을 관리하는 스크립트
/// </summary>
public class ExtraDeckZone : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 요소")]
    public TextMeshProUGUI extraDeckCountText; // 엑스트라 덱 카드 수 표시
    public GameObject cardPrefab;              // 카드 프리팹
    public InGameCardListPanel extraDeckListPanel; // 클릭 시 보여줄 패널

    [Header("시각적 설정")]
    [Tooltip("화면 실제 픽셀 기준의 아주 미세한 간격(px). Canvas Scaler를 보정하여 적용합니다.")]
    public float pixelSpacing = 0.15f; // 기본 0.15px 정도의 극미세 간격
    [Tooltip("X축 간격 배수(음수면 왼쪽 방향)")]
    public float xSpacingMultiplier = -1f;
    [Tooltip("Y축 간격 배수(양수면 위 방향)")]
    public float ySpacingMultiplier = 0.6f;
    [Tooltip("오프셋이 증가하는 최대 단계 수(총 이동량 제한)")]
    public int maxOffsetSteps = 4;
    public int maxVisibleCards = 20; // 겹쳐 보여줄 최대 카드 수

    // 실제 엑스트라 덱 데이터 (DeckCardEntry 리스트)
    public List<DeckCardEntry> extraDeck = new List<DeckCardEntry>();

    // 시각적으로 표시되는 카드 오브젝트들
    private List<GameObject> visualCardObjs = new List<GameObject>();

    void Start()
    {
        cardPrefab = PlayerCardManager.Instance.cardPrefab;
        UpdateVisual();
        
        // Image 컴포넌트가 없으면 추가
        if (GetComponent<Image>() == null)
        {
            var image = gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.1f); // 반투명 검정 (디버깅용)
        }
    }

    /// <summary>
    /// 클릭 이벤트 처리
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("ExtraDeckZone 클릭됨!");
        
        if (extraDeckListPanel != null)
        {
            extraDeckListPanel.Show(GetAllEntries(), "엑스트라 덱");
        }
        else
        {
            Debug.LogWarning("extraDeckListPanel이 할당되지 않았습니다!");
        }
    }

    /// <summary>
    /// 엑스트라 덱을 외부에서 초기화(덱 불러오기 등)
    /// </summary>
    public void InitializeExtraDeck(List<DeckCardEntry> entries)
    {
        extraDeck.Clear();
        if (entries != null)
        {
            // 깊은 복사 필요시 직접 복사
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
    /// 엑스트라 덱에서 카드 한 장 꺼내기(융합/싱크로 등)
    /// </summary>
    public BaseCardData RemoveFromExtraDeck()
    {
        for (int i = 0; i < extraDeck.Count; i++)
        {
            if (extraDeck[i].count > 0)
            {
                extraDeck[i].count--;
                BaseCardData card = extraDeck[i].card;
                // count가 0이 되면 리스트에서 제거
                if (extraDeck[i].count <= 0)
                    extraDeck.RemoveAt(i);
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
        // 이미 있는 카드면 count++
        var entry = extraDeck.Find(e => e.card == card);
        if (entry != null)
        {
            entry.count++;
        }
        else
        {
            extraDeck.Add(new DeckCardEntry { card = card, count = 1, cardId = card.cardId });
        }
        UpdateVisual();
    }

    /// <summary>
    /// 엑스트라 덱에서 특정 카드 한 장 제거(부활 등)
    /// </summary>
    public bool RemoveSpecificCard(BaseCardData card)
    {
        var entry = extraDeck.Find(e => e.card == card && e.count > 0);
        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0)
                extraDeck.Remove(entry);
            UpdateVisual();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 엑스트라 덱 카드 수 반환 (전체 합계)
    /// </summary>
    public int GetCount()
    {
        int sum = 0;
        foreach (var entry in extraDeck)
            sum += entry.count;
        return sum;
    }

    /// <summary>
    /// 엑스트라 덱의 모든 카드(DeckCardEntry) 반환(복사본)
    /// </summary>
    public List<DeckCardEntry> GetAllEntries()
    {
        // 깊은 복사 필요시 직접 복사
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

    /// <summary>
    /// 시각적 UI 갱신
    /// </summary>
    private void UpdateVisual()
    {
        // 카드 수 텍스트 갱신
        if (extraDeckCountText != null)
            extraDeckCountText.text = GetCount().ToString();

        // 기존 카드 오브젝트들 제거
        foreach (var cardObj in visualCardObjs)
        {
            if (cardObj != null)
                Destroy(cardObj);
        }
        visualCardObjs.Clear();

        // 카드 개수만큼 썸네일 생성
        if (extraDeck.Count > 0 && cardPrefab != null)
        {
            int cardIndex = 0;
            foreach (var entry in extraDeck)
            {
                for (int i = 0; i < entry.count && cardIndex < maxVisibleCards; i++)
                {
                    GameObject cardObj = Instantiate(cardPrefab, transform);
                    cardObj.GetComponent<CanvasGroup>().blocksRaycasts = false; // 클릭 방지
                    cardObj.transform.localScale = Vector3.one;

                    // 카드 위치: 극미세 대각 겹침 (Canvas 스케일 보정)
                    float scale = 1f;
                    var canvas = GetComponentInParent<Canvas>();
                    if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                    {
                        scale = Mathf.Max(0.01f, canvas.scaleFactor);
                    }
                    float eff = pixelSpacing / scale; // 실제 화면 px을 기준으로 한 보정 값
                    int step = Mathf.Min(cardIndex, Mathf.Max(0, maxOffsetSteps));
                    float xPos = step * eff * xSpacingMultiplier;
                    float yPos = step * eff * ySpacingMultiplier;
                    var rt = cardObj.GetComponent<RectTransform>();
                    if (rt != null)
                        rt.anchoredPosition = new Vector2(xPos, yPos);
                    else
                        cardObj.transform.localPosition = new Vector3(xPos, yPos, 0f);
                    // 최신 카드가 위에 오도록 정렬
                    cardObj.transform.SetAsLastSibling();
                    
                    // 카드 UI 설정
                    var cardUI = cardObj.GetComponent<CardUI>();
                    if (cardUI != null)
                    {
                        cardUI.SetCard(entry.card);
                        cardUI.EnableCardFlip = false; // 엑스트라 덱에서는 카드 플립 비활성화
                        cardUI.GetComponent<Image>().raycastTarget = false; // 클릭 방지
                        cardUI.FlipCard(false);
                        cardUI.SetFace(false); // 뒷면으로 설정
                    }

                    visualCardObjs.Add(cardObj);
                    cardIndex++;
                }
            }
        }
    }
}