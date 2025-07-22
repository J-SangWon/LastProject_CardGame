using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundSprite : MonoBehaviour
{
    [SerializeField] private Image SelectImage;
    public Sprite bgSpirte { get; private set; }
    public bool isSelect { get; private set; }
    [HideInInspector] public bool isOwned;

    void Start()
    {
        bgSpirte = GetComponent<Image>()?.sprite;
        isSelect = false;
    }

    public void SetIsSelect(bool _isSelect)
    {
        isSelect = _isSelect;
        SelectImage.SetEnabled(isSelect);
    }
}
