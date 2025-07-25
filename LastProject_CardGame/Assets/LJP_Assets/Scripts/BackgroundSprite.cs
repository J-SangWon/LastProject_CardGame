using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundSprite : MonoBehaviour
{
    private BackgroundEditController backgroundController;

    public GameObject SelectImage;
    public GameObject nonOwneImage;

    public Sprite bgSpirte { get; private set; }

    [HideInInspector] public bool isSelect;
    public bool isOwned;

    void Start()
    {
        backgroundController = GetComponentInParent<BackgroundEditController>();
        bgSpirte = GetComponent<Image>()?.sprite;
        isSelect = false;

        if (!isOwned) nonOwneImage.SetActive(true);
    }

    public void SetIsSelect()
    {
        if (!isOwned) return;

        backgroundController.BackgroundSpriteAllUnSelect();
        isSelect = !isSelect;
        SelectImage.SetActive(isSelect);
        backgroundController.backgroundSprite = bgSpirte;
    }
}
