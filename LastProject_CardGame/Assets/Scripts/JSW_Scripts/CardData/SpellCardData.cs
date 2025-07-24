using UnityEngine;

[CreateAssetMenu(menuName = "Card/SpellCard")]
public class SpellCardData : BaseCardData
{
    public SpellType spellType;

    protected override void OnEnable()
    {
        base.OnEnable();
        cardType = CardType.Spell;
    }
}
