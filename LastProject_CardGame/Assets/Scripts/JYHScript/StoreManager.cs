using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Buy 幗が ⊥ �騉芚� 葬蝶お 儅撩
/// 譆堅 �騉芚翕� 蜃朝 檜めお 轎溘 + 蘋萄ね �捎�
/// 贗葛(�分� 攪纂) 衛 褒薯 蘋萄 10濰 Instantiate
/// </summary>
public class StoreManager : MonoBehaviour
{
    // 式式式式式式式式式式式式式式式式式式 等檜攪 & �捕� 式式式式式式式式式式式式式式式式式式
    [Header("蘋萄 偃熱 / �捕�")]
    public int cardCount = 10;

    [Range(0, 100)] public int normalRate = 60;
    [Range(0, 100)] public int rareRate = 30;
    [Range(0, 100)] public int superRareRate = 9;
    [Range(0, 100)] public int ultraRareRate = 1;

    // 式式式式式式式式式式式式式式式式式式 翱轎辨 螃粽薛お 式式式式式式式式式式式式式式式式式式
    [Header("翱轎 螃粽薛お")]
    public Transform cardPackDisplayPoint;        // 蘋萄ね 醞懈 ル衛 嬪纂
    public GameObject[] rarityEffects;            // 0~3 : Normal/Rare/SR/UR だじ贗 蛔

    // 式式式式式式式式式式式式式式式式式式 蘋萄 模�紊� 式式式式式式式式式式式式式式式式式式
    [Header("蘋萄 模��")]
    public CardPackViewController packViewController;
    public GameObject cardPrefab;                 // 1濰 蘋萄 Щ葬ぱ
    public Transform cardSpawnContent;            // 10濰 寡纂 睡賅
    public GameObject cardSpawnPanel;             // 匐擎 寡唳 + 蘋萄 艙羲
    public Button cardOpenBtn;
    public Button cardPanelExit;                  // 殘晦 幗が

    // 式式式式式式式式式式式式式式式式式式 UI 式式式式式式式式式式式式式式式式式式
    [Header("鼻薄 UI")]
    public Text coinText;
    public Button buyButton;
    public int coin = 100;

    // 式式式式式式式式式式式式式式式式式式 頂睡 鼻鷓 式式式式式式式式式式式式式式式式式式
    private bool isOpening = false;
    private GameObject currentPack;
    private GameObject particle;
    private readonly List<CardRarity> rarityList = new();   // �騉芚絡� 盪濰
    private readonly List<CardInfo> cardList = new();   // 贗葛 �� 褒薯 蘋萄 薑爾 盪濰
    private bool skipRemaining = false; // 贗葛 衛 檜�� 蘋萄 闊衛 寡纂 罹睡  

