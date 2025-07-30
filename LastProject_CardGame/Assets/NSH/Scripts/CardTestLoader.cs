using UnityEngine;

public class CardTestLoader : MonoBehaviour
{
    public string cardIdToLoad = "84789399-d353-4f3b-ad2c-ec4f25cf3158"; // 테스트할 카드 ID
    public CardUI_N cardUIPrefab; // CardUI 프리팹
    public Transform spawnParent; // UI가 배치될 부모 오브젝트

    void Start()
    {
        // 모든 카드 불러오기
        BaseCardData[] allCards = Resources.LoadAll<BaseCardData>("CardData");

        // 해당 ID의 카드 찾기
        foreach (var card in allCards)
        {
            if (card.cardId == cardIdToLoad)
            {
                Debug.Log($"카드 '{card.cardName}' 로드 성공");

                // 카드 UI 인스턴스 생성
                CardUI_N ui = Instantiate(cardUIPrefab, spawnParent);
                ui.SetCard(card);
                return;
            }
        }

        Debug.LogWarning("일치하는 카드 ID를 가진 카드가 없습니다.");
    }
}
