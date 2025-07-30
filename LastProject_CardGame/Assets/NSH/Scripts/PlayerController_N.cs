using System.Collections.Generic;
using UnityEngine;

public class PlayerController_N : MonoBehaviour
{
    private List<GameObject> handCards = new List<GameObject>();

    // 플레이어가 보유한 카드 목록
    public bool HasCards => handCards.Count > 0;

    // 파괴할 카드를 찾는 함수 (예시)
    public GameObject GetCardToDestroy()
    {
        if (handCards.Count > 0)
        {
            return handCards[0]; // 첫 번째 카드 예시로 반환
        }
        return null;
    }

    // 카드를 파괴하는 함수
    public void DestroyCard(GameObject card)
    {
        if (card != null)
        {
            handCards.Remove(card);
            Destroy(card);
            Debug.Log($"카드 {card.name} 파괴");
        }
    }

    // 카드 추가하는 함수 (드로우)
    public void AddCardToHand(GameObject card)
    {
        handCards.Add(card);
    }
}
