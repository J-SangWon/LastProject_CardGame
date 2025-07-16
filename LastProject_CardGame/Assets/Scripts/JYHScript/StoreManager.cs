using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Buy ��ư �� ��͵� ����Ʈ ����
/// �ְ� ��͵��� �´� ����Ʈ ��� + ī���� Ȯ��
/// Ŭ��(Ȥ�� ��ġ) �� ���� ī�� 10�� Instantiate
/// </summary>
public class StoreManager : MonoBehaviour
{
    // ������������������������������������ ������ & Ȯ�� ������������������������������������
    [Header("ī�� ���� / Ȯ��")]
    public int cardCount = 10;

    [Range(0, 100)] public int normalRate = 60;
    [Range(0, 100)] public int rareRate = 30;
    [Range(0, 100)] public int superRareRate = 9;
    [Range(0, 100)] public int ultraRareRate = 1;

    // ������������������������������������ ����� ������Ʈ ������������������������������������
    [Header("���� ������Ʈ")]
    public Transform cardPackDisplayPoint;        // ī���� �߾� ǥ�� ��ġ
    public GameObject[] rarityEffects;            // 0~3 : Normal/Rare/SR/UR ��ƼŬ ��

    // ������������������������������������ ī�� ��ȯ�� ������������������������������������
    [Header("ī�� ��ȯ")]
    public CardPackViewController packViewController;
    public GameObject cardPrefab;                 // 1�� ī�� ������
    public Transform cardSpawnContent;            // 10�� ��ġ �θ�
    public GameObject cardSpawnPanel;             // ���� ��� + ī�� ����
    public Button cardPanelExit;                  // �ݱ� ��ư

    // ������������������������������������ UI ������������������������������������
    [Header("���� UI")]
    public Text coinText;
    public Button buyButton;
    public int coin = 100;

    // ������������������������������������ ���� ���� ������������������������������������
    private bool isOpening = false;
    private GameObject currentPack;
    private GameObject particle;
    private readonly List<CardRarity> rarityList = new();   // ��͵��� ����
    private readonly List<CardInfo> cardList = new();   // Ŭ�� �� ���� ī�� ���� ����

    // ������������������������������������ �ʱ�ȭ ������������������������������������
    void Start()
    {
        coinText.text = coin.ToString();
        buyButton.onClick.AddListener(BuyCard);
        cardPanelExit.onClick.AddListener(ClosePanel);

        // ��ũ�� ���� ���� ���� (�ɼ�)
        packViewController.onDragStart += () => buyButton.interactable = false;
        packViewController.onSnapEnd += () => buyButton.interactable = true;

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
    }

    // ������������������������������������ Buy Ŭ�� ������������������������������������
    void BuyCard()
    {
        if (isOpening || coin < 10) return;

        coin -= 10;
        coinText.text = coin.ToString();
        isOpening = true;

        rarityList.Clear();
        cardList.Clear();

        GenerateRarityList();                    // 1) ��͵��� �̱�
        StartCoroutine(ShowPackAndEffect());     // 2) ����Ʈ & �� ����
    }

    // ������������������������������������ 1) ��͵� �̱� ������������������������������������
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

    // ������������������������������������ 2) ī���� + ����Ʈ ������������������������������������
    IEnumerator ShowPackAndEffect()
    {
        cardSpawnPanel.SetActive(true);

        // ��ũ�� �� ���� ���, ���� ���� �� Prefab�� CardPackData�� �־�δ� ���� ����
        if (currentPack) Destroy(currentPack);
        if (particle) Destroy(particle);

        // ī���� ��Ÿ����
        yield return CardPackAppear();
        // Ŭ�� ���
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        // ���� + �Ʒ��� �����
        yield return AnimatePackVanish();
        // 3) ���� ī�� ������ ���� �� ����
        GenerateCardsFromRarities();
        yield return StartCoroutine(SpawnCards());
    }

    // ������������������������������������ ����Ʈ ���� ������������������������������������
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

    void ShowEffect(CardRarity rarity)  //ī���� ��ƼŬ
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

    //ī���� ���� �� �����
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

    // ������������������������������������ 3) ��͵� �� ���� ī�� ��ȯ ������������������������������������
    void GenerateCardsFromRarities()
    {
        CardPackType type = packViewController.selectedCardPackView.cardPackData.packType;

        foreach (CardRarity r in rarityList)
        {
            Race race = (Race)Random.Range(0, System.Enum.GetValues(typeof(Race)).Length);
            cardList.Add(new CardInfo(r, race, type));
        }
    }

    // ������������������������������������ ī�� Instantiate ������������������������������������
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

            // ��ǥ ��ǥ
            int row = i / cols, col = i % cols;
            Vector2 target = origin + new Vector2(
                col * (cell.x + sp.x),
               -(row * (cell.y + sp.y)));

            float d = 0.06f * i;

            // �ڷ�ƾ �и� ȣ��
            StartCoroutine(AnimateCardToGrid(rt, target, d, cp));

            // Ŭ�� �� ������ ���
            if (!skipDelay)
            {
                float t = 0, wait = .1f;
                while (t < wait)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        skipDelay = true;
                        DOTween.Kill(rt);               // ��� ����
                        rt.anchoredPosition = target;
                        rt.localScale = Vector3.one;
                        StartCoroutine(cp.Flip());      // �ٷ� Flip
                        break;
                    }
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }
        cardPanelExit.gameObject.SetActive(true);
    }

    // ������������������������������������ ī�� 1�� �ִϸ��̼� (�̵� + ������ + Flip) ������������������������������������
    IEnumerator AnimateCardToGrid(RectTransform rt, Vector2 target, float baseDelay, CardPrefab cp)
    {
        // �ö���� + ������
        rt.DOAnchorPos(target, 0.35f)
          .SetEase(Ease.OutQuad)
          .SetDelay(baseDelay);

        rt.DOScale(1f, 0.35f)
          .SetEase(Ease.OutBack)
          .SetDelay(baseDelay);

        // 0.1�� �� Flip
        yield return new WaitForSeconds(baseDelay + 0.1f);
        yield return cp.Flip();   // CardPrefab ���� Flip �ڷ�ƾ
    }

    // ������������������������������������ Close ������������������������������������
    void ClosePanel()
    {
        //ī�� ���� ����


        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        if (currentPack) Destroy(currentPack);

        cardSpawnPanel.SetActive(false);
        cardPanelExit.gameObject.SetActive(false);
        isOpening = false;
    }

    // ������������������������������������ ���� ī�� ����ü ������������������������������������
    struct CardInfo
    {
        public CardRarity rarity;
        public Race race;
        public CardPackType type;
        public CardInfo(CardRarity r, Race ra, CardPackType t)
        { rarity = r; race = ra; type = t; }
    }
}
