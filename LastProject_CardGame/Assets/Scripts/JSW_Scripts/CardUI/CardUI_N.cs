using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI_N : MonoBehaviour
{
    public Image imageArtwork;
    public TMP_Text textCardName;
    public TMP_Text textCost;
    public TMP_Text textDescription;
    public TMP_Text textAttack;
    public TMP_Text textHealth;

    public void SetCard(BaseCardData data)
    {
        if (data == null)
        {
            Debug.LogError("SetCard 호출 시 카드 데이터가 null입니다.");
            return;
        }

        if (data.artwork == null)
        {
            Debug.LogWarning($"카드 {data.cardName}의 artwork가 null입니다.");
        }
        textCardName.text = data.cardName;
        imageArtwork.sprite = data.artwork;
        textCost.text = data.cost.ToString();
        textDescription.text = data.description;

        // 몬스터 카드일 경우에만 공격력/체력 표시
        if (data is MonsterCardData monsterData)
        {
            textAttack.text = monsterData.attack.ToString();
            textHealth.text = monsterData.health.ToString();
            textAttack.gameObject.SetActive(true);
            textHealth.gameObject.SetActive(true);
        }
        else
        {
            textAttack.gameObject.SetActive(false);
            textHealth.gameObject.SetActive(false);
        }
    }
}
