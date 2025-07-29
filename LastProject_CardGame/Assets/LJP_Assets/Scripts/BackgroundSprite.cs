using UnityEngine;
using UnityEngine.UI;

public class BackgroundSprite : MonoBehaviour
{
    private BackgroundEditController backgroundController;

    public GameObject SelectImage;
    public GameObject nonOwneImage;

    public Sprite bgSpirte { get; private set; }

    [HideInInspector] public bool isSelect;
    public bool isOwned;

    private void Awake()
	{
		IntiComponent();
		isSelect = false;

		if (!isOwned) nonOwneImage.SetActive(true);
	}

	private void OnEnable()
	{
        backgroundController.BackgroundSpriteAllUnSelect();		
	}

	private void IntiComponent()
	{
		backgroundController = GetComponentInParent<BackgroundEditController>();
		bgSpirte = GetComponent<Image>()?.sprite;
	}

	public void SetIsSelect()
    {
        if (!isOwned) return;

        backgroundController.BackgroundSpriteAllUnSelect();
        isSelect = !isSelect;
        SelectImage.SetActive(isSelect);
        backgroundController.backgroundSprite = bgSpirte;
        UIManager.Instance.Background.sprite = bgSpirte;
    }
}
