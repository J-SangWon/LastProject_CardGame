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

        GameObject packPrefab = packViewController.selectedCardPackView.gameObject;

        currentPack = Instantiate(packPrefab, cardPackDisplayPoint);
        RectTransform packrect = currentPack.GetComponent<RectTransform>();
        packrect.sizeDelta = new Vector2(400, 540); // 카드팩 크기 조정 (예시)                                                   
        packrect.anchorMin = new Vector2(0.5f, 0.5f);// 앵커를 Middle Center로 설정 (0.5, 0.5)
        packrect.anchorMax = new Vector2(0.5f, 0.5f);
        currentPack.transform.localScale = Vector3.zero;

        // 최고 희귀도에 맞는 파티클(이펙트) 표시
        ShowEffect(GetHighestRarity());

        // 확대 애니메이션
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            currentPack.transform.localScale = Vector3.one * Mathf.SmoothStep(0, 1, t);
            yield return null;
        }

        // 클릭 대기
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // 3) 실제 카드 데이터 생성 후 등장
        GenerateCardsFromRarities();
        yield return StartCoroutine(SpawnCards());
    }

    // ────────────────── 이펙트 연출 ──────────────────
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

    // ────────────────── Close ──────────────────
    void ClosePanel()
    {
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
