using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카드 매니저. 덱을 로드하고 핸드로 드로우하며 카드 효과도 여기서 일부 조정 가능.
/// </summary>
public class CardManager_test : MonoBehaviour
{
    public static CardManager_test Instance;

    [Header("필드 연결")]
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

    /// <summary>
    /// 덱 데이터 로드 및 카드 프리팹 생성
    /// </summary>
    void LoadDeckFromTransfer()
    {
        currentDeckData = DeckTransferManager.Instance?.GetDeck();
        if (currentDeckData == null)
        {
            Debug.LogWarning("DeckTransferManager로부터 덱 데이터를 가져오지 못했습니다.");
            return;
        }

        // 카드 데이터를 Resources에서 재연결
        BaseCardData[] allCards = Resources.LoadAll<BaseCardData>("CardData");
        foreach (var entry in currentDeckData.mainDeck)
        {
            if (entry.card == null)
            {
                foreach (var c in allCards)
                {
                    if (c.cardId == entry.cardId)
                    {
                        entry.card = c;
                        break;
                    }
                }
            }
        }

        ClearDeck();

        int zIndex = 0;
        foreach (var cardEntry in currentDeckData.mainDeck)
        {
            for (int i = 0; i < cardEntry.count; i++)
            {
                GameObject card = Instantiate(cardPrefab, deckZone);
                card.transform.localScale = Vector3.one;
                card.transform.localPosition = new Vector3(0, 0, -zIndex * 0.01f);
                zIndex++;

                // 카드 UI 설정
                var cardUI = card.GetComponent<CardUI_N>();
                if (cardUI != null)
                {
                    cardUI.SetCard(cardEntry.card);
                }
                else
                {
                    Debug.LogWarning("CardUI_N 컴포넌트를 찾을 수 없습니다.");
                }

                // 카드 대상지정 컴포넌트 추가
                if (card.GetComponent<TargetableCard>() == null)
                    card.AddComponent<TargetableCard>();

                // 몬스터 소환 효과 설정
                var effect = card.GetComponent<MonsterEffectOnSummon>();
                if (effect == null)
                    effect = card.AddComponent<MonsterEffectOnSummon>();

                effect.cardData = cardEntry.card;
                effect.cardManager = this;

                deck.Add(card);
            }
        }

        // 초기 드로우
        DrawCards(5);
    }

    /// <summary>
    /// 덱 클리어
    /// </summary>
    private void ClearDeck()
    {
        foreach (var card in deck)
        {
            Destroy(card);
        }
        deck.Clear();
    }

    /// <summary>
    /// 카드 여러 장 드로우
    /// </summary>
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

    /// <summary>
    /// 카드 한 장 드로우
    /// </summary>
    public void DrawCard()
    {
        DrawCards(1);
    }

    /// <summary>
    /// 핸드 카드 정렬
    /// </summary>
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

    /// <summary>
    /// 카드 효과를 발동할 때 호출 (외부에서 사용)
    /// </summary>
    public void ResolveCard(PlayerController_N player, System.Action onComplete)
    {
        // 효과 발동 시 필요한 추가 로직을 작성
        Debug.Log("카드 효과 해결 중...");
        DrawCard(); // 예시 효과
        onComplete?.Invoke();
    }
}
