using UnityEngine;

public class CardUI_TestDataLoad : MonoBehaviour
{
    CardUI cardUI;
    public BaseCardData testCardData;
    void Start()
    {
        cardUI = GetComponent<CardUI>();
        if(cardUI == null)
        {
            Debug.LogError("CardUI component not found on this GameObject.");
            return;
        }

        // Load test data
        //testCardData = GetBaseCardData("acb2d30a-cebf-4a4c-a847-a2fb1468abda");
        LoadTestData();
    }

    public void LoadTestData()
    {
        cardUI.SetCard(testCardData);
        cardUI.imageBack.gameObject.SetActive(false);
    }

    public BaseCardData GetBaseCardData(string cardID)
    {
        return CardManager.Instance.GetCardById(cardID);
    }
}
