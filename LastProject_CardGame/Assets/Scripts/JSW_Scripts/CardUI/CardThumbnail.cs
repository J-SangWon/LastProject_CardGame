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

    public void SetCard(BaseCardData card, int owned, int available)
    {
        cardData = card;
        artworkImage.sprite = card.artwork;
        countText.text = $"{available}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void SetUnavailableVisual()
    {
        var img = GetComponent<Image>();
        if (img != null) img.color = new Color(0.7f, 0.7f, 0.7f, 0.5f); // 회색+반투명
        if (countText != null) countText.color = Color.gray;
    }
}
