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
        LoadDeckFromTransfer();  // 덱을 로딩하고 카드 드로우 시작
    }

    #region 플레이어 덱 로딩 및 드로우

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
                zIndex++;

                deck.Add(card);
            }
        }

        DrawCards(5);  // 처음에 5개의 카드를 드로우
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

    #region 공통 카드 생성 메서드

    /// <summary>
    /// 카드 생성 및 UI, 효과 초기화
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
            cardUI.SetFace(true);  // 카드 뒷면으로 설정 (초기화)
            cardUI.EnableCardFlip = rotation == Quaternion.identity; // 플레이어 카드만 클릭 가능
        }

        // 드래그
        var dragHandler = card.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.enabled = rotation == Quaternion.identity;
        }

        // 카드 효과: 플레이어 카드에만 추가
        if (rotation == Quaternion.identity)
        {
            if (card.GetComponent<TargetableCard>() == null)
                card.AddComponent<TargetableCard>();

            var effect = card.GetComponent<MonsterEffectOnSummon>();
            if (effect == null) effect = card.AddComponent<MonsterEffectOnSummon>();

            effect.cardData = data;
            effect.cardManager = this;
        }

        return card;
    }

    #endregion

    #region 카드 효과 처리

    public void ResolveCard(PlayerController_N player, System.Action onComplete)
    {
        Debug.Log("카드 효과 해결 중...");
        DrawCard(); // 예시: 카드 드로우
        onComplete?.Invoke();
    }

    #endregion
}
