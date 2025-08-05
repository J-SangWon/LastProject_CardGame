using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundEditController : MonoBehaviour
{
    public List<BackgroundSprite> backgroundSprites;

    [SerializeField] private GameObject LobbyCharacterEdit;
    [SerializeField] private GameObject BackgroundEdit;

    [SerializeField] private Button OKBtn;
    [SerializeField] private Button CancelBtn;

    public Sprite backgroundSprite;
    private Sprite beforeSprite;

    private bool lobbycharacterEditActive = false;
    private bool backgroundEditActive = false;

	private void Awake()
	{
        OKBtn.onClick.AddListener(() => UIManager.Instance.ChangeBackground(backgroundSprite));
        CancelBtn.onClick.AddListener(() => UIManager.Instance.ChangeBackground(beforeSprite));
	}

	private void OnEnable()
	{
		backgroundSprite = UIManager.Instance.Background.sprite;
        beforeSprite = UIManager.Instance.Background.sprite;
	}

	public void BackgroundSpriteAllUnSelect()
    {
        for (int i = 0; i < backgroundSprites.Count; i++)
        {
            backgroundSprites[i].isSelect = false;
            backgroundSprites[i].SelectImage.SetActive(false);
        }
    }

    public void LobbyCharacterBtn()
    {
        lobbycharacterEditActive = !lobbycharacterEditActive;
        LobbyCharacterEdit.SetActive(lobbycharacterEditActive);
        backgroundEditActive = false;
        BackgroundEdit.SetActive(backgroundEditActive);
    }

    public void BackgroundBtn()
    {
		lobbycharacterEditActive = false;
		LobbyCharacterEdit.SetActive(lobbycharacterEditActive);
		backgroundEditActive = !backgroundEditActive;
		BackgroundEdit.SetActive(backgroundEditActive);
	}
}
