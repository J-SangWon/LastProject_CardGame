using UnityEngine;
using UnityEngine.UI;

public class CardPackView : MonoBehaviour
{
    public CardPackData cardPackData;

    public Image artworkImage;
    public Text packNameText;

    void Start()
    {
        //if (cardPackData != null)
        //{
        //    artworkImage.sprite = cardPackData.packArtwork;
        //    packNameText.text = cardPackData.packType.ToString();
        //}
    }
}
