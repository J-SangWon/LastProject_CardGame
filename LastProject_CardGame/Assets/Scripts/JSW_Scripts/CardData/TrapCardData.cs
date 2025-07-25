using UnityEngine;

[CreateAssetMenu(menuName = "Card/TrapCard")]
public class TrapCardData : BaseCardData
{
    public TrapType trapType;

    protected override void OnEnable()
    {
        cardType = CardType.Trap;
    }
}