using System.Collections.Generic;
using UnityEngine;

public enum CardType { Monster, Spell, Trap }
public enum MonsterType { Normal, Effect, Ritual, Fusion, Synchro, XYZ, Link }
public enum SpellType { Normal, Continuous, QuickPlay, Ritual, Field, Equip }
public enum TrapType { Normal, Continuous, Counter }
public enum Race { Null, Undead, Dragon, Warrior, wizard, Fiend, Fairy, Fish, Insect, Beast, Plant, Machine, Angel }
public enum CardRarity { Normal, Rare, SuperRare, UltraRare }
public enum EffectTiming { OnUse, OnDeath, Continuous, OnTurnStart, OnTurnEnd }

public abstract class BaseCardData : ScriptableObject
{
    public string cardName;
    [TextArea(2, 5)] 
    public string description;
    public Sprite artwork;
    public CardType cardType;
    public CardRarity rarity;
    public int cost;
    public bool haveLive2D;
    public string live2DPath;
    public List<string> effectIds;
    public List<EffectTiming> effectTimings;
    public List<string> tags;
}
[CreateAssetMenu(menuName = "Card/MonsterCard")]
public class MonsterCardData : BaseCardData
{
    public MonsterType monsterType;
    public int attack;
    public int health;
    public Race race;
}

[CreateAssetMenu(menuName = "Card/SpellCard")]
public class SpellCardData : BaseCardData
{
    public SpellType spellType;
}

[CreateAssetMenu(menuName = "Card/TrapCard")]
public class TrapCardData : BaseCardData
{
    public TrapType trapType;
}
