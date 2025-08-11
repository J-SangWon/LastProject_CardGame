using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 카드 매니저: 덱 로딩, 드로우, 카드 배치.
/// </summary>
public class OpponentCardManager : MonoBehaviour
{
    public static OpponentCardManager Instance;

    [Header("적 카드")]
    public GameObject cardPrefab;
    public Transform deckZone;
    public Transform handZone;

    private List<GameObject> deck = new List<GameObject>();
    public DeckData currentDeckData;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        LoadDeckFromTransfer();
    }

    #region 적 덱 로딩 및 드로우

    void LoadDeckFromTransfer()
    {
        currentDeckData = DeckTransferManager.Instance?.GetDeck();

        if (currentDeckData == null)
        {
            Debug.LogWarning("DeckTransferManager로부터 덱 데이터를 가져오지 못했습니다.");
            return;
        }

        // ScriptableObject 재연결
        BaseCardData[] allCards = Resources.LoadAll<BaseCardData>("CardData");

        foreach (var entry in currentDeckData.mainDeck)
        {
            if (entry.card == null)
            {
                entry.card = System.Array.Find(allCards, c => c.cardId == entry.cardId);
            }
        }

        ClearDeck();

        int zIndex = 0;
        foreach (var entry in currentDeckData.mainDeck)
        {
            for (int i = 0; i < entry.count; i++)
            {
                GameObject card = CreateCard(entry.card, cardPrefab, deckZone, Quaternion.identity);
                card.transform.localPosition = new Vector3(0, 0, -zIndex * 0.01f);
                card.GetComponent<CardUI>().EnableCardFlip = false;
                card.GetComponent<CardUI>().ownerType = OwnerType.Opponent; // 적 카드로 설정
                card.AddComponent<FildMonster>();
                zIndex++;

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

    public void DrawCard() => DrawCards(1);

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

    #endregion

    #region 서치

    public void SearchCard(System.Func<GameObject, bool> condition, int count = 1)
    {
        int movedCount = 0;
        for (int i = 0; i < deck.Count && movedCount < count; i++)
        {
            if (deck[i] != null && condition(deck[i]))
            {
                GameObject card = deck[i];
                deck.RemoveAt(i);
                i--;

                card.transform.SetParent(handZone, false);
                card.transform.localScale = Vector3.one;

                var ui = card.GetComponent<CardUI>();
                if (ui != null)
                {
                    ui.ownerType = OwnerType.Opponent;
                }

                movedCount++;
            }
        }
        UpdateHandLayout();
    }

    #endregion

    #region 공통 카드 생성 메서드

    /// <summary>
    /// 카드 생성 및 UI, 초기화
    /// </summary>
    private GameObject CreateCard(BaseCardData data, GameObject prefab, Transform parent, Quaternion rotation)
    {
        GameObject card = Instantiate(prefab, parent);
        card.transform.localScale = Vector3.one;
        card.transform.localRotation = rotation;

        // UI 세팅
        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.SetCard(data);
            cardUI.SetFace(true);
            cardUI.EnableCardFlip = rotation == Quaternion.identity; // 플레이어 카드만 클릭 가능
        }

        // 드래그
        var dragHandler = card.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.enabled = rotation == Quaternion.identity;
        }

        // Collider 자동 추가 (UI 카드에 Raycast 되도록 BoxCollider2D 사용 권장)
        if (card.GetComponent<Collider2D>() == null)
        {
            var collider = card.AddComponent<BoxCollider2D>();

            // 크기 자동 설정 (필요에 따라 조절 가능)
            var rect = card.GetComponent<RectTransform>();
            if (rect != null)
            {
                collider.offset = rect.rect.center;
                collider.size = rect.rect.size;
            }
        }

        return card;
    }

    /// <summary>
    /// 몬스터존 위치 계산 (5슬롯 기준 중앙 정렬)
    /// </summary>
    private Vector3 GetSlotPosition(int index, int total, float spacing)
    {
        float startX = -((total - 1) * spacing) / 2f;
        return new Vector3(startX + index * spacing, 0, 0);
    }

    public Transform fieldZone; // 필드 영역 참조 필요

    public void PlayCardToField(GameObject card)
    {
        card.transform.SetParent(fieldZone, false);
        card.transform.localScale = Vector3.one;

        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.isOnField = true;
        }

        // 위치 정렬 예시 (필요시 커스터마이즈 가능)
        UpdateFieldLayout();
    }

    private void UpdateFieldLayout()
    {
        float spacing = 160f;
        for (int i = 0; i < fieldZone.childCount; i++)
        {
            RectTransform rt = fieldZone.GetChild(i).GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * spacing, 0);
            }
        }
    }

    #endregion
}