    // 式式式式式式式式式式式式式式式式式式 蟾晦�� 式式式式式式式式式式式式式式式式式式
    void Start()
    {
        coinText.text = coin.ToString();
        buyButton.onClick.AddListener(BuyCard);
        cardOpenBtn.onClick.AddListener(CardAllOpen);
        cardPanelExit.onClick.AddListener(ClosePanel);

        // 蝶觼煤 紫醞 掘衙 寞雖 (褫暮)
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

    // 式式式式式式式式式式式式式式式式式式 Buy 贗葛 式式式式式式式式式式式式式式式式式式
    void BuyCard()
    {
        if (isOpening || coin < 10) return;

        coin -= 10;
        coinText.text = coin.ToString();
        isOpening = true;

        GenerateRarityList();                    // 1) �騉芚絡� 鉻晦
        StartCoroutine(ShowPackAndEffect());     // 2) 檜めお & ね 蛔濰
    }

    // 式式式式式式式式式式式式式式式式式式 1) �騉芚� 鉻晦 式式式式式式式式式式式式式式式式式式
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

    // 式式式式式式式式式式式式式式式式式式 2) 蘋萄ね + 檜めお 式式式式式式式式式式式式式式式式式式
    IEnumerator ShowPackAndEffect()
    {
        cardSpawnPanel.SetActive(true);

        // 蝶觼煤 撚 犒薯 渠褐, 翱轎 瞪辨 賅筐 Prefab擊 CardPackData縑 厥橫舒朝 ら檜 謠擠
        if (currentPack) Destroy(currentPack);
        if (particle) Destroy(particle);

        // 蘋萄ね 釭顫頂晦
        yield return CardPackAppear();
        // 贗葛 渠晦
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        // 霞翕 + 嬴楚煎 餌塭颶
        yield return AnimatePackVanish();
        // 3) 褒薯 蘋萄 等檜攪 儅撩 �� 蛔濰
        GenerateCardsFromRarities();
        yield return StartCoroutine(SpawnCards());
    }

    // 式式式式式式式式式式式式式式式式式式 檜めお 翱轎 式式式式式式式式式式式式式式式式式式
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

    void ShowEffect(CardRarity rarity)  //蘋萄ね だじ贗
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

    //蘋萄ね 霞翕 塽 餌塭颶
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

    // 式式式式式式式式式式式式式式式式式式 3) �騉芚� ⊥ 褒薯 蘋萄 滲�� 式式式式式式式式式式式式式式式式式式
    void GenerateCardsFromRarities()
    {
        CardPackType type = packViewController.selectedCardPackView.cardPackData.packType;

        foreach (CardRarity r in rarityList)
        {
            Race race = (Race)Random.Range(0, System.Enum.GetValues(typeof(Race)).Length);
            cardList.Add(new CardInfo(r, race, type));
        }
    }

    // 式式式式式式式式式式式式式式式式式式 蘋萄 Instantiate 式式式式式式式式式式式式式式式式式式
    IEnumerator SpawnCards()
    {
        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        skipRemaining = false;
        yield return new WaitForSeconds(0.25f);

        // 醞懈 晦遽 嬪纂 啗骯
        int cols = 5;
        float cellW = 250f, cellH = 350f;
        float spacingX = 50f, spacingY = 50f;

        int rows = Mathf.CeilToInt(cardList.Count / (float)cols);

        // 瞪羹 傘綠, 堪檜
        float totalWidth = cols * cellW + (cols - 1) * spacingX;
        float totalHeight = rows * cellH + (rows - 1) * spacingY;

        // 醞懈 晦遽 謝鼻欽 晦遽薄 啗骯
        Vector2 startPos = new Vector2(
            -totalWidth / 2f + cellW / 2f,
             totalHeight / 2f - cellH / 2f
        );

        StartCoroutine(DetectClickToSkip());

        for (int i = 0; i < cardList.Count; i++)
        {
            CardInfo info = cardList[i];
            int row = i / cols;
            int col = i % cols;

            Vector2 target = startPos + new Vector2(
                col * (cellW + spacingX),
               -row * (cellH + spacingY)
            );

            GameObject obj = Instantiate(cardPrefab, cardSpawnContent);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -900f);
            rt.localScale = Vector3.one * 0.8f;

            CardPrefab cp = obj.GetComponent<CardPrefab>();
            cp.Initialize(info.rarity, info.race, info.type);

            if (skipRemaining)
            {
                // DOTween 橈檜 闊衛 檜翕
                rt.anchoredPosition = target;
                rt.localScale = Vector3.one;
                continue;
            }

            // 蘋萄 1濰 擁棲詭檜暮 (檜翕 + 蝶馨橾)
            yield return AnimateCardToGrid(rt, target);

            // 贗葛 馬雖 �� 檜�� 蘋萄睡攪 闊衛 寡纂
            if (Input.GetMouseButtonDown(0))
            {
                skipRemaining = true;
            }
        }

        cardOpenBtn.gameObject.SetActive(true);
    }

    // 式式式式式式式式式式式式式式式式式式 蘋萄 1濰 擁棲詭檜暮 (檜翕 + 蝶馨橾) 式式式式式式式式式式式式式式式式式式
    IEnumerator AnimateCardToGrid(RectTransform rt, Vector2 target)
    {
        float moveTime = 0.25f;

        Tween move = rt.DOAnchorPos(target, moveTime).SetEase(Ease.OutQuad);
        Tween scale = rt.DOScale(1f, moveTime).SetEase(Ease.OutBack);

        yield return move.WaitForCompletion();
    }

    // 式式式式式式式式式式式式式式式式式 贗葛 馬雖 式式式式式式式式式式式式式式式式式式
    IEnumerator DetectClickToSkip()
    {
        while (!skipRemaining)
        {
            if (Input.GetMouseButtonDown(0))  // 賅夥橾擎 攪纂煎 掖羹 陛棟
            {
                skipRemaining = true;
                yield break;
            }
            yield return null; // 棻擠 Щ溯歜梱雖 渠晦
        }
    }

    // 式式式式式式式式式式式式式式式式式式 賅萇 蘋萄 菴餵晦 式式式式式式式式式式式式式式式式式式
    void CardAllOpen()
    {
        foreach (Transform child in cardSpawnContent)
        {
            var card = child.GetComponent<CardPrefab>();
            if (card != null && !card.isFlipped)
                StartCoroutine(card.Flip());
        }
    }

    // 式式式式式式式式式式式式式式式式式式 蘋萄 flip鼻鷓 �挫� 式式式式式式式式式式式式式式式式式式
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

    // 式式式式式式式式式式式式式式式式式式 Close 式式式式式式式式式式式式式式式式式式
    void ClosePanel()
    {
        //蘋萄 薑爾 盪濰
        //cardList

        //薑爾 蟾晦��
        rarityList.Clear();
        cardList.Clear();

        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        if (currentPack) Destroy(currentPack);

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
        isOpening = false;
    }

    // 式式式式式式式式式式式式式式式式式式 頂睡 蘋萄 掘褻羹 式式式式式式式式式式式式式式式式式式
    struct CardInfo
    {
        public CardRarity rarity;
        public Race race;
        public CardPackType type;
        public CardInfo(CardRarity r, Race ra, CardPackType t)
        { rarity = r; race = ra; type = t; }
    }
}
