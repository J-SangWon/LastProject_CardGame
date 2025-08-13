using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 카드 매니저: 덱 로딩, 드로우, 카드 배치 및 일부 카드 효과 처리.
/// </summary>
public class PlayerCardManager : MonoBehaviour
{
    public static PlayerCardManager Instance;

    [Header("플레이어 카드")]
    public GameObject cardPrefab;
    public Transform playerDeckZone;
    public Transform enemyDeckZone;
    public Transform playerHandZone;
    public Transform enemyHandZone;
    public Transform playerMonsterZone;
    public Transform enemyMonsterZone;

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

    #region 플레이어 덱 로딩 및 드로우

    public List<GameObject> GetDeck()
    {
        return deck;
    }

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

        //  덱 카드 단위로 리스트화
        List<BaseCardData> flatDeck = new List<BaseCardData>();
        foreach (var entry in currentDeckData.mainDeck)
        {
            for (int i = 0; i < entry.count; i++)
            {
                flatDeck.Add(entry.card);
            }
        }

        // 셔플
        ShuffleDeck(flatDeck);

        //  셔플된 순서로 덱에 카드 생성
        int zIndex = 0;
        foreach (var cardData in flatDeck)
        {
            GameObject card = CreateCard(cardData, cardPrefab, playerDeckZone, Quaternion.identity);
            card.transform.localPosition = new Vector3(-zIndex * 0.5f, zIndex * 0.5f, 0);
            card.GetComponent<CardUI>().EnableCardFlip = false;
            card.GetComponent<CardUI>().FlipCard(false); // 초기에는 뒷면으로 설정
            card.AddComponent<FildMonster>();
            zIndex++;

            deck.Add(card);
        }

