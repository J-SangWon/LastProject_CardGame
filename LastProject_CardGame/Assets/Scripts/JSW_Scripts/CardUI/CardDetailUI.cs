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

    void Awake()
    {
        Instance = this;
    }

    public void SetCardDetail(BaseCardData cardData)
    {
        cardImage.sprite = cardData.artwork;
        nameText.text = cardData.cardName;
        statText.text = ""; // 몬스터면 공격력/체력 등 표시
        descriptionText.text = cardData.description;

        if (cardData is MonsterCardData m)
        {
            statText.text = $"공격력: {m.attack} \n체력: {m.health}";
        }
    }
}