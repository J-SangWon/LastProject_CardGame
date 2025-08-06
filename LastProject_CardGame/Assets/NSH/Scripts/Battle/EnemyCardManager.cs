using System.Collections.Generic;
using UnityEngine;

public class EnemyCardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform enemyFieldZone;

    public List<BaseCardData> enemyMonsterList; // ScriptableObject 리스트로 설정해도 좋음

    void Start()
    {
        SpawnEnemyMonsters(3); // 예시로 3장 소환
    }

    public void SpawnEnemyMonsters(int count)
    {
        float spacing = 150f;
        int total = Mathf.Min(count, enemyMonsterList.Count);

        for (int i = 0; i < total; i++)
        {
            var data = enemyMonsterList[i];
            GameObject card = Instantiate(cardPrefab, enemyFieldZone);
            card.transform.localScale = Vector3.one;
            card.transform.localRotation = Quaternion.Euler(180f, 0f, 0); //  뒤집기
            card.transform.localPosition = GetSlotPosition(i, total, spacing);

            var cardUI = card.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetCard(data);
                cardUI.SetFace(false); //  뒷면 보여주기
                cardUI.isOnField = true;
            }

            // 드래그/대상 지정 제거
            if (card.TryGetComponent<CardDragHandler>(out var drag)) drag.enabled = false;
            if (card.TryGetComponent<TargetableCard>(out var target)) Destroy(target);

            // Collider 추가 (자동 감지용, 이미 추가된 경우 생략됨)
            if (card.GetComponent<Collider2D>() == null)
            {
                var collider = card.AddComponent<BoxCollider2D>();
                var rect = card.GetComponent<RectTransform>();
                if (rect != null)
                {
                    collider.offset = rect.rect.center;
                    collider.size = rect.rect.size;
                }
            }
        }
    }

    private Vector3 GetSlotPosition(int index, int total, float spacing)
    {
        float startX = -((total - 1) * spacing) / 2f;
        return new Vector3(startX + index * spacing, 0, 0);
    }
}
