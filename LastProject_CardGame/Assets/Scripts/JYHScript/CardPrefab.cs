using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPrefab : MonoBehaviour
{
    public TextMeshProUGUI CardPackType;
    public TextMeshProUGUI Rarity;
    public TextMeshProUGUI Race;

    public void Initialize(CardRarity rarity, Race race, CardPackType type)
    {
        CardPackType.text = "ī����\n"+ type.ToString(); // CardPackType�� enum�̹Ƿ� ToString()���� ���ڿ��� ��ȯ
        Rarity.text = "���\n"+rarity.ToString();
        Race.text = "����\n" + race.ToString();

        GetComponent<Image>().color = GetRarityColor(rarity);
    }

    Color32 GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Normal:
                return new Color32(255,255,255,200);    //���
            case CardRarity.Rare:
                return new Color32(100, 255, 255, 200); // �ϴû�
            case CardRarity.SuperRare:
                return new Color32(150, 50, 255, 200); // �����
            case CardRarity.UltraRare:
                return new Color32(255, 150, 50, 200); // �����
            default:
                return new Color32(255, 255, 255, 200); //�⺻ ���
        }
    }
}
