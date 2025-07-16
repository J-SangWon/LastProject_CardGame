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
    public Transform cardPackDisplayPoint;        // 카드팩 중앙 표시 위치
    public GameObject[] rarityEffects;            // 0~3 : Normal/Rare/SR/UR 파티클 등

    // ────────────────── 카드 소환용 ──────────────────
    [Header("카드 소환")]
    public CardPackViewController packViewController;
    public GameObject cardPrefab;                 // 1장 카드 프리팹
    public Transform cardSpawnContent;            // 10장 배치 부모
    public GameObject cardSpawnPanel;             // 검은 배경 + 카드 영역
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
    private readonly List<CardInfo> cardList = new();   // 클릭 후 실제 카드 정보 저장

    // ────────────────── 초기화 ──────────────────
    void Start()
    {
        coinText.text = coin.ToString();
        buyButton.onClick.AddListener(BuyCard);
        cardPanelExit.onClick.AddListener(ClosePanel);

        // 스크롤 도중 구매 방지 (옵션)
        packViewController.onDragStart += () => buyButton.interactable = false;
        packViewController.onSnapEnd += () => buyButton.interactable = true;

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
    }

    // ────────────────── Buy 클릭 ──────────────────
    void BuyCard()
    {
        if (isOpening || coin < 10) return;

        coin -= 10;
        coinText.text = coin.ToString();
        isOpening = true;

        rarityList.Clear();
        cardList.Clear();

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
        currentPack = Instantiate(packPrefab, cardPackDisplayPoint);
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
            particle = Instantiate(rarityEffects[idx], cardPackDisplayPoint);
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
        CardPackType type = packViewController.selectedCardPackView.cardPackData.packType;

        foreach (CardRarity r in rarityList)
        {
            Race race = (Race)Random.Range(0, System.Enum.GetValues(typeof(Race)).Length);
            cardList.Add(new CardInfo(r, race, type));
        }
    }

    // ────────────────── 카드 Instantiate ──────────────────
    IEnumerator SpawnCards()
    {
        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        yield return new WaitForSeconds(0.25f);

        GridLayoutGroup g = cardSpawnContent.GetComponent<GridLayoutGroup>();
        Vector2 cell = g.cellSize, sp = g.spacing;
        RectOffset pad = g.padding; int cols = g.constraintCount;

        Vector2 origin = new(pad.left + cell.x * .5f,
                            -(pad.top + cell.y * .5f));

        bool skipDelay = false;

        for (int i = 0; i < cardList.Count; i++)
        {
            CardInfo info = cardList[i];

            // Instantiate
            GameObject obj = Instantiate(cardPrefab, cardSpawnContent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, -900);
            rt.localScale = Vector3.one * .8f;

            CardPrefab cp = obj.GetComponent<CardPrefab>();
            cp.Initialize(info.rarity, info.race, info.type);

            // 목표 좌표
            int row = i / cols, col = i % cols;
            Vector2 target = origin + new Vector2(
                col * (cell.x + sp.x),
               -(row * (cell.y + sp.y)));

            float d = 0.06f * i;

            // 코루틴 분리 호출
            StartCoroutine(AnimateCardToGrid(rt, target, d, cp));

            // 클릭 시 나머지 즉시
            if (!skipDelay)
            {
                float t = 0, wait = .1f;
                while (t < wait)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        skipDelay = true;
                        DOTween.Kill(rt);               // 즉시 완주
                        rt.anchoredPosition = target;
                        rt.localScale = Vector3.one;
                        StartCoroutine(cp.Flip());      // 바로 Flip
                        break;
                    }
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }
        cardPanelExit.gameObject.SetActive(true);
    }

    // ────────────────── 카드 1장 애니메이션 (이동 + 스케일 + Flip) ──────────────────
    IEnumerator AnimateCardToGrid(RectTransform rt, Vector2 target, float baseDelay, CardPrefab cp)
    {
        // 올라오기 + 스케일
        rt.DOAnchorPos(target, 0.35f)
          .SetEase(Ease.OutQuad)
          .SetDelay(baseDelay);

        rt.DOScale(1f, 0.35f)
          .SetEase(Ease.OutBack)
          .SetDelay(baseDelay);

        // 0.1초 뒤 Flip
        yield return new WaitForSeconds(baseDelay + 0.1f);
        yield return cp.Flip();   // CardPrefab 내부 Flip 코루틴
    }

    // ────────────────── Close ──────────────────
    void ClosePanel()
    {
        //카드 정보 저장


        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        if (currentPack) Destroy(currentPack);

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
        isOpening = false;
    }

    // ────────────────── 내부 카드 구조체 ──────────────────
    struct CardInfo
    {
        public CardRarity rarity;
        public Race race;
        public CardPackType type;
        public CardInfo(CardRarity r, Race ra, CardPackType t)
        { rarity = r; race = ra; type = t; }
    }
}
