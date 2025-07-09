using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    //ī�� ����
    [Header("ī�� ������")]
    public List<BaseCardData> cardList = new List<BaseCardData>();
    [Header("ī�� ���� ����")]
    public int cardCount = 10;
    [Header("ī�� ��͵� ���� (100% ����)")]
    [SerializeField] private int normalRate = 60; // �Ϲ� ī�� Ȯ��
    [SerializeField] private int rareRate = 30; // ��� ī�� Ȯ��
    [SerializeField] private int superRareRate = 9; // ���� ��� ī�� Ȯ��
    [SerializeField] private int ultraRareRate = 1; // ��Ʈ�� ��� ī�� Ȯ��

    [Header("ī�� ��ȯ")]
    public CardPackViewController cardPackViewController;
    public GameObject cardPrefab;
    public GameObject cardSpawnPanel;
    public Button cardPanelExit;
    public Transform cardSpawnContent;

    [Header("���� UI")]
    public Text CoinText;
    public Button BuyCardBtn;

    public int coin;

    void Start()
    {
        coin = 100; // �ʱ� ���� ����

        cardPackViewController.onDragStart += () => BuyCardBtn.interactable = false; // ī���� ���� ���� �� ��ư ��Ȱ��ȭ
        cardPackViewController.onSnapEnd += () => BuyCardBtn.interactable = true; // ī���� ���� ���� �� ��ư Ȱ��ȭ

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
            Debug.Log("������ �����մϴ�.");
            return;
        }

        // ī�� ���� ����
        coin -= 10; // ī�� ���� ���� (��: 10 ����)

       StartCoroutine(CardSpawn());

    }

    IEnumerator CardSpawn()
    {
        foreach (Transform child in cardSpawnContent)
        {
            Destroy(child.gameObject); // ���� ī�� ����
        }

        yield return new WaitForSeconds(0.2f); // ī�� ���� �� ��� ���

        cardSpawnPanel.SetActive(true); // ī�� ���� �г� Ȱ��ȭ

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
            Debug.LogError("ī�� ��͵� Ȯ���� ���� 100�� �ƴմϴ�.");
            Debug.LogError("Ȯ���� �ٽ� �����ϼ���");
            return CardRarity.Normal; // �⺻�� ��ȯ
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