        //  드로우
        DrawCards(5);
    }

    private void ShuffleDeck(List<BaseCardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
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

            card.GetComponent<CardUI>().FlipCard(true);
            card.transform.SetParent(playerHandZone, false);
            card.transform.localScale = Vector3.one;
        }

        UpdateHandLayout();
    }

    public void DrawCard() => DrawCards(1);

    public void SearchCard(System.Func<GameObject, bool> condition, int count = 1)
    {
        Debug.Log($"[PlayerCardManager] SearchCard 호출: count={count}, deck.Count={deck.Count}");
        int movedCount = 0;
        for (int i = 0; i < deck.Count && movedCount < count; i++)
        {
            if (deck[i] != null && condition(deck[i]))
            {
                GameObject card = deck[i];
                deck.RemoveAt(i);
                i--; // 리스트에서 제거했으니 인덱스 보정

                card.transform.SetParent(playerHandZone, false);
                card.transform.localScale = Vector3.one;

                movedCount++;
            }
        }
        UpdateHandLayout();
    }

    public void UpdateHandLayout()
    {
        int cardCount = playerHandZone.childCount;
        if (cardCount == 0) return;

        RectTransform handRect = playerHandZone.GetComponent<RectTransform>();
        float maxWidth = handRect.rect.width;

        float cardWidth = 150f;    // 카드 너비 (실제 카드 크기로 맞춰야 함)
        float minSpacing = 30f;    // 최소 간격

        // 카드 간격 기본값
        float spacing = cardWidth;

        // 전체 카드 너비 = 카드 한 장 너비 + 간격 * (개수 - 1)
        float totalWidth = cardWidth + spacing * (cardCount - 1);

        if (totalWidth > maxWidth)
        {
            spacing = (maxWidth - cardWidth) / (cardCount - 1);
            spacing = Mathf.Max(spacing, minSpacing);
            totalWidth = cardWidth + spacing * (cardCount - 1);
        }

        // 시작 위치: 핸드존 왼쪽 끝 기준 (pivot이 (0,0.5)라면 anchoredPosition.x=0이 왼쪽 끝)
        float startX = -(totalWidth / 2) + (cardWidth / 2); // 왼쪽 끝부터 시작하도록 조정

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform rt = playerHandZone.GetChild(i).GetComponent<RectTransform>();
            if (rt != null)
            {
                // 카드 Pivot도 (0,0.5)여서 anchoredPosition은 카드 왼쪽 위치 기준임
                rt.anchoredPosition = new Vector2(startX + spacing * i, 0);
            }
        }
    }


    #endregion

    #region 공통 카드 생성 메서드

    /// <summary>
    /// 카드 생성 및 UI, 효과 초기화
    /// </summary>
    public GameObject CreateCard(BaseCardData data, GameObject prefab, Transform parent, Quaternion rotation)
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

        // 대상 지정 및 소환 효과는 플레이어 카드만
        if (rotation == Quaternion.identity)
        {
            if (card.GetComponent<TargetableCard>() == null)
                card.AddComponent<TargetableCard>();

            var effect = card.GetComponent<MonsterEffectOnSummon>();
            if (effect == null) effect = card.AddComponent<MonsterEffectOnSummon>();

            effect.cardData = data;
            effect.cardManager = this;
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

        // 소환 시점 효과 트리거
        var fm = card.GetComponent<FildMonster>();
        if (fm != null)
        {
            fm.OnPlacedOnField();
        }

        // 위치 정렬 예시 (필요시 커스터마이즈 가능)
        UpdateFieldLayout();
    }

    /// <summary>
    /// 카드 데이터로부터 새 오브젝트를 생성하여 즉시 필드에 소환
    /// </summary>
    public GameObject SummonFromData(BaseCardData data)
    {
        if (data == null) return null;
        GameObject card = CreateCard(data, cardPrefab, fieldZone, Quaternion.identity);
        PlayCardToField(card);
        return card;
    }

    // Monster Slot Summon Utilities
    private Transform FindFirstFreeMonsterSlot(OwnerType ownerType)
    {
        string tag = ownerType == OwnerType.Player ? "PlayerZone" : "EnemyZone";
        var slots = GameObject.FindGameObjectsWithTag(tag);
        foreach (var go in slots)
        {
            var slot = go.GetComponent<MonsterSlotDrop>();
            if (slot != null && !slot.isOccupied)
            {
                return slot.transform;
            }
        }
        return null;
    }

    public bool PlaceExistingCardToMonsterSlot(GameObject card, OwnerType ownerType)
    {
        if (card == null) return false;
        Transform slotTr = FindFirstFreeMonsterSlot(ownerType);
        if (slotTr == null)
        {
            Debug.LogWarning("[PlayerCardManager] No free monster slot for " + ownerType);
            return false;
        }

        card.transform.SetParent(slotTr, false);
        card.transform.localPosition = Vector3.zero;

        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.isOnField = true;
            cardUI.ownerType = ownerType;
        }

        var drag = card.GetComponent<CardDragHandler>();
        if (drag != null)
        {
            drag.isSummoned = true;
            drag.droppedOnSlot = true;
        }

        var slot = slotTr.GetComponent<MonsterSlotDrop>();
        if (slot != null)
        {
            slot.isOccupied = true;
        }

        var fm = card.GetComponent<FildMonster>();
        if (fm != null)
        {
            fm.OnPlacedOnField();
        }

        return true;
    }

    public GameObject SummonFromDataToMonsterZone(BaseCardData data, OwnerType ownerType)
    {
        if (data == null) return null;
        Transform slotTr = FindFirstFreeMonsterSlot(ownerType);
        if (slotTr == null)
        {
            Debug.LogWarning("[PlayerCardManager] No free monster slot for " + ownerType);
            return null;
        }

        GameObject card = Instantiate(cardPrefab, slotTr);
        card.transform.localScale = Vector3.one;
        card.transform.localPosition = Vector3.zero;
        if (ownerType == OwnerType.Opponent)
        {
            card.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        }

        var cardUI = card.GetComponent<CardUI>();
        if (cardUI != null)
        {
            cardUI.SetCard(data);
            cardUI.SetFace(ownerType == OwnerType.Player);
            cardUI.EnableCardFlip = false;
            cardUI.isOnField = true;
            cardUI.ownerType = ownerType;
        }

        var dragHandler = card.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.enabled = ownerType == OwnerType.Player;
            dragHandler.isSummoned = true;
            dragHandler.droppedOnSlot = true;
        }

        var slot = slotTr.GetComponent<MonsterSlotDrop>();
        if (slot != null)
        {
            slot.isOccupied = true;
        }

        if (card.GetComponent<Collider2D>() == null)
        {
            var collider = card.AddComponent<BoxCollider2D>();
            var rect = card.GetComponent<RectTransform>();
            if (rect != null)
            {
                collider.offset = rect.rect.center;
                collider.size = rect.rect.size;
            }
        }

        var fm = card.GetComponent<FildMonster>();
        if (fm == null && data is MonsterCardData)
        {
            fm = card.AddComponent<FildMonster>();
        }
        if (fm != null)
        {
            fm.OnPlacedOnField();
        }

        return card;
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

    #region 카드 효과 처리

    public void ResolveCard(PlayerController_N player, System.Action onComplete)
    {
        Debug.Log("카드 효과 해결 중...");
        DrawCard(); // 예시
        onComplete?.Invoke();
    }

    #endregion
}
