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
    public GameObject CardPackAnim;
    public Image CardPackUpImg;
    public Image CardPackDownImg;          // Ä«µåÆÑ ¾÷/´Ù¿î ¿ÀºêÁ§Æ®
    public GameObject[] rarityEffects;            // 0~3 : Normal/Rare/SR/UR ÆÄÆ¼Å¬ µî

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µå ¼ÒÈ¯¿ë ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("Ä«µå ¼ÒÈ¯")]
    public CardPackViewController packViewController;
    public GameObject cardPrefab;                 // 1Àå Ä«µå ÇÁ¸®ÆÕ
    public Transform cardSpawnContent;            // 10Àå ¹èÄ¡ ºÎ¸ð
    public GameObject CardSpawnPanel;             // °ËÀº ¹è°æ + Ä«µå ¿µ¿ª
    public GameObject StoreMenu;
    public Button cardOpenBtn;
    public Button cardPanelExit;                  // ´Ý±â ¹öÆ°

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ UI ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("»óÁ¡ UI")]
    public Text coinText;
    public Button buyButton;
    public Button coinPlusButton; // ÄÚÀÎ Ãß°¡ ¹öÆ° (¿É¼Ç, ½ÇÁ¦ °ÔÀÓ¿¡¼­´Â »ç¿ë ¾ÈÇÔ)
    public int coin = 100;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ³»ºÎ »óÅÂ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private bool isOpening = false;
    private GameObject particle;
    private readonly List<CardRarity> rarityList = new();   // Èñ±Íµµ¸¸ ÀúÀå
    private readonly List<BaseCardData> cardList = new();   // Å¬¸¯ ÈÄ ½ÇÁ¦ Ä«µå Á¤º¸ ÀúÀå
    private bool skipRemaining = false; // Å¬¸¯ ½Ã ÀÌÈÄ Ä«µå Áï½Ã ¹èÄ¡ ¿©ºÎ  

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µåÆÑ ¿¬Ãâ ÃÊ±â »óÅÂ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private Vector3 packUpInitPos;
    private Quaternion packUpInitRot;
    private Vector3 packUpInitScale;
    private Vector3 packDownInitPos;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ ÃÊ±âÈ­ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void Start()
    {
        coinText.text = coin.ToString();
        buyButton.onClick.AddListener(BuyCard);
        cardOpenBtn.onClick.AddListener(CardAllOpen);
        cardPanelExit.onClick.AddListener(ClosePanel);
        if(coinPlusButton != null) // ÄÚÀÎ Ãß°¡ ¹öÆ°ÀÌ ÀÖÀ» °æ¿ì
            coinPlusButton.onClick.AddListener(() => { coin += 100; coinText.text = coin.ToString(); }); // ÄÚÀÎ Ãß°¡ (¿É¼Ç)

        // ½ºÅ©·Ñ µµÁß ±¸¸Å ¹æÁö (¿É¼Ç)
        packViewController.onDragStart += () => buyButton.interactable = false;
        packViewController.onDragStart += () => SoundManager.Instance.PlaySFX("PACKSLIDE"); // ½ºÅ©·Ñ ½ÃÀÛ ½Ã Å¬¸¯ »ç¿îµå
        packViewController.onSnapEnd += () => buyButton.interactable = true;

        CardSpawnPanel.SetActive(false);
        cardOpenBtn.gameObject.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);

        CardPackInit();
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

    private void CardPackInit()
    {
        // transform ÃÊ±â »óÅÂ ÀúÀå
        packUpInitPos = CardPackUpImg.rectTransform.localPosition;
        packUpInitRot = CardPackUpImg.rectTransform.localRotation;
        packUpInitScale = CardPackUpImg.rectTransform.localScale;
        packDownInitPos = CardPackDownImg.rectTransform.localPosition;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Buy Å¬¸¯ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void BuyCard()
    {
        if (isOpening || coin < 10)
        {
            SoundManager.Instance.PlaySFX("MENUSELECT_ERROR"); // ¿À·ù »ç¿îµå
            return;
        }

        SoundManager.Instance.StopBGM(); // BGM Á¤Áö
        SoundManager.Instance.PlaySFX("BUYCARD"); // ±¸¸Å »ç¿îµå

        coin -= 10;
        coinText.text = coin.ToString();
        isOpening = true;

        GenerateRarityList();                    // 1) Èñ±Íµµ¸¸ »Ì±â
        StartCoroutine(ShowPackAndEffect());     // 2) ÀÌÆåÆ® & ÆÑ µîÀå
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ 1) Èñ±Íµµ »Ì±â ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void GenerateRarityList()
    {
        rarityList.Clear();
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
        StoreMenu.SetActive(false);
        CardSpawnPanel.SetActive(true);

        // ½ºÅ©·Ñ ¼¿ º¹Á¦ ´ë½Å, ¿¬Ãâ Àü¿ë ¸ðµ¨ PrefabÀ» CardPackData¿¡ ³Ö¾îµÎ´Â ÆíÀÌ ÁÁÀ½
        CardPackAnim.SetActive(false); // ¿¬Ãâ¿ë Ä«µåÆÑ ºñÈ°¼ºÈ­

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
        CardPackAnim.SetActive(true); // ¿¬Ãâ¿ë Ä«µåÆÑ È°¼ºÈ­
        CardPackAnim.transform.localScale = Vector3.zero; // ÃÊ±â ½ºÄÉÀÏ 0

        CardPackUpImg.gameObject.SetActive(true); // Ä«µåÆÑ ¾÷ ¿ÀºêÁ§Æ® È°¼ºÈ­
        CardPackDownImg.gameObject.SetActive(true); // Ä«µåÆÑ ´Ù¿î ¿ÀºêÁ§Æ® È°¼ºÈ­

        CardPackUpImg.sprite = packViewController.selectedCardPackView.cardPackData.packUpImg;
        CardPackDownImg.sprite = packViewController.selectedCardPackView.cardPackData.packDownImg;

        ShowEffect(GetHighestRarity());

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            CardPackAnim.transform.localScale = Vector3.one * Mathf.SmoothStep(0, 1, t);
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
        particle?.gameObject.SetActive(false); // ÆÄÆ¼Å¬ ºñÈ°¼ºÈ­

        //Áøµ¿ ½Ã°£ ¹× ¼¼±â
        float shakeDuration = 0f;
        float shakeStrength = 0f;

        //SOUND
        if (GetHighestRarity() == CardRarity.UltraRare)
        {
            SoundManager.Instance.PlaySFX("PACKOPEN_UR");
            shakeDuration = 2f;
            shakeStrength = 0.7f; // ¿ïÆ®¶ó ·¹¾î´Â ´õ °­ÇÑ Áøµ¿
        }
        else if (GetHighestRarity() == CardRarity.SuperRare)
        {
            SoundManager.Instance.PlaySFX("PACKOPEN_SR");
            shakeDuration = 1f;
            shakeStrength = 0.4f;

        }
        else
        {
            SoundManager.Instance.PlaySFX("PACKOPEN");
            shakeDuration = 0.5f;
            shakeStrength = 0.2f; // ÀÏ¹Ý/·¹¾î´Â ¾àÇÑ Áøµ¿
        }

        RectTransform topRect = CardPackUpImg.rectTransform;
        RectTransform downRect = CardPackDownImg.rectTransform;

        // Á¾ÀÌ ÂõµíÀÌ È¸ÀüÇÏ¸é¼­ ¿Ã¶ó°¡±â
        Sequence seq = DOTween.Sequence();

        // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µåÆÑ ¾Ö´Ï¸ÞÀÌ¼Ç ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
        // 1. Ä«µå ÆÑÀÇ Áøµ¿À» ÁÜ
        seq.Join(CardPackAnim.transform.DOShakeScale(shakeDuration, shakeStrength));

        seq.AppendInterval(0.2f);

        // 1. »ó´Ü Âõ¾îÁü: È¸Àü + ÀÌµ¿
        seq.Join(topRect.DOLocalRotate(new Vector3(0, 0, -25f), 0.4f).SetEase(Ease.InOutSine));
        seq.Join(topRect.DOLocalMove(new Vector3(150f, 600f, 0f), 0.6f).SetEase(Ease.InBack));

        // 2. Àá±ñ ÅÒÀ» µÎ°í
        seq.AppendInterval(0.2f);

        // 3. ÇÏ´Ü ³»·Á°¡±â
        seq.Append(downRect.DOLocalMoveY(-620f, 0.6f).SetEase(Ease.InBack));

        // 4. ºñÈ°¼ºÈ­
        seq.OnComplete(() => {
            CardPackUpImg.gameObject.SetActive(false);
        });

        yield return seq.WaitForCompletion();
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ 3) Èñ±Íµµ ¡æ ½ÇÁ¦ Ä«µå º¯È¯ ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void GenerateCardsFromRarities()
    {
        cardList.Clear();
        var packData = packViewController.selectedCardPackView.cardPackData;

        foreach (CardRarity r in rarityList)
        {
            var carddatas = packData.cards.FindAll(BaseCardData => BaseCardData.rarity == r);

            if(carddatas.Count > 0)
            {
                // ·£´ýÀ¸·Î Ä«µå ¼±ÅÃ
                BaseCardData selectedCard = carddatas[Random.Range(0, carddatas.Count)];
                cardList.Add(selectedCard);
            }
            else
            {
                Debug.LogWarning($"No cards found for rarity: {r}");
            }

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
        float spacingX = 80f, spacingY = 100f;

        int rows = Mathf.CeilToInt(cardList.Count / (float)cols);

        // ÀüÃ¼ ³Êºñ, ³ôÀÌ
        float totalWidth = cols * cellW + (cols - 1) * spacingX;
        float totalHeight = rows * cellH + (rows - 1) * spacingY;

        // Áß¾Ó ±âÁØ ÁÂ»ó´Ü ±âÁØÁ¡ °è»ê
        Vector2 startPos = new Vector2(
            -totalWidth / 2f + cellW / 2f,
             totalHeight / 2f - cellH / 2f + 50
        );

        //Å¬¸¯°¨Áö Ä«µå ¼ÒÈ¯ ½ºÅµ
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

            SoundManager.Instance.PlaySFX("CARDSPAWN");

            if (skipRemaining)
            {
                // DOTween ¾øÀÌ Áï½Ã ÀÌµ¿
                rt.anchoredPosition = target;
                rt.localScale = Vector3.one * 1.3f;
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

        //Ä«µå ¼ÒÈ¯ ´Ù µÇ¾úÀ»¶§
        CardPackDownImg.gameObject.SetActive(false); // Ä«µåÆÑ ´Ù¿î ¿ÀºêÁ§Æ® È°¼ºÈ­
        cardOpenBtn.gameObject.SetActive(true);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡ Ä«µå 1Àå ¾Ö´Ï¸ÞÀÌ¼Ç (ÀÌµ¿ + ½ºÄÉÀÏ) ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    IEnumerator AnimateCardToGrid(RectTransform rt, Vector2 target)
    {
        float moveTime = 0.25f;

        Tween move = rt.DOAnchorPos(target, moveTime).SetEase(Ease.OutQuad);
        Tween scale = rt.DOScale(1.3f, moveTime).SetEase(Ease.OutBack);

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
        SoundManager.Instance.PlaySFX("MENUSELECT_01"); // ´Ý±â »ç¿îµå
        foreach (Transform child in cardSpawnContent)
        {
            var card = child.GetComponent<CardPrefab>();
            if (card != null && !card.isFlipped)
                card.Flip(true);
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
        foreach(BaseCardData card in cardList)
        {
            PlayerCardCollectionManager.Instance.AddCard(card.cardId);
        }
        //cardList

        SoundManager.Instance.PlaySFX("MENUSELECT_01"); // ´Ý±â »ç¿îµå
        //SoundManager.Instance.PlayBGM("MAIN_BGM"); // BGM Àç»ý

        //Á¤º¸ ÃÊ±âÈ­
        rarityList.Clear();
        cardList.Clear();

        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);

        CardPackAnim.SetActive(false);
        ResetCardPackTransforms();

        StoreMenu.SetActive(true);
        CardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
        isOpening = false;
    }

    void ResetCardPackTransforms()
    {
        RectTransform top = CardPackUpImg.rectTransform;
        RectTransform down = CardPackDownImg.rectTransform;

        top.localPosition = packUpInitPos;
        top.localRotation = packUpInitRot;
        top.localScale = packUpInitScale;
        down.localPosition = packDownInitPos;
    }

}
