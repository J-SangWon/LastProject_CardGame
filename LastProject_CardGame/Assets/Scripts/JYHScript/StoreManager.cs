using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Buy ¹öÆ° ¡æ Èñ±Íµµ ¸®½ºÆ® »ý¼º
/// ÃÖ°í Èñ±Íµµ¿¡ ¸Â´Â ÀÌÆåÆ® Ãâ·Â + Ä«µåÆÑ È®´ë
/// Å¬¸¯(È¤Àº ÅÍÄ¡) ½Ã ½ÇÁ¦ Ä«µå 10Àå Instantiate
/// </summary>
public class StoreManager : MonoBehaviour
{
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ µ¥ÀÌÅÍ & È®·ü ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("Ä«µå °³¼ö / È®·ü")]
    public int cardCount = 10;

    [Range(0, 100)] public int normalRate = 60;
    [Range(0, 100)] public int rareRate = 30;
    [Range(0, 100)] public int superRareRate = 9;
    [Range(0, 100)] public int ultraRareRate = 1;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ¿¬Ãâ¿ë ¿ÀºêÁ§Æ® ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("¿¬Ãâ ¿ÀºêÁ§Æ®")]
    public Transform cardPackContainer;        // Ä«µåÆÑ Áß¾Ó Ç¥½Ã À§Ä¡
    public GameObject[] rarityEffects;            // 0~3 : Normal/Rare/SR/UR ÆÄÆ¼Å¬ µî

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µå ¼ÒÈ¯¿ë ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("Ä«µå ¼ÒÈ¯")]
    public CardPackViewController packViewController;
    public GameObject cardPrefab;                 // 1Àå Ä«µå ÇÁ¸®ÆÕ
    public Transform cardSpawnContent;            // 10Àå ¹èÄ¡ ºÎ¸ð
    public GameObject cardSpawnPanel;             // °ËÀº ¹è°æ + Ä«µå ¿µ¿ª
    public Button cardOpenBtn;
    public Button cardPanelExit;                  // ´Ý±â ¹öÆ°

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ UI ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("»óÁ¡ UI")]
    public Text coinText;
    public Button buyButton;
    public int coin = 100;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ³»ºÎ »óÅÂ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private bool isOpening = false;
    private GameObject currentPack;
    private GameObject particle;
    private readonly List<CardRarity> rarityList = new();   // Èñ±Íµµ¸¸ ÀúÀå
    private readonly List<CardInfo> cardList = new();   // Å¬¸¯ ÈÄ ½ÇÁ¦ Ä«µå Á¤º¸ ÀúÀå
    private bool skipRemaining = false; // Å¬¸¯ ½Ã ÀÌÈÄ Ä«µå Áï½Ã ¹èÄ¡ ¿©ºÎ  

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ÃÊ±âÈ­ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void Start()
    {
        coinText.text = coin.ToString();
        buyButton.onClick.AddListener(BuyCard);
        cardOpenBtn.onClick.AddListener(CardAllOpen);
        cardPanelExit.onClick.AddListener(ClosePanel);

        // ½ºÅ©·Ñ µµÁß ±¸¸Å ¹æÁö (¿É¼Ç)
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Buy Å¬¸¯ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void BuyCard()
    {
        if (isOpening || coin < 10) return;

        coin -= 10;
        coinText.text = coin.ToString();
        isOpening = true;

        GenerateRarityList();                    // 1) Èñ±Íµµ¸¸ »Ì±â
        StartCoroutine(ShowPackAndEffect());     // 2) ÀÌÆåÆ® & ÆÑ µîÀå
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ 1) Èñ±Íµµ »Ì±â ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ 2) Ä«µåÆÑ + ÀÌÆåÆ® ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    IEnumerator ShowPackAndEffect()
    {
        cardSpawnPanel.SetActive(true);

        // ½ºÅ©·Ñ ¼¿ º¹Á¦ ´ë½Å, ¿¬Ãâ Àü¿ë ¸ðµ¨ PrefabÀ» CardPackData¿¡ ³Ö¾îµÎ´Â ÆíÀÌ ÁÁÀ½
        if (currentPack) Destroy(currentPack);
        if (particle) Destroy(particle);

        // Ä«µåÆÑ ³ªÅ¸³»±â
        yield return CardPackAppear();
        // Å¬¸¯ ´ë±â
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        // Áøµ¿ + ¾Æ·¡·Î »ç¶óÁü
        yield return AnimatePackVanish();
        // 3) ½ÇÁ¦ Ä«µå µ¥ÀÌÅÍ »ý¼º ÈÄ µîÀå
        GenerateCardsFromRarities();
        yield return StartCoroutine(SpawnCards());
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ÀÌÆåÆ® ¿¬Ãâ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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

    void ShowEffect(CardRarity rarity)  //Ä«µåÆÑ ÆÄÆ¼Å¬
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

    //Ä«µåÆÑ Áøµ¿ ¹× »ç¶óÁü
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ 3) Èñ±Íµµ ¡æ ½ÇÁ¦ Ä«µå º¯È¯ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void GenerateCardsFromRarities()
    {
        CardPackType type = packViewController.selectedCardPackView.cardPackData.packType;

        foreach (CardRarity r in rarityList)
        {
            Race race = (Race)Random.Range(0, System.Enum.GetValues(typeof(Race)).Length);
            cardList.Add(new CardInfo(r, race, type));
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µå Instantiate ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    IEnumerator SpawnCards()
    {
        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        skipRemaining = false;
        yield return new WaitForSeconds(0.25f);

        // Áß¾Ó ±âÁØ À§Ä¡ °è»ê
        int cols = 5;
        float cellW = 250f, cellH = 350f;
        float spacingX = 50f, spacingY = 50f;

        int rows = Mathf.CeilToInt(cardList.Count / (float)cols);

        // ÀüÃ¼ ³Êºñ, ³ôÀÌ
        float totalWidth = cols * cellW + (cols - 1) * spacingX;
        float totalHeight = rows * cellH + (rows - 1) * spacingY;

        // Áß¾Ó ±âÁØ ÁÂ»ó´Ü ±âÁØÁ¡ °è»ê
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
                // DOTween ¾øÀÌ Áï½Ã ÀÌµ¿
                rt.anchoredPosition = target;
                rt.localScale = Vector3.one;
                continue;
            }

            // Ä«µå 1Àå ¾Ö´Ï¸ÞÀÌ¼Ç (ÀÌµ¿ + ½ºÄÉÀÏ)
            yield return AnimateCardToGrid(rt, target);

            // Å¬¸¯ °¨Áö ÈÄ ÀÌÈÄ Ä«µåºÎÅÍ Áï½Ã ¹èÄ¡
            if (Input.GetMouseButtonDown(0))
            {
                skipRemaining = true;
            }
        }

        cardOpenBtn.gameObject.SetActive(true);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µå 1Àå ¾Ö´Ï¸ÞÀÌ¼Ç (ÀÌµ¿ + ½ºÄÉÀÏ) ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    IEnumerator AnimateCardToGrid(RectTransform rt, Vector2 target)
    {
        float moveTime = 0.25f;

        Tween move = rt.DOAnchorPos(target, moveTime).SetEase(Ease.OutQuad);
        Tween scale = rt.DOScale(1f, moveTime).SetEase(Ease.OutBack);

        yield return move.WaitForCompletion();
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Å¬¸¯ °¨Áö ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    IEnumerator DetectClickToSkip()
    {
        while (!skipRemaining)
        {
            if (Input.GetMouseButtonDown(0))  // ¸ð¹ÙÀÏÀº ÅÍÄ¡·Î ±³Ã¼ °¡´É
            {
                skipRemaining = true;
                yield break;
            }
            yield return null; // ´ÙÀ½ ÇÁ·¹ÀÓ±îÁö ´ë±â
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ¸ðµç Ä«µå µÚÁý±â ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void CardAllOpen()
    {
        foreach (Transform child in cardSpawnContent)
        {
            var card = child.GetComponent<CardPrefab>();
            if (card != null && !card.isFlipped)
                StartCoroutine(card.Flip());
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µå flip»óÅÂ È®ÀÎ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Close ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void ClosePanel()
    {
        //Ä«µå Á¤º¸ ÀúÀå
        //cardList

        //Á¤º¸ ÃÊ±âÈ­
        rarityList.Clear();
        cardList.Clear();

        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        if (currentPack) Destroy(currentPack);

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
        isOpening = false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ³»ºÎ Ä«µå ±¸Á¶Ã¼ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    struct CardInfo
    {
        public CardRarity rarity;
        public Race race;
        public CardPackType type;
        public CardInfo(CardRarity r, Race ra, CardPackType t)
        { rarity = r; race = ra; type = t; }
    }
}
