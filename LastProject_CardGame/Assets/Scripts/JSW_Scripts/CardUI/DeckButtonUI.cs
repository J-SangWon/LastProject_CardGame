using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckButtonUI : MonoBehaviour
{
    public TextMeshProUGUI deckNameText;
    public Button selectButton;
    public Button deleteButton;

    public void SetDeck(DeckData deck)
    {
        deckNameText.text = deck.deckName;
    }


}
