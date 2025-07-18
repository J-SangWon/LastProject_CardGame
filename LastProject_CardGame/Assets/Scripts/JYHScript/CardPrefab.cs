using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPrefab : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI CardPackType;
    public TextMeshProUGUI Rarity;
    public TextMeshProUGUI Race;

    private RectTransform rt;
    public bool isFlipped { get; private set; } = false;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        // 텍스트 알파값을 전부 0으로 시작 (뒷면 상태)
        SetTextAlpha(0f);
    }


    public void Initialize(CardRarity rarity, Race race, CardPackType type)
    {
        CardPackType.text = "카드팩\n"+ type.ToString(); // CardPackType는 enum이므로 ToString()으로 문자열로 변환
        Rarity.text = "등급\n"+rarity.ToString();
        Race.text = "종족\n" + race.ToString();

        GetComponent<Image>().color = GetRarityColor(rarity);
    }

    Color32 GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Normal:
                return new Color32(255,255,255,200);    //흰색
            case CardRarity.Rare:
                return new Color32(100, 255, 255, 200); // 하늘색
            case CardRarity.SuperRare:
                return new Color32(150, 50, 255, 200); // 보라색
            case CardRarity.UltraRare:
                return new Color32(255, 150, 50, 200); // 노란색
            default:
                return new Color32(255, 255, 255, 200); //기본 흰색
        }
    }

    // ────────────────── Flip 애니메이션 ──────────────────
    public IEnumerator Flip()
    {
        if (isFlipped) yield break;
        isFlipped = true;

        float duration = 0.25f;

        // 1. 접힘
        yield return rt.DOScaleX(0f, duration * 0.5f).SetEase(Ease.InCubic).WaitForCompletion();

        // 2. 텍스트 알파값 보이게
        SetTextAlpha(0f); // 알파 0 → 1 fade in
        DOTween.To(() => 0f, a => SetTextAlpha(a), 1f, 0.2f).SetEase(Ease.OutSine);

        // 3. 펼침
        yield return rt.DOScaleX(1f, duration * 0.5f).SetEase(Ease.OutBack).WaitForCompletion();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFlipped)
            StartCoroutine(Flip());
    }

    void SetTextAlpha(float alpha)
    {
        Color c1 = CardPackType.color; c1.a = alpha; CardPackType.color = c1;
        Color c2 = Rarity.color; c2.a = alpha; Rarity.color = c2;
        Color c3 = Race.color; c3.a = alpha; Race.color = c3;
    }
}
