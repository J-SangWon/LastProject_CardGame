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
    public GameObject cardName;
    public TMP_Text textCardName;
    public GameObject Cost;
    public TMP_Text textCost;
    public GameObject description;
    public TMP_Text textDescription;
    public GameObject Attack;
    public TMP_Text textAttack;
    public GameObject Health;
    public TMP_Text textHealth;
    public GameObject race;
    public TMP_Text textRace;
    public GameObject Rarity;
    public Sprite[] rarityImages;

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
        
        SetRarity(cardData.rarity);
        SetRace(cardData);
        SetFace(isFlipped); // �ʱ⿡�� �޸鸸 ���̵��� ����

        GetRarityEffect(cardData.rarity); // �ʱ�ȭ �� ����Ʈ
    }

    private void SetAttackHealth(BaseCardData data)
    {
        if (data is MonsterCardData monsterData)
        {
            textAttack.text = monsterData.attack.ToString();
            textHealth.text = monsterData.health.ToString();
            race.SetActive(true);
            Attack.SetActive(true);
            Health.SetActive(true);
        }
        else
        {
            textAttack.text = "";
            textHealth.text = "";
            race.SetActive(false);
            Attack.SetActive(false);
            Health.SetActive(false);
        }
    }

    private void SetRace(BaseCardData data)
    {
        if (data is MonsterCardData m)
        {
            switch (m.race)
            {
                case Race.wizard:
                    textRace.text = "������";
                    break;
                case Race.Warrior:
                    textRace.text = "����";
                    break;
                case Race.Undead:
                    textRace.text = "�𵥵�";
                    break;
                case Race.Dragon:
                    textRace.text = "�巡��";
                    break;
                case Race.Fiend:
                    textRace.text = "�Ǹ�";
                    break;
                case Race.Fairy:
                    textRace.text = "����";
                    break;
                case Race.Fish:
                    textRace.text = "���";
                    break;
                case Race.Insect:
                    textRace.text = "����";
                    break;
                case Race.Beast:
                    textRace.text = "�߼�";
                    break;
                case Race.Plant:
                    textRace.text = "�Ĺ�";
                    break;
                case Race.Machine:
                    textRace.text = "���";
                    break;
                case Race.Angel:
                    textRace.text = "õ��";
                    break;
                default:
                    textRace.text = "";
                    break;

            }

        }
    }

    private void SetRarity(CardRarity rarity)
    {
        // Rarity �̹��� ����
        var RImage = Rarity.GetComponentInChildren<Image>();
        if (RImage != null && rarityImages.Length > (int)rarity)
        {
            RImage.sprite = rarityImages[(int)rarity];
        }
        else
        {
            Debug.LogWarning("Rarity image not found or index out of range.");
        }

    }

    void GetRarityEffect(CardRarity rarity)
    {
        if(rarity == CardRarity.Rare)
            particleprefab = Instantiate(ParticlePrefab[0], transform);
        else if (rarity == CardRarity.SuperRare)
            particleprefab = Instantiate(ParticlePrefab[1], transform);
        else if (rarity == CardRarity.UltraRare)
            particleprefab = Instantiate(ParticlePrefab[2], transform);
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
        if (imageBack)
            imageBack.gameObject.SetActive(!showFront);
        if (imageFront)
            imageFront.gameObject.SetActive(showFront);
        if (imageArtwork)
            imageArtwork.gameObject.SetActive(showFront);
        if (textCardName)
            cardName.SetActive(showFront);
        if (textCost)
            Cost.SetActive(showFront);
        if (description)
            description.SetActive(showFront);
        if (Attack)
            Attack.SetActive(showFront);
        if (Health)
            Health.SetActive(showFront);
        if (race)
            race.SetActive(showFront);
        if (Rarity)
            Rarity.SetActive(showFront);

        if (showFront)
            SetAttackHealth(cardData);
    }
}
