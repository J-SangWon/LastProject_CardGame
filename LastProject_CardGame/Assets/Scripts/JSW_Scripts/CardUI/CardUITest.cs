using UnityEngine;

public class CardUITest : MonoBehaviour
{
    public CardUI cardUI; // Inspector에 CardUI 오브젝트 드래그
    public string cardName; // Inspector에 카드 이름 입력

    void Start()
    {
        BaseCardData cardData = CardManager.Instance.GetCardByName(cardName);
        if (cardData != null)
        {
            cardUI.SetCard(cardData);
            cardUI.SetFace(true); // 앞면으로
        }
    }

    void Update()
    {
        
    }
}
