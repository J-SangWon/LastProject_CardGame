using System.Collections.Generic;
using UnityEngine;
using static CardEffectManager;

public enum CardType { Monster, Spell, Trap }
public enum MonsterType { Normal, Effect, Ritual, Fusion, Synchro, XYZ, Link }
public enum SpellType { Normal, Continuous, QuickPlay, Ritual, Field, Equip }
public enum TrapType { Normal, Continuous, Counter }
public enum Race { Null, Undead, Dragon, Warrior, wizard, Fiend, Fairy, Fish, Insect, Beast, Plant, Machine, Angel }
public enum CardRarity { Normal, Rare, SuperRare, UltraRare }
public enum AbilityType { TakeDamage, TakeDamageAll, Heal, HealAll, Destroy, Draw, Serch, Create, Buff, Appeared, CloneSummon, Stun }

public enum OwnerType { Player, Opponent }
public static class CardCraftConfig
{
    public static readonly Dictionary<CardRarity, int> CraftCostByRarity = new Dictionary<CardRarity, int>
    {
        { CardRarity.Normal, 10 },
        { CardRarity.Rare, 30 },
        { CardRarity.SuperRare, 100 },
        { CardRarity.UltraRare, 400 }
    };

    // 분해(회수) 비용은 제작 비용의 1/4로 자동 계산
    public static int GetDisenchantReward(CardRarity rarity)
    {
        if (CraftCostByRarity.TryGetValue(rarity, out int craftCost))
            return Mathf.RoundToInt(craftCost * 0.25f);
        return 0;
    }
}

public abstract class BaseCardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardName;
    [TextArea(2, 5)] 
    public string description;
    public Sprite artwork;
    public CardType cardType;
    public CardRarity rarity;
    public int cost;
    
    [Header("고유 ID")]
    public string cardId;
    
    [Header("Live2D 설정")]
    public bool haveLive2D;
    public string live2DPath;

    [Header("카드 효과")]
    public CardAbility cardAbility;
    public int abilityValue;

    [Header("기타 정보")]
    public List<string> tags = new List<string>();
    public OwnerType ownerType = OwnerType.Player;

    public int craftCost = 0;
    public int disenchantReward = 0;
    public bool canCraft = true;
    public bool canDisenchant = true;

    public int maxHP; // maxHP는 각 카드에서 정의된 값을 사용 (기본 설정은 자식 클래스에서)
    public int currentHP;
    protected virtual void OnEnable()
    {
        currentHP = maxHP;
    }
    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;  // 체력이 0 이하로 떨어지면 0으로 처리
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(cardId))
        {
            cardId = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        if (CardCraftConfig.CraftCostByRarity.ContainsKey(rarity))
            craftCost = CardCraftConfig.CraftCostByRarity[rarity];
        disenchantReward = CardCraftConfig.GetDisenchantReward(rarity);
    }
#endif


}

