using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		EnsureFieldZone();
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

		// 1) 덱 평탄화 후 셔플
		List<BaseCardData> flatDeck = new List<BaseCardData>();
		foreach (var entry in currentDeckData.mainDeck)
		{
			for (int i = 0; i < entry.count; i++)
			{
				flatDeck.Add(entry.card);
			}
		}
		ShuffleDeckData(flatDeck);

		// 2) 셔플된 순서로 카드 생성
		int zIndex = 0;
		foreach (var data in flatDeck)
		{
			GameObject card = CreateCard(data, cardPrefab, deckZone, Quaternion.identity);
			card.transform.localPosition = new Vector3(-zIndex * 0.5f, zIndex * 0.5f, 0);
			var ui = card.GetComponent<CardUI>();
			if (ui != null)
			{
				ui.EnableCardFlip = false;
				ui.ownerType = OwnerType.Opponent;
				ui.FlipCard(false);
			}
			if (card.GetComponent<FildMonster>() == null) card.AddComponent<FildMonster>();
			if (card.GetComponent<AuraTracker>() == null) card.AddComponent<AuraTracker>();
			zIndex++;
			deck.Add(card);
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

            card.GetComponent<CardUI>().FlipCard(true); // 카드 앞면으로 설정
            card.transform.SetParent(handZone, false);
            card.transform.localScale = Vector3.one;
        }

        UpdateHandLayout();
    }

    public void DrawCard() => DrawCards(1);

    public void UpdateHandLayout()
    {
        int cardCount = handZone.childCount;
        if (cardCount == 0) return;

        RectTransform handRect = handZone.GetComponent<RectTransform>();
        var layout = handZone.GetComponent<HorizontalLayoutGroup>();
        if (!layout) return;
        float maxWidth = handRect.rect.width;

        float cardWidth = 150f;    // 카드 너비 (실제 카드 크기로 맞춰야 함)
        float minSpacing = -40f;    // 최소 간격
        float maxSpacing = 60f;    // 최대 간격

        // 카드 간격 기본값
        float spacing = Mathf.Clamp(maxSpacing - 20 * (cardCount - 2), minSpacing, maxSpacing);

        layout.spacing = spacing; // 레이아웃 그룹에 간격 설정

        // 필요시 즉시 반영 (대부분 없어도 되지만 안전하게)
        LayoutRebuilder.ForceRebuildLayoutImmediate(handRect);
        Canvas.ForceUpdateCanvases();

        //float spacing = 150f;
        //for (int i = 0; i < handZone.childCount; i++)
        //{
        //    RectTransform rt = handZone.GetChild(i).GetComponent<RectTransform>();
        //    if (rt != null)
        //    {
        //        rt.anchoredPosition = new Vector2(i * spacing, 0);
        //    }
        //}
    }

    //핸드존 
    public void MouseOnHandZone()
    {
        var layoutGroup = handZone.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null) return;

        // 1) pad 복사/수정
        var pad = layoutGroup.padding;
        pad.top = -50;

        // 2) 재할당(중요!)
        layoutGroup.padding = pad;

        // 3) 강제 리빌드
        LayoutRebuilder.ForceRebuildLayoutImmediate(handZone as RectTransform);
        Canvas.ForceUpdateCanvases();
    }

    public void MouseExitHandZone()
    {
        var layoutGroup = handZone.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null) return;

        var pad = layoutGroup.padding;
        pad.top = 300;                         // 네가 원하는 값
        layoutGroup.padding = pad;             // 재할당(중요!)

        LayoutRebuilder.ForceRebuildLayoutImmediate(handZone as RectTransform);
        Canvas.ForceUpdateCanvases();
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

        // AuraTracker 보장: 없으면 추가
        if (card.GetComponent<AuraTracker>() == null)
        {
            card.AddComponent<AuraTracker>();
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
	private const string EnemyFieldZoneName = "MonsterZoneG_E"; // 적 필드 전용 존 이름 (폴백)
	private readonly string[] enemySlotNames = new string[] { "E_MonsterZone1", "E_MonsterZone2", "E_MonsterZone3", "E_MonsterZone4", "E_MonsterZone5" };
	private Transform[] enemySlots;

	/// <summary>
	/// 적 몬스터 슬롯(E_MonsterZone1~5)에 빈 자리가 있는지 여부
	/// </summary>
	public bool HasEmptyEnemySlot()
	{
		EnsureEnemySlots();
		if (enemySlots == null || enemySlots.Length == 0)
		{
			// 슬롯을 못 찾는 경우, 폴백 필드 유무에 따라 판단
			EnsureFieldZone();
			return fieldZone != null; // 폴백 필드가 있으면 일단 배치 가능으로 간주
		}
		foreach (var slot in enemySlots)
		{
			if (slot == null) continue;
			if (slot.name == "EnemyGraveZone") continue;
			if (slot.childCount == 0) return true;
		}
		return false;
	}

	/// <summary>
	/// 리스트 셔플 (Fisher-Yates)
	/// </summary>
	private void ShuffleDeckData<T>(List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int r = Random.Range(i, list.Count);
			(list[i], list[r]) = (list[r], list[i]);
		}
	}

	/// <summary>
	/// 현재 남은 덱(GameObject 리스트)을 셔플 (게임 중에도 호출 가능)
	/// </summary>
	public void ShuffleDeck()
	{
		if (deck == null || deck.Count <= 1) return;
		for (int i = 0; i < deck.Count; i++)
		{
			int r = Random.Range(i, deck.Count);
			(deck[i], deck[r]) = (deck[r], deck[i]);
		}

		// 시각 정렬(선택): 덱존에서 카드 스택 위치 재배치
		if (deckZone != null)
		{
			int zIndex = 0;
			foreach (var card in deck)
			{
				if (card == null) continue;
				card.transform.SetParent(deckZone, false);
				card.transform.localPosition = new Vector3(-zIndex * 0.5f, zIndex * 0.5f, 0);
				zIndex++;
			}
		}
	}

	public void PlayCardToField(GameObject card)
	{
		// 몬스터 전용: 스펠/함정은 몬스터존(소환존)으로 배치 금지
		var uiCheck = card != null ? card.GetComponent<CardUI>() : null;
		if (uiCheck == null || uiCheck.cardData == null)
		{
			Debug.LogWarning("[OpponentCardManager] 유효하지 않은 카드로 PlayCardToField 호출");
			return;
		}
		if (uiCheck.cardData.cardType != CardType.Monster)
		{
			Debug.LogWarning("[OpponentCardManager] 몬스터존에는 몬스터 카드만 배치할 수 있습니다.");
			return;
		}

		// 1) 적 몬스터 슬롯(E_MonsterZone1~5) 중 빈 슬롯에만 배치
		EnsureEnemySlots();
		Transform parentSlot = null;
		bool hasEnemySlots = enemySlots != null && enemySlots.Length > 0;
		if (hasEnemySlots)
		{
			foreach (var slot in enemySlots)
			{
				if (slot == null) continue;
				if (slot.name == "EnemyGraveZone") continue; // 안전 장치
				if (slot.childCount == 0)
				{
					parentSlot = slot;
					break;
				}
			}
			// 빈 슬롯이 없다면 배치하지 않음
			if (parentSlot == null)
			{
				Debug.LogWarning("[OpponentCardManager] 적 몬스터 슬롯이 가득 찼습니다. 배치 취소.");
				return;
			}
		}

		// 2) 적 슬롯 자체를 찾지 못한 경우에만 폴백 필드 사용
		if (!hasEnemySlots)
		{
			EnsureFieldZone();
			if (fieldZone == null)
			{
				Debug.LogWarning("[OpponentCardManager] 적 몬스터 슬롯과 폴백 필드 모두 찾지 못했습니다. 배치 취소.");
				return;
			}
			parentSlot = fieldZone;
		}

        card.transform.SetParent(parentSlot, false);
		card.transform.localScale = Vector3.one;
		card.transform.localRotation = Quaternion.identity;
		card.transform.localPosition = Vector3.zero;
		card.transform.SetAsLastSibling();
		// UI RectTransform을 슬롯 중앙에 정렬
		var rect = card.GetComponent<RectTransform>();
		if (rect != null)
		{
			rect.anchorMin = new Vector2(0.5f, 0.5f);
			rect.anchorMax = new Vector2(0.5f, 0.5f);
			rect.pivot = new Vector2(0.5f, 0.5f);
			rect.anchoredPosition = Vector2.zero;
		}

		var cardUI = card.GetComponent<CardUI>();
		if (cardUI != null)
		{
			cardUI.isOnField = true;
		}

        // AuraTracker 보장: 없으면 추가
        if (card.GetComponent<AuraTracker>() == null)
        {
            card.AddComponent<AuraTracker>();
        }

        // 필드 컨테이너에 직접 붙은 경우에만 필드 레이아웃 업데이트
        if (parentSlot == fieldZone)
        {
            UpdateFieldLayout();
        }
	}

	private void UpdateFieldLayout()
	{
		float spacing = 160f;
		int total = fieldZone.childCount;
		for (int i = 0; i < total; i++)
		{
			RectTransform rt = fieldZone.GetChild(i).GetComponent<RectTransform>();
			if (rt != null)
			{
				Vector3 pos = GetSlotPosition(i, total, spacing);
				rt.anchoredPosition = new Vector2(pos.x, pos.y);
			}
		}
	}

	private void EnsureFieldZone()
	{
        if (fieldZone != null && fieldZone.name == EnemyFieldZoneName) return;
        var go = GameObject.Find(EnemyFieldZoneName);
		if (go != null)
		{
			fieldZone = go.transform;
		}
	}

	private void EnsureEnemySlots()
	{
		if (enemySlots != null && enemySlots.Length == enemySlotNames.Length)
		{
			bool allSet = true;
			for (int i = 0; i < enemySlots.Length; i++)
			{
				if (enemySlots[i] == null) { allSet = false; break; }
			}
			if (allSet) return;
		}

		enemySlots = new Transform[enemySlotNames.Length];
		for (int i = 0; i < enemySlotNames.Length; i++)
		{
			var go = GameObject.Find(enemySlotNames[i]);
			if (go != null)
				enemySlots[i] = go.transform;
		}
	}

    #endregion

    #region AI를 위한 메서드들

    /// <summary>
    /// 핸드 카드 개수 반환
    /// </summary>
    public int GetHandCardCount()
    {
        if (handZone == null) return 0;
        return handZone.childCount;
    }

    /// <summary>
    /// 핸드의 모든 카드 데이터 반환
    /// </summary>
    public List<BaseCardData> GetHandCards()
    {
        var handCards = new List<BaseCardData>();
        if (handZone == null) return handCards;
        
        for (int i = 0; i < handZone.childCount; i++)
        {
            var child = handZone.GetChild(i);
            if (child == null) continue;
            
            var cardUI = child.GetComponent<CardUI>();
            if (cardUI != null && cardUI.cardData != null)
            {
                handCards.Add(cardUI.cardData);
            }
        }
        return handCards;
    }

    /// <summary>
    /// 카드 소환 (AI용)
    /// </summary>
    public System.Collections.IEnumerator SummonCard(BaseCardData cardData)
    {
        if (cardData == null) yield break;
        if (handZone == null) yield break;

        // 몬스터가 아닌 카드(스펠/함정)는 소환 경로 금지: 반드시 전용 사용 플로우 사용
        if (cardData is SpellCardData || cardData is TrapCardData)
        {
            Debug.LogWarning("[OpponentCardManager] 스펠/함정 카드는 SummonCard로 소환할 수 없습니다. 전용 사용 플로우를 사용하세요.");
            yield break;
        }

        // 핸드에서 해당 카드 찾기
        GameObject cardToSummon = null;
        for (int i = 0; i < handZone.childCount; i++)
        {
            var child = handZone.GetChild(i);
            if (child == null) continue;
            
            var childCardUI = child.GetComponent<CardUI>();
            if (childCardUI != null && childCardUI.cardData == cardData)
            {
                cardToSummon = child.gameObject;
                break;
            }
        }

        if (cardToSummon == null)
        {
            Debug.LogWarning("[OpponentCardManager] 핸드에서 소환할 카드를 찾을 수 없습니다.");
            yield break;
        }

        // 필드로 이동 (몬스터 전용)
        PlayCardToField(cardToSummon);

        // 카드 UI 설정
        var cardUIComponent = cardToSummon.GetComponent<CardUI>();
        if (cardUIComponent != null)
        {
            cardUIComponent.isOnField = true;
            cardUIComponent.ownerType = OwnerType.Opponent;
            cardUIComponent.FlipCard(true);
        }

        // 드래그 비활성화
        var dragHandler = cardToSummon.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.enabled = false;
        }

        Debug.Log($"[OpponentCardManager] {cardData.cardName} 소환 완료");
        yield return new WaitForSeconds(0.5f); // 소환 애니메이션 시간
    }

    /// <summary>
    /// 손패에서 해당 데이터의 카드 오브젝트를 찾아 반환 (AI용)
    /// </summary>
    public GameObject FindHandCardObject(BaseCardData data)
    {
        if (handZone == null || data == null) return null;
        for (int i = 0; i < handZone.childCount; i++)
        {
            var child = handZone.GetChild(i);
            var ui = child.GetComponent<CardUI>();
            if (ui != null && ui.cardData == data)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    #endregion
}
