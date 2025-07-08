using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardThumbnail : MonoBehaviour, IPointerClickHandler
{
    public Image artworkImage;
    public TMP_Text countText;

    public BaseCardData cardData;

    public void SetCard(BaseCardData card, int count)
    {
        cardData = card;
        artworkImage.sprite = card.artwork;
        countText.text = count.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // OnPointerClick, AddCardToDeck 등 행동 관련 코드 제거
        // CardThumbnail은 카드 정보 표시만 담당
    }

    public void SetUnavailableVisual()
    {
        var img = GetComponent<Image>();
        if (img != null) img.color = new Color(0.7f, 0.7f, 0.7f, 0.5f); // 회색+반투명
        if (countText != null) countText.color = Color.gray;
    }
}
