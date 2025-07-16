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

        // �ؽ�Ʈ ���İ��� ���� 0���� ���� (�޸� ����)
        SetTextAlpha(0f);
    }


    public void Initialize(CardRarity rarity, Race race, CardPackType type)
    {
        CardPackType.text = "ī����\n"+ type.ToString(); // CardPackType�� enum�̹Ƿ� ToString()���� ���ڿ��� ��ȯ
        Rarity.text = "���\n"+rarity.ToString();
        Race.text = "����\n" + race.ToString();

        GetComponent<Image>().color = GetRarityColor(rarity);
    }

    Color32 GetRarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Normal:
                return new Color32(255,255,255,200);    //���
            case CardRarity.Rare:
                return new Color32(100, 255, 255, 200); // �ϴû�
            case CardRarity.SuperRare:
                return new Color32(150, 50, 255, 200); // �����
            case CardRarity.UltraRare:
                return new Color32(255, 150, 50, 200); // �����
            default:
                return new Color32(255, 255, 255, 200); //�⺻ ���
        }
    }

    // ������������������������������������ Flip �ִϸ��̼� ������������������������������������
    public IEnumerator Flip()
    {
        if (isFlipped) yield break;
        isFlipped = true;

        float duration = 0.25f;

        // 1. ����
        yield return rt.DOScaleX(0f, duration * 0.5f).SetEase(Ease.InCubic).WaitForCompletion();

        // 2. �ؽ�Ʈ ���İ� ���̰�
        SetTextAlpha(0f); // ���� 0 �� 1 fade in
        DOTween.To(() => 0f, a => SetTextAlpha(a), 1f, 0.2f).SetEase(Ease.OutSine);

        // 3. ��ħ
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
