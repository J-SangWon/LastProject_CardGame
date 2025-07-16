using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailUI : MonoBehaviour
{
    public static CardDetailUI Instance;

    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text statText;
    public TMP_Text descriptionText;

    public Button craftButton;
    public Button disenchantButton;
    public TextMeshProUGUI craftCostText;
    public TextMeshProUGUI disenchantRewardText;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);

    }

    public void SetCardDetail(BaseCardData card)
    {
        cardImage.sprite = card.artwork;
        nameText.text = card.cardName;
        statText.text = ""; // 몬스터면 공격력/체력 등 표시
        descriptionText.text = card.description;

        if (card is MonsterCardData m)
        {
            statText.text = $"공격력: {m.attack} \n체력: {m.health}";
        }

        craftButton.gameObject.SetActive(card.canCraft);
        disenchantButton.gameObject.SetActive(card.canDisenchant);

        craftCostText.text = $"제작 : {card.craftCost.ToString()}";
        disenchantRewardText.text = $"분해 : {card.disenchantReward.ToString()}";

        // 버튼 리스너 등록
        craftButton.onClick.RemoveAllListeners();
        disenchantButton.onClick.RemoveAllListeners();

        craftButton.onClick.AddListener(() => {
            bool result = CardManager.Instance.TryCraftCard(card.cardId);
            if (result)
            {
                SetCardDetail(card); // 수량/재화 갱신
                DeckMakingUI.Instance?.RefreshCraftPointUI();
                DeckMakingUI.Instance?.RefreshAllCardList();
            }
            else
            {
                // 실패 안내
            }
        });

        disenchantButton.onClick.AddListener(() => {
            bool result = CardManager.Instance.TryDisenchantCard(card.cardId);
            if (result)
            {
                SetCardDetail(card);
                DeckMakingUI.Instance?.RefreshCraftPointUI();
                DeckMakingUI.Instance?.RefreshAllCardList();
                Debug.Log("카드 분해 성공!");
            }
            else
            {
                Debug.Log("카드 분해 실패!");
                // 실패 안내
            }
        });
    }
}