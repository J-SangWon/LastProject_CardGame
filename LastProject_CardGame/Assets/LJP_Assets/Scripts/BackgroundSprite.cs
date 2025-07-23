using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundSprite : MonoBehaviour
{
    private BackgroundEditController backgroundController;

    public GameObject SelectImage;
    public Sprite bgSpirte { get; private set; }

    [HideInInspector] public bool isSelect;
    public bool isOwned;

    void Start()
    {
        backgroundController = GetComponentInParent<BackgroundEditController>();
        bgSpirte = GetComponent<Image>()?.sprite;
        isSelect = false;
    }

    public void SetIsSelect()
    {
        if (!isOwned) return;

        backgroundController.BackgroundSpriteAllUnSelect();
        isSelect = !isSelect;
        SelectImage.SetActive(isSelect);
    }
}
