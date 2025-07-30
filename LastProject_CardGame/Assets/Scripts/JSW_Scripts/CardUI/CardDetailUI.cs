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
        statText.text = "";
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
                SetCardDetail(card); 
                DeckMakingUI.Instance?.RefreshCraftPointUI();
                DeckMakingUI.Instance?.RefreshAllCardList();
            }
            else
            {
            }
        });

        disenchantButton.onClick.AddListener(() => {
            bool result = CardManager.Instance.TryDisenchantCard(card.cardId);
            if (result)
            {
                SetCardDetail(card);
                DeckMakingUI.Instance?.RefreshCraftPointUI();
                DeckMakingUI.Instance?.RefreshAllCardList();
            }
            else
            {
            }
        });
    }
}