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
    public TMP_Text textCardName;
    public TMP_Text textCost;
    public TMP_Text textDescription;
    public TMP_Text textAttack;
    public TMP_Text textHealth;

    [Header("희귀도에 따른 이펙트")]
    public GameObject[] ParticlePrefab;
    private GameObject particleprefab;

    private BaseCardData cardData;
    public bool isFlipped { get; private set; } = false;

    void Awake()
    {
        GetRarityColor(cardData.rarity); // 초기화 시 색상 설정
    }


    public void Init(BaseCardData data)
    {
        textCardName.text = data.cardName;
        imageArtwork.sprite = data.artwork;
        textCost.text = data.cost.ToString();
        textDescription.text = data.description;

        // 몬스터 카드일 때만 공격력/체력 표시
        if (data is MonsterCardData monsterData)
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
            textAttack.gameObject.SetActive(false); // UI에서 숨김
            textHealth.gameObject.SetActive(false);
        }
    }

    void GetRarityColor(CardRarity rarity)
    {
        if(rarity == CardRarity.SuperRare)
            particleprefab = Instantiate(ParticlePrefab[0], transform);
        else if(rarity == CardRarity.UltraRare)
            particleprefab = Instantiate(ParticlePrefab[1], transform);
    }

    // ────────────────── Flip 애니메이션 ──────────────────
    public void Flip(bool showFront)
    {
        // 0.15초 동안 Y축 90도 회전 → 앞/뒷면 교체 → 다시 0도로 회전
        transform.DORotate(new Vector3(0, 90, 0), 0.15f)
            .OnComplete(() => {
                SetFace(showFront); // 앞/뒷면 교체
                transform.DORotate(Vector3.zero, 0.15f);
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFlipped)
        {
            Destroy(particleprefab);
            Flip(false);
        }
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
