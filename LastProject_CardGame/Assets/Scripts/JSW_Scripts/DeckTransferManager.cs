using UnityEngine;

public class DeckTransferManager : MonoBehaviour
{
    public static DeckTransferManager Instance { get; private set; }

    // 씬 간 전달할 덱 데이터
    public DeckData selectedDeck;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 덱 할당 함수
    public void SetDeck(DeckData deck)
    {
        selectedDeck = deck;
    }

    // 덱 가져오기 함수
    public DeckData GetDeck()
    {
        return selectedDeck;
    }
} 