using System.Collections.Generic;
using UnityEngine;

public class MonsterZone : MonoBehaviour
{
    // 몬스터카드를 저장할 리스트
    private List<BaseCardData> monsterCards = new List<BaseCardData>();

    // 카드 프리팹 (UI로 표시될 카드)
    public GameObject cardPrefab;

    // 몬스터존에 카드를 추가하는 메서드
    public void SendToMonsterZone(BaseCardData card)
    {
        // 카드 리스트에 추가
        monsterCards.Add(card);

        // 카드 클론을 몬스터존 자식으로 생성
        GameObject cardObject = Instantiate(cardPrefab, transform); // 몬스터존 오브젝트의 transform을 부모로 설정
        cardObject.GetComponent<CardUI>().SetCard(card); // SetCard는 카드 정보를 UI로 설정하는 메서드

        // 카드 위치를 몬스터존에 맞게 설정
        UpdateVisual(cardObject);

        // 디버그 로그로 카드가 추가되었음을 확인
        Debug.Log("몬스터존에 카드가 추가됨: " + card.cardName);
    }

    // 카드가 몬스터존에 추가될 때 위치와 회전을 업데이트하는 메서드
    void UpdateVisual(GameObject cardObject)
    {
        if (cardObject != null)
        {
            // 몬스터존의 위치를 가져옵니다
            Vector3 monsterZonePosition = transform.position;

            // 카드의 위치를 몬스터존의 위치에 맞게 조정합니다
            cardObject.transform.position = new Vector3(monsterZonePosition.x, monsterZonePosition.y, monsterZonePosition.z);

            // 카드가 위아래로 뒤집히도록 x축 기준으로 180도 회전
            cardObject.transform.Rotate(180f, 0f, 0f); // x축을 기준으로 180도 회전
        }
    }

    void Start()
    {
        // 카드 프리팹 자동 할당 (PlayerCardManager에서 가져옴)
        cardPrefab = PlayerCardManager.Instance.cardPrefab;
    }
}
