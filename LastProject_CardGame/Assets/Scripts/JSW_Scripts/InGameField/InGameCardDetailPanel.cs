using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameCardDetailPanel : MonoBehaviour
{
    public static InGameCardDetailPanel Instance;

    [Header("UI 요소")]
    public GameObject panelRoot;
    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI raceText;

    public bool IsOpen => panelRoot.activeSelf;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        
    }

    public void ShowCard(BaseCardData card)
    {
        if (card == null) return;

        cardImage.sprite = card.artwork;
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        costText.text = $"비용: {card.cost}";

        if (card is MonsterCardData monster)
        {
            attackText.text = $"공격력: {monster.attack}";
            healthText.text = $"체력: {monster.health}";
            switch (monster.race)
            {
                case Race.wizard: raceText.text = "마법사"; break;
                case Race.Warrior: raceText.text = "전사"; break;
                case Race.Undead: raceText.text = "언데드"; break;
                case Race.Dragon: raceText.text = "드래곤"; break;
                case Race.Fiend: raceText.text = "악마"; break;
                case Race.Fairy: raceText.text = "정령"; break;
                case Race.Fish: raceText.text = "어류"; break;
                case Race.Insect: raceText.text = "곤충"; break;
                case Race.Beast: raceText.text = "야수"; break;
                case Race.Plant: raceText.text = "식물"; break;
                case Race.Machine: raceText.text = "기계"; break;
                case Race.Angel: raceText.text = "천사"; break;
                default: raceText.text = ""; break;
            }
        }
        else if(card is SpellCardData spell)
        {
            if(spell.spellType == SpellType.Continuous)
            {
                raceText.text = "지속 마법";
            }
            else if (spell.spellType == SpellType.Field)
            {
                raceText.text = "필드 마법";
            }
            else if (spell.spellType == SpellType.Ritual)
            {
                raceText.text = "의식 마법";
            }
            else if (spell.spellType == SpellType.QuickPlay)
            {
                raceText.text = "즉시 발동";
            }
            else if (spell.spellType == SpellType.Equip)
            {
                raceText.text = "장착 마법";
            }
            else
            {
                raceText.text = "일반 마법";
            }

            attackText.text = "";
            healthText.text = "";

        }
        else if (card is TrapCardData trap)
        {
            attackText.text = "";
            healthText.text = "";
            raceText.text = "비밀 함정";
        }
        else
        {
            attackText.text = "";
            healthText.text = "";
            raceText.text = "";
        }


        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
