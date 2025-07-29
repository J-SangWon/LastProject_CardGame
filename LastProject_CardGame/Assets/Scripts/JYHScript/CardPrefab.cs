using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPrefab : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 요소")]
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

    [Header("희귀도에 따른 이펙트")]
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
        SetFace(isFlipped); // 초기에는 뒷면만 보이도록 설정

        GetRarityEffect(cardData.rarity); // 초기화 시 이펙트
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
                    textRace.text = "마법사";
                    break;
                case Race.Warrior:
                    textRace.text = "전사";
                    break;
                case Race.Undead:
                    textRace.text = "언데드";
                    break;
                case Race.Dragon:
                    textRace.text = "드래곤";
                    break;
                case Race.Fiend:
                    textRace.text = "악마";
                    break;
                case Race.Fairy:
                    textRace.text = "정령";
                    break;
                case Race.Fish:
                    textRace.text = "어류";
                    break;
                case Race.Insect:
                    textRace.text = "곤충";
                    break;
                case Race.Beast:
                    textRace.text = "야수";
                    break;
                case Race.Plant:
                    textRace.text = "식물";
                    break;
                case Race.Machine:
                    textRace.text = "기계";
                    break;
                case Race.Angel:
                    textRace.text = "천사";
                    break;
                default:
                    textRace.text = "";
                    break;

            }

        }
    }

    private void SetRarity(CardRarity rarity)
    {
        // Rarity 이미지 설정
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

    // ────────────────── Flip 애니메이션 ──────────────────
    public void Flip(bool showFront)
    {
        isFlipped = showFront;

        Destroy(particleprefab);

        // 0.15초 동안 Y축 90도 회전 → 앞/뒷면 교체 → 다시 0도로 회전
        transform.DORotate(new Vector3(0, 90, 0), 0.15f)
            .OnComplete(() => {
                SetFace(showFront); // 앞/뒷면 교체
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
