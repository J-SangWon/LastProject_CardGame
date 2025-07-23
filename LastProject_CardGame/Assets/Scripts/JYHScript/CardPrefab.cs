using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPrefab : MonoBehaviour, IPointerClickHandler
{
    [Header("UI ���")]
    public Image imageBack;
    public Image imageFront;
    public Image imageArtwork;
    public TMP_Text textCardName;
    public TMP_Text textCost;
    public TMP_Text textDescription;
    public TMP_Text textAttack;
    public TMP_Text textHealth;

    [Header("��͵��� ���� ����Ʈ")]
    public GameObject[] ParticlePrefab;
    private GameObject particleprefab;

    private BaseCardData cardData;
    public bool isFlipped { get; private set; } = false;

    public void Init(BaseCardData data)
    {
        cardData = data;

        textCardName.text = cardData.cardName;
        imageArtwork.sprite = cardData.artwork;
        textCost.text = cardData.cost.ToString();
        textDescription.text = cardData.description;

        // ���� ī���� ���� ���ݷ�/ü�� ǥ��
        if (cardData is MonsterCardData monsterData)
        {
            textAttack.text = monsterData.attack.ToString();
            textHealth.text = monsterData.health.ToString();
            textAttack.gameObject.SetActive(true);
            textHealth.gameObject.SetActive(true);
        }
        else
        {
            textAttack.text = "";
            textHealth.text = "";
            textAttack.gameObject.SetActive(false); // UI���� ����
            textHealth.gameObject.SetActive(false);
        }

        SetFace(isFlipped); // �ʱ⿡�� �޸鸸 ���̵��� ����

        GetRarityEffect(cardData.rarity); // �ʱ�ȭ �� ����Ʈ
    }

    void GetRarityEffect(CardRarity rarity)
    {
        if(rarity == CardRarity.SuperRare)
            particleprefab = Instantiate(ParticlePrefab[0], transform);
        else if(rarity == CardRarity.UltraRare)
            particleprefab = Instantiate(ParticlePrefab[1], transform);
    }

    // ������������������������������������ Flip �ִϸ��̼� ������������������������������������
    public void Flip(bool showFront)
    {
        isFlipped = showFront;

        Destroy(particleprefab);

        // 0.15�� ���� Y�� 90�� ȸ�� �� ��/�޸� ��ü �� �ٽ� 0���� ȸ��
        transform.DORotate(new Vector3(0, 90, 0), 0.15f)
            .OnComplete(() => {
                SetFace(showFront); // ��/�޸� ��ü
                transform.DORotate(Vector3.zero, 0.15f);
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isFlipped)
            return;

        if (!isFlipped)
            Flip(true);
        
    }

    public void SetFace(bool showFront)
    {
        imageBack.gameObject.SetActive(!showFront);
        imageFront.gameObject.SetActive(showFront);
        imageArtwork.gameObject.SetActive(showFront);
        textCardName.gameObject.SetActive(showFront);
        textCost.gameObject.SetActive(showFront);
        textDescription.gameObject.SetActive(showFront);
        textAttack.gameObject.SetActive(showFront);
        textHealth.gameObject.SetActive(showFront);
    }
}
