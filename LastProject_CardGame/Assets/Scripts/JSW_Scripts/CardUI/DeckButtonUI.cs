using UnityEngine;
using UnityEngine.UI;

public class DeckButtonUI : MonoBehaviour
{
    public Text deckNameText;
    public Text cardCountText;
    public Button selectButton;
    public Button deleteButton;

    public void SetDeck(DeckData deck)
    {
        deckNameText.text = deck.deckName;
        cardCountText.text = deck.cards.Count + "장";
    }


}
