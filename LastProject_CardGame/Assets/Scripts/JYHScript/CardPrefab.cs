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
        CardPackType.text = "카드팩\n"+ type.ToString(); // CardPackType는 enum이므로 ToString()으로 문자열로 변환
        Rarity.text = "등급\n"+rarity.ToString();
        Race.text = "종족\n" + race.ToString();

        GetComponent<Image>().color = GetRarityColor(rarity);
    }

    Color32 GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Normal:
                return new Color32(255,255,255,200);    //흰색
            case CardRarity.Rare:
                return new Color32(100, 255, 255, 200); // 하늘색
            case CardRarity.SuperRare:
                return new Color32(150, 50, 255, 200); // 보라색
            case CardRarity.UltraRare:
                return new Color32(255, 150, 50, 200); // 노란색
            default:
                return new Color32(255, 255, 255, 200); //기본 흰색
        }
    }
}
