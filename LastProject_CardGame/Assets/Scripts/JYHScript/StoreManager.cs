using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    //카드 관련
    [Header("카드 데이터")]
    public List<BaseCardData> cardList = new List<BaseCardData>();
    [Header("카드 개수 설정")]
    public int cardCount = 10;
    [Header("카드 희귀도 설정 (100% 기준)")]
    [SerializeField] private int normalRate = 60; // 일반 카드 확률
    [SerializeField] private int rareRate = 30; // 희귀 카드 확률
    [SerializeField] private int superRareRate = 9; // 슈퍼 희귀 카드 확률
    [SerializeField] private int ultraRareRate = 1; // 울트라 희귀 카드 확률

    [Header("카드 소환")]
    public CardPackViewController cardPackViewController;
    public GameObject cardPrefab;
    public GameObject cardSpawnPanel;
    public Button cardPanelExit;
    public Transform cardSpawnContent;

    [Header("상점 UI")]
    public Text CoinText;
    public Button BuyCardBtn;

    public int coin;

    void Start()
    {
        coin = 100; // 초기 코인 설정

        cardPackViewController.onDragStart += () => BuyCardBtn.interactable = false; // 카드팩 스냅 시작 시 버튼 비활성화
        cardPackViewController.onSnapEnd += () => BuyCardBtn.interactable = true; // 카드팩 스냅 종료 시 버튼 활성화

        if (BuyCardBtn != null)
            BuyCardBtn.onClick.AddListener(BuyCard);

        if(cardPanelExit != null)
            cardPanelExit.onClick.AddListener(() => cardSpawnPanel.SetActive(false));
    }

    void Update()
    {
        if (CoinText != null)
        {
            CoinText.text = coin.ToString();
        }
    }

    void BuyCard()
    {
        if (coin < 10)
        {
            Debug.Log("코인이 부족합니다.");
            return;
        }

        // 카드 구매 로직
        coin -= 10; // 카드 가격 설정 (예: 10 코인)

       StartCoroutine(CardSpawn());

    }

    IEnumerator CardSpawn()
    {
        foreach (Transform child in cardSpawnContent)
        {
            Destroy(child.gameObject); // 기존 카드 제거
        }

        yield return new WaitForSeconds(0.2f); // 카드 제거 후 잠시 대기

        cardSpawnPanel.SetActive(true); // 카드 생성 패널 활성화

        for (int i = 0; i < cardCount; i++)
        {
            CardRarity rarity = GetRandomRarity();
            Race race = (Race)Random.Range(0, System.Enum.GetValues(typeof(Race)).Length);
            CardPackType selectedType = cardPackViewController.selectedCardPackView.cardPackData.packType;

            GameObject cardObj = Instantiate(cardPrefab, cardSpawnContent);
            CardPrefab cardPrefabComponent = cardObj.GetComponent<CardPrefab>();
            cardPrefabComponent.Initialize(rarity, race, selectedType);

            //List<BaseCardData> rarityCard = cardList.FindAll(card => card.rarity == rarity);

            //if (rarityCard.Count == 0)
            //    return;

            //BaseCardData selectedCard = rarityCard[Random.Range(0, rarityCard.Count)];

            yield return new WaitForSeconds(0.3f);
        }

    }

    CardRarity GetRandomRarity()
    {
        int totalRate = normalRate + rareRate + superRareRate + ultraRareRate;
        if (totalRate != 100)
        {
            Debug.LogError("카드 희귀도 확률의 합이 100이 아닙니다.");
            Debug.LogError("확률을 다시 설정하세요");
            return CardRarity.Normal; // 기본값 반환
        }

        int randomValue = Random.Range(0, 100);
        if (randomValue < normalRate)
        {
            return CardRarity.Normal;
        }
        else if (randomValue < normalRate + rareRate)
        {
            return CardRarity.Rare;
        }
        else if (randomValue < normalRate + rareRate + superRareRate)
        {
            return CardRarity.SuperRare;
        }
        else
        {
            return CardRarity.UltraRare;
        }
    }


}
