using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject cardPrefab;                      // 카드 프리팹
    public Transform[] enemyMonsterSlots;              // 상단 5개의 슬롯

    public BaseCardData[] enemyDeckData;               // 테스트용 카드 데이터

    void Start()
    {
        // 테스트용: 게임 시작 시 몬스터 자동 소환
        for (int i = 0; i < 5 && i < enemyDeckData.Length; i++)
        {
            SummonEnemyMonster(enemyDeckData[i], i);
        }
    }

    public void SummonEnemyMonster(BaseCardData data, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= enemyMonsterSlots.Length) return;

        GameObject card = Instantiate(cardPrefab, enemyMonsterSlots[slotIndex]);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.Euler(0, 180f, 0); // 뒤집기 (상대 방향)
        card.transform.localScale = Vector3.one;

        var cardUI = card.GetComponent<CardUI>();
        cardUI.SetCard(data);
        cardUI.SetFace(false); // 뒷면 상태
        cardUI.isOnField = true;

        // 상대 카드는 드래그 불가
        if (card.TryGetComponent<CardDragHandler>(out var drag)) drag.enabled = false;

        // TargetableCard는 남겨두되, 플레이어가 타겟팅 못하게 비활성화
        if (card.TryGetComponent<TargetableCard>(out var target))
        {
            target.interactable = false; // 또는 필요한 경우 체력만 보여주기
        }
    }

}
