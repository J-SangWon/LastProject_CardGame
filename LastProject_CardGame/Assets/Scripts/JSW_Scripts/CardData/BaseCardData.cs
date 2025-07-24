using System.Collections.Generic;
using UnityEngine;

public enum CardType { Monster, Spell, Trap }
public enum MonsterType { Normal, Effect, Ritual, Fusion, Synchro, XYZ, Link }
public enum SpellType { Normal, Continuous, QuickPlay, Ritual, Field, Equip }
public enum TrapType { Normal, Continuous, Counter }
public enum Race { Null, Undead, Dragon, Warrior, wizard, Fiend, Fairy, Fish, Insect, Beast, Plant, Machine, Angel }
public enum CardRarity { Normal, Rare, SuperRare, UltraRare }

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
    public string cardId; // 카드 고유 ID (직접 입력 또는 자동 생성)
    
    [Header("Live2D 설정")]
    public bool haveLive2D;
    public string live2DPath;
    
    [Header("효과 시스템")]
    public List<CardEffectData> cardEffects; // ScriptableObject 참조 리스트
    
    [Header("기타 정보")]
    public List<string> tags = new List<string>();
    
    // 기존 호환성을 위한 속성들
    public List<string> effectIds => new List<string>();
    public List<string> effectTimings => new List<string>();

    public int craftCost = 0;
    public int disenchantReward = 0;
    public bool canCraft = true;
    public bool canDisenchant = true;
    
    // 효과 관련 메서드들
    public void AddEffect(CardEffectData effect)
    {
        if (effect != null && !cardEffects.Contains(effect))
        {
            cardEffects.Add(effect);
        }
    }
    
    public void RemoveEffect(CardEffectData effect)
    {
        if (effect != null)
        {
            cardEffects.Remove(effect);
        }
    }
    
    public List<CardEffect> GetEffects()
    {
        var effects = new List<CardEffect>();
        foreach (var effectData in cardEffects)
        {
            if (effectData != null)
            {
                effects.Add((CardEffect)effectData);
            }
        }
        return effects;
    }
    
    public List<CardEffect> GetEffectsByTrigger(EffectTrigger trigger)
    {
        var effects = new List<CardEffect>();
        foreach (var effectData in cardEffects)
        {
            if (effectData != null && effectData.trigger == trigger)
            {
                effects.Add((CardEffect)effectData);
            }
        }
        return effects;
    }
    
    public List<CardEffect> GetEffectsByType(EffectType effectType)
    {
        var effects = new List<CardEffect>();
        foreach (var effectData in cardEffects)
        {
            if (effectData != null && effectData.effectType == effectType)
            {
                effects.Add((CardEffect)effectData);
            }
        }
        return effects;
    }
    
    // 카드 효과 등록
    public void RegisterEffects()
    {
        if (!string.IsNullOrEmpty(cardName) && cardEffects.Count > 0)
        {
            CardEffectManager.Instance.RegisterCardEffects(cardName, GetEffects());
        }
    }

    protected virtual void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
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

[CreateAssetMenu(menuName = "CardGame/CardEffectData")]
public class CardEffectData : ScriptableObject
{
    [Header("효과 기본 정보")]
    public string effectName;
    [TextArea(3, 6)]
    public string effectDescription;
    public EffectTrigger trigger;
    public EffectType effectType;
    
    [Header("효과 조건")]
    public EffectCondition condition;
    public bool hasCondition => condition != null && condition.conditionType != EffectCondition.ConditionType.None;
    
    [Header("효과 실행")]
    public string effectMethodName; // 실행할 메서드 이름
    public List<string> effectParameters; // 효과 파라미터
    
    [Header("효과 설정")]
    public bool isOncePerTurn = false; // 턴당 한 번만
    public bool isOncePerGame = false; // 게임당 한 번만
    public int activationCount = 0; // 발동 횟수 추적
    
    // CardEffect와의 호환성을 위한 암시적 변환
    public static implicit operator CardEffect(CardEffectData effectData)
    {
        if (effectData == null) return null;
        
        return new CardEffect
        {
            effectName = effectData.effectName,
            effectDescription = effectData.effectDescription,
            trigger = effectData.trigger,
            effectType = effectData.effectType,
            condition = effectData.condition,
            effectMethodName = effectData.effectMethodName,
            effectParameters = effectData.effectParameters != null ? new List<string>(effectData.effectParameters) : new List<string>(),
            isOncePerTurn = effectData.isOncePerTurn,
            isOncePerGame = effectData.isOncePerGame,
            activationCount = effectData.activationCount
        };
    }
}
