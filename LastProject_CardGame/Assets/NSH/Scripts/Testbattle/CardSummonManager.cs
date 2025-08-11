using UnityEngine;

public class CardSummonManager : MonoBehaviour
{
    public static CardSummonManager Instance;

    private GameObject selectedCard;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SelectCard(GameObject card)
    {
        selectedCard = card;
    }

    public GameObject GetSelectedCard()
    {
        return selectedCard;
    }

    public void DeselectCard()
    {
        selectedCard = null;
    }
}
