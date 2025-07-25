using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Buy 버튼 → 희귀도 리스트 생성
/// 최고 희귀도에 맞는 이펙트 출력 + 카드팩 확대
/// 클릭(혹은 터치) 시 실제 카드 10장 Instantiate
/// </summary>
public class StoreManager : MonoBehaviour
{
    // ────────────────── 데이터 & 확률 ──────────────────
    [Header("카드 개수 / 확률")]
    public int cardCount = 10;

    [Range(0, 100)] public int normalRate = 60;
    [Range(0, 100)] public int rareRate = 30;
    [Range(0, 100)] public int superRareRate = 9;
    [Range(0, 100)] public int ultraRareRate = 1;

    // ────────────────── 연출용 오브젝트 ──────────────────
    [Header("연출 오브젝트")]
    public Transform cardPackContainer;        // 카드팩 중앙 표시 위치
    public GameObject[] rarityEffects;            // 0~3 : Normal/Rare/SR/UR 파티클 등

    // ────────────────── 카드 소환용 ──────────────────
    [Header("카드 소환")]
    public CardPackViewController packViewController;
    public GameObject cardPrefab;                 // 1장 카드 프리팹
    public Transform cardSpawnContent;            // 10장 배치 부모
    public GameObject cardSpawnPanel;             // 검은 배경 + 카드 영역
    public Button cardOpenBtn;
    public Button cardPanelExit;                  // 닫기 버튼

    // ────────────────── UI ──────────────────
    [Header("상점 UI")]
    public Text coinText;
    public Button buyButton;
    public int coin = 100;

    // ────────────────── 내부 상태 ──────────────────
    private bool isOpening = false;
    private GameObject currentPack;
    private GameObject particle;
    private readonly List<CardRarity> rarityList = new();   // 희귀도만 저장
    private readonly List<BaseCardData> cardList = new();   // 클릭 후 실제 카드 정보 저장
    private bool skipRemaining = false; // 클릭 시 이후 카드 즉시 배치 여부  

