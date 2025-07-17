using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleDeckSelectButtonUI : MonoBehaviour
{
    public Button selectButton;
    public TextMeshProUGUI deckNameText;

    public void SetDeck(DeckData deck)
    {
        deckNameText.text = deck.deckName;
    }
}
