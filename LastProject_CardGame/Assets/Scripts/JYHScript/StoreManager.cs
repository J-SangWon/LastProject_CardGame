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

        GameObject packPrefab = packViewController.selectedCardPackView.gameObject;

        currentPack = Instantiate(packPrefab, cardPackDisplayPoint);
        RectTransform packrect = currentPack.GetComponent<RectTransform>();
        packrect.sizeDelta = new Vector2(400, 540); // ī���� ũ�� ���� (����)                                                   
        packrect.anchorMin = new Vector2(0.5f, 0.5f);// ��Ŀ�� Middle Center�� ���� (0.5, 0.5)
        packrect.anchorMax = new Vector2(0.5f, 0.5f);
        currentPack.transform.localScale = Vector3.zero;

        // �ְ� ��͵��� �´� ��ƼŬ(����Ʈ) ǥ��
        ShowEffect(GetHighestRarity());

        // Ȯ�� �ִϸ��̼�
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            currentPack.transform.localScale = Vector3.one * Mathf.SmoothStep(0, 1, t);
            yield return null;
        }

        // Ŭ�� ���
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // 3) ���� ī�� ������ ���� �� ����
        GenerateCardsFromRarities();
        yield return StartCoroutine(SpawnCards());
    }

    // ������������������������������������ ����Ʈ ���� ������������������������������������
    void ShowEffect(CardRarity rarity)
    {
        int idx = (int)rarity;
        if (idx < rarityEffects.Length && rarityEffects[idx])
        {
            particle = Instantiate(rarityEffects[idx], cardPackDisplayPoint);
            particle.transform.position = new Vector2(0, 0);
        }
    }

    CardRarity GetHighestRarity()
    {
        CardRarity hi = CardRarity.Normal;
        foreach (var r in rarityList)
            if ((int)r > (int)hi) hi = r;
        return hi;
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
        if (currentPack) Destroy(currentPack);
        if (particle) Destroy(particle);

        foreach (Transform child in cardSpawnContent) Destroy(child.gameObject);
        yield return new WaitForSeconds(0.25f);

        foreach (CardInfo info in cardList)
        {
            GameObject obj = Instantiate(cardPrefab, cardSpawnContent);
            obj.GetComponent<CardPrefab>().Initialize(info.rarity, info.race, info.type);
            yield return new WaitForSeconds(0.25f);
        }

        cardPanelExit.gameObject.SetActive(true);
    }

    // ������������������������������������ Close ������������������������������������
    void ClosePanel()
    {
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
