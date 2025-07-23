using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundEditController : MonoBehaviour
{
    [SerializeField] private List<BackgroundSprite> backgroundSprites;
    [SerializeField] private Button OKBtn;
    [SerializeField] private Button CancelBtn;

    public void BackgroundSpriteAllUnSelect()
    {
        for (int i = 0; i < backgroundSprites.Count; i++)
        {
            backgroundSprites[i].isSelect = false;
            backgroundSprites[i].SelectImage.SetActive(false);
        }
    }
}
