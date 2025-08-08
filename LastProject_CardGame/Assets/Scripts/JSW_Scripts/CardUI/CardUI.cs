using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine.SceneManagement;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("카드 플립 설정")]
    [SerializeField] private bool enableCardFlip = true;

    [Header("카드 데이터 받아두기")]
    public BaseCardData cardData;
    private MonsterCardData monsterCardData;

    public GameObject Back;
    public Image imageBack;
    public GameObject Front;
    public Image imageFront;
    public GameObject Artwork;
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

    public int attack;
    public int health;

    private Outline outline;
    private bool isFront = true;
    public bool isOnField = false;
    public bool IsDead =>
            cardData != null &&
            monsterCardData.IsDead();

    // [추가] 공격 제한 플래그
    public bool hasAttackedThisTurn = false;

    private void Start()
    {
        SetFace(isFront);
        foreach (var image in GetComponentsInChildren<Image>())
        {
            if (image.gameObject.name == "CardBackground" || image.gameObject.name == "Frame")
            {
                image.raycastTarget = false;
            }
        }

        health = textHealth.text.Trim().Length > 0 ? int.Parse(textHealth.text.Trim()) : 0;
        attack = textAttack.text.Trim().Length > 0 ? int.Parse(textAttack.text.Trim()) : 0;

        if (cardData is MonsterCardData)
        {
            monsterCardData = (MonsterCardData)cardData;
        }
    }

    public void ReduceHealth(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;
        textHealth.text = health.ToString();

        if(health == 0)
        {
            HandleDeath();
            DuelZoneManager.Instance.SendToGraveyard(cardData);
        }
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
        if (Attack && cardData is MonsterCardData)
            Attack.SetActive(showFront);
        if (Health && cardData is MonsterCardData)
            Health.SetActive(showFront);
        if (race)
            race.SetActive(showFront);
        if (Rarity)
            Rarity.SetActive(showFront);
    }

    private void Awake()
    {
        outline = GetComponentInChildren<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    public void SetOutline(bool active)
    {
        if (outline != null)
            outline.enabled = active;
    }

    public void SetCard(BaseCardData data)
    {
        cardData = data;
        textCardName.text = data.cardName;
        imageArtwork.sprite = data.artwork;
        textCost.text = data.cost.ToString();
        textDescription.text = data.description;
        SetRarity(data.rarity);

        if (data is MonsterCardData m)
        {
            switch (m.race)
            {
                case Race.wizard: textRace.text = "마법사"; break;
                case Race.Warrior: textRace.text = "전사"; break;
                case Race.Undead: textRace.text = "언데드"; break;
                case Race.Dragon: textRace.text = "드래곤"; break;
                case Race.Fiend: textRace.text = "악마"; break;
                case Race.Fairy: textRace.text = "정령"; break;
                case Race.Fish: textRace.text = "어류"; break;
                case Race.Insect: textRace.text = "곤충"; break;
                case Race.Beast: textRace.text = "야수"; break;
                case Race.Plant: textRace.text = "식물"; break;
                case Race.Machine: textRace.text = "기계"; break;
                case Race.Angel: textRace.text = "천사"; break;
                default: textRace.text = ""; break;
            }
        }
        else if (data is SpellCardData spellData)
        {
            switch (spellData.spellType)
            {
                case SpellType.Normal: textRace.text = "마법"; break;
                case SpellType.Continuous: textRace.text = "지속 마법"; break;
                case SpellType.Field: textRace.text = "필드 마법"; break;
                case SpellType.Ritual: textRace.text = "의식 마법"; break;
            }
        }
        else if (data is TrapCardData trapData)
        {
            textRace.text = "비밀";
        }

        if (data is MonsterCardData monsterData)
        {
            textAttack.text = monsterData.attack.ToString();
            textHealth.text = monsterData.currentHP.ToString();
            Attack.gameObject.SetActive(true);
            Health.gameObject.SetActive(true);
        }
        else
        {
            textAttack.text = "";
            textHealth.text = "";
            Attack.gameObject.SetActive(false);
            Health.gameObject.SetActive(false);
        }
    }

    public void SetRarity(CardRarity rarity)
    {
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

    public void FlipCard(bool showFront)
    {
        transform.DORotate(new Vector3(0, 90, 0), 0.15f)
            .OnComplete(() =>
            {
                SetFace(showFront);
                transform.DORotate(Vector3.zero, 0.15f);
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (enableCardFlip)
        {
            isFront = !isFront;
            FlipCard(isFront);
            return;
        }

        if (!isFront || cardData == null)
            return;

        if (SceneManager.GetActiveScene().name == "InGame")
        {
            ShowDetailPanel(cardData);
        }

        if (!(cardData is MonsterCardData))
            return;
        if (GameManager.Instance.CurrentPhase != GamePhase.BattlePhase)
        {
            Debug.Log("BattlePhase가 아니라 공격할 수 없습니다.");
            return;
        }

        // 공격 제한 체크
        if (BattleManager.Instance.HasAttacker())
        {
            BattleManager.Instance.SetTarget(this.gameObject);
        }
        else
        {
            BattleManager.Instance.SetAttacker(this.gameObject);
        }
    }

    public bool EnableCardFlip
    {
        get { return enableCardFlip; }
        set { enableCardFlip = value; }
    }

    public void ShowDetailPanel(BaseCardData cardData)
    {
        if (InGameCardDetailPanel.Instance != null)
        {
            InGameCardDetailPanel.Instance.ShowCard(cardData);
        }
        else
        {
            Debug.LogWarning("InGameCardDetailPanel 인스턴스가 없습니다.");
        }
    }

    public void UpdateHealth()
    {
        if (cardData is MonsterCardData monsterData && textHealth != null)
        {
            textHealth.text = monsterData.currentHP.ToString();
        }
    }

    public void HandleDeath()
    {
        Debug.Log($"{cardData.cardName} 카드가 사망했습니다.");
        Destroy(gameObject);
    }

    public MonsterCardData MonsterData => cardData as MonsterCardData;

    //  공격 후 호출
    public void MarkAsAttacked()
    {
        hasAttackedThisTurn = true;
        SetAttackVisual(false);
    }

    // 턴 시작 시 호출
    public void ResetAttackFlag()
    {
        hasAttackedThisTurn = false;
        SetAttackVisual(true);
    }

    // 공격 가능/불가 시각 표시
    private void SetAttackVisual(bool canAttack)
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = canAttack ? 1f : 0.5f;
        }
    }
}
