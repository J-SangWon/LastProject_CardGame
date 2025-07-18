using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardComponent : MonoBehaviour
{
    private BaseCardData cardData;

    [Header("UI 참조")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private Image artworkImage;

    // 카드 데이터 세팅
    public void SetCardData(BaseCardData data)
    {
        cardData = data;

        if (cardData == null) return;

        nameText.text = cardData.cardName;
        costText.text = cardData.cost.ToString();

        // 몬스터 카드라면 공격력, 체력 표시
        if (cardData is MonsterCardData monster)
        {
            attackText.text = monster.attack.ToString();
            healthText.text = monster.health.ToString();
        }
        else
        {
            attackText.text = "-";
            healthText.text = "-";
        }

        // 효과(간단히 설명만 텍스트로 표시)
        if (cardData.cardEffects != null && cardData.cardEffects.Count > 0)
        {
            effectText.text = cardData.cardEffects[0].effectDescription;
        }
        else
        {
            effectText.text = "";
        }

        artworkImage.sprite = cardData.artwork;
    }
}
