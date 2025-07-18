using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardComponent : MonoBehaviour
{
    private BaseCardData cardData;

    [Header("UI ����")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private Image artworkImage;

    // ī�� ������ ����
    public void SetCardData(BaseCardData data)
    {
        cardData = data;

        if (cardData == null) return;

        nameText.text = cardData.cardName;
        costText.text = cardData.cost.ToString();

        // ���� ī���� ���ݷ�, ü�� ǥ��
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

        // ȿ��(������ ���� �ؽ�Ʈ�� ǥ��)
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