    // ────────────────── 초기화 ──────────────────
    void Start()
    {
        coinText.text = coin.ToString();
        buyButton.onClick.AddListener(BuyCard);
        cardOpenBtn.onClick.AddListener(CardAllOpen);
        cardPanelExit.onClick.AddListener(ClosePanel);

        // 스크롤 도중 구매 방지 (옵션)
        packViewController.onDragStart += () => buyButton.interactable = false;
        packViewController.onSnapEnd += () => buyButton.interactable = true;

        cardSpawnPanel.SetActive(false);
        cardOpenBtn.gameObject.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isOpening && !cardPanelExit.gameObject.activeSelf)
        {
            if (AreAllCardsFlipped() && cardSpawnContent.childCount > 0)
            {
                cardOpenBtn.gameObject.SetActive(false);
                cardPanelExit.gameObject.SetActive(true);
            }
        }
    }

    // ────────────────── Buy 클릭 ──────────────────
    void BuyCard()
    {
        if (isOpening || coin < 10) return;

        coin -= 10;
        coinText.text = coin.ToString();
        isOpening = true;

        GenerateRarityList();                    // 1) 희귀도만 뽑기
        StartCoroutine(ShowPackAndEffect());     // 2) 이펙트 & 팩 등장
    }

    // ────────────────── 1) 희귀도 뽑기 ──────────────────
    void GenerateRarityList()
    {
        for (int i = 0; i < cardCount; i++)
            rarityList.Add(RandomRarity());
    }

    CardRarity RandomRarity()
    {
        int v = Random.Range(0, 100);
        if (v < normalRate) return CardRarity.Normal;
        if (v < normalRate + rareRate) return CardRarity.Rare;
        if (v < normalRate + rareRate + superRareRate) return CardRarity.SuperRare;
        return CardRarity.UltraRare;
    }

    // ────────────────── 2) 카드팩 + 이펙트 ──────────────────
    IEnumerator ShowPackAndEffect()
    {
        cardSpawnPanel.SetActive(true);

        // 스크롤 셀 복제 대신, 연출 전용 모델 Prefab을 CardPackData에 넣어두는 편이 좋음
        if (currentPack) Destroy(currentPack);
        if (particle) Destroy(particle);

        // 카드팩 나타내기
        yield return CardPackAppear();
        // 클릭 대기
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        // 진동 + 아래로 사라짐
        yield return AnimatePackVanish();
        // 3) 실제 카드 데이터 생성 후 등장
        GenerateCardsFromRarities();
        yield return StartCoroutine(SpawnCards());
    }

    // ────────────────── 이펙트 연출 ──────────────────
    IEnumerator CardPackAppear()
    {
        GameObject packPrefab = packViewController.selectedCardPackView.gameObject;
        currentPack = Instantiate(packPrefab, cardPackContainer);
        RectTransform rect = currentPack.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 540);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        currentPack.transform.localScale = Vector3.zero;

        ShowEffect(GetHighestRarity());

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            currentPack.transform.localScale = Vector3.one * Mathf.SmoothStep(0, 1, t);
            yield return null;
        }
    }

    void ShowEffect(CardRarity rarity)  //카드팩 파티클
    {
        int idx = (int)rarity;
        if (idx < rarityEffects.Length && rarityEffects[idx])
        {
            particle = Instantiate(rarityEffects[idx], cardPackContainer);
            particle.transform.localPosition = Vector3.zero;
        }
    }

    CardRarity GetHighestRarity()
    {
        CardRarity hi = CardRarity.Normal;
        foreach (var r in rarityList)
            if ((int)r > (int)hi) hi = r;
        return hi;
    }

    //카드팩 진동 및 사라짐
    IEnumerator AnimatePackVanish()
    {
        float shakeStrength = 10f;
        float shakeDuration = 0.3f;

        switch (GetHighestRarity())
        {
            case CardRarity.Rare: shakeStrength = 20f; shakeDuration = 0.5f; break;
            case CardRarity.SuperRare: shakeStrength = 30f; shakeDuration = 1f; break;
            case CardRarity.UltraRare: shakeStrength = 40f; shakeDuration = 1.5f; break;
        }

        currentPack.transform.DOShakeRotation(shakeDuration, shakeStrength);
        yield return new WaitForSeconds(shakeDuration + 0.1f);

        RectTransform rect = currentPack.GetComponent<RectTransform>();
        Vector2 endPos = rect.anchoredPosition + Vector2.down * 800f;

        if (particle) Destroy(particle);

        Sequence vanish = DOTween.Sequence();
        vanish.Append(rect.DOAnchorPos(endPos, 0.5f).SetEase(Ease.InBack));
        vanish.Join(currentPack.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));

        yield return vanish.WaitForCompletion();

        Destroy(currentPack);
    }

    // ────────────────── 3) 희귀도 → 실제 카드 변환 ──────────────────
    void GenerateCardsFromRarities()
    {
        var packData = packViewController.selectedCardPackView.cardPackData;

        foreach (CardRarity r in rarityList)
        {
            BaseCardData[] carddatas = packData.cards.FindAll(BaseCardData => BaseCardData.rarity == r).ToArray();

            if(carddatas.Length > 0)
            {
                // 랜덤으로 카드 선택
                BaseCardData selectedCard = carddatas[Random.Range(0, carddatas.Length)];
                cardList.Add(selectedCard);
            }
            else
            {
                Debug.LogWarning($"No cards found for rarity: {r}");
            }

        }
    }

    // ────────────────── 카드 Instantiate ──────────────────
    IEnumerator SpawnCards()
    {
        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        skipRemaining = false;
        yield return new WaitForSeconds(0.25f);

        // 중앙 기준 위치 계산
        int cols = 5;
        float cellW = 250f, cellH = 350f;
        float spacingX = 80f, spacingY = 100f;

        int rows = Mathf.CeilToInt(cardList.Count / (float)cols);

        // 전체 너비, 높이
        float totalWidth = cols * cellW + (cols - 1) * spacingX;
        float totalHeight = rows * cellH + (rows - 1) * spacingY;

        // 중앙 기준 좌상단 기준점 계산
        Vector2 startPos = new Vector2(
            -totalWidth / 2f + cellW / 2f,
             totalHeight / 2f - cellH / 2f
        );

        StartCoroutine(DetectClickToSkip());

        for (int i = 0; i < cardList.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector2 target = startPos + new Vector2(
                col * (cellW + spacingX),
               -row * (cellH + spacingY)
            );

            BaseCardData data = cardList[i];
            GameObject obj = Instantiate(cardPrefab, cardSpawnContent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -900f);
            rt.localScale = Vector3.one * 0.5f;

            CardPrefab cp = obj.GetComponent<CardPrefab>();
            cp.Init(data);

            if (skipRemaining)
            {
                // DOTween 없이 즉시 이동
                rt.anchoredPosition = target;
                rt.localScale = Vector3.one * 1.3f;
                continue;
            }

            // 카드 1장 애니메이션 (이동 + 스케일)
            yield return AnimateCardToGrid(rt, target);

            // 클릭 감지 후 이후 카드부터 즉시 배치
            if (Input.GetMouseButtonDown(0))
            {
                skipRemaining = true;
            }
        }

        cardOpenBtn.gameObject.SetActive(true);
    }

    // ────────────────── 카드 1장 애니메이션 (이동 + 스케일) ──────────────────
    IEnumerator AnimateCardToGrid(RectTransform rt, Vector2 target)
    {
        float moveTime = 0.25f;

        Tween move = rt.DOAnchorPos(target, moveTime).SetEase(Ease.OutQuad);
        Tween scale = rt.DOScale(1.3f, moveTime).SetEase(Ease.OutBack);

        yield return move.WaitForCompletion();
    }

    // ───────────────── 클릭 감지 ──────────────────
    IEnumerator DetectClickToSkip()
    {
        while (!skipRemaining)
        {
            if (Input.GetMouseButtonDown(0))  // 모바일은 터치로 교체 가능
            {
                skipRemaining = true;
                yield break;
            }
            yield return null; // 다음 프레임까지 대기
        }
    }

    // ────────────────── 모든 카드 뒤집기 ──────────────────
    void CardAllOpen()
    {
        foreach (Transform child in cardSpawnContent)
        {
            var card = child.GetComponent<CardPrefab>();
            if (card != null && !card.isFlipped)
                card.Flip(true);
        }
    }

    // ────────────────── 카드 flip상태 확인 ──────────────────
    bool AreAllCardsFlipped()
    {
        foreach (Transform child in cardSpawnContent)
        {
            var card = child.GetComponent<CardPrefab>();
            if (card != null && !card.isFlipped)
                return false;
        }
        return true;
    }

    // ────────────────── Close ──────────────────
    void ClosePanel()
    {
        //카드 정보 저장
        foreach(BaseCardData card in cardList)
        {
            PlayerCardCollectionManager.Instance.AddCard(card.cardId);
        }
        //cardList

        //정보 초기화
        rarityList.Clear();
        cardList.Clear();

        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        if (currentPack) Destroy(currentPack);

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
        isOpening = false;
    }

}
