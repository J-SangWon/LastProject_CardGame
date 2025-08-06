using UnityEngine;

public class MonsterZoneTestManager : MonoBehaviour
{
    public KeyCode addToMonsterZoneKey = KeyCode.M; // 몬스터존에 추가하는 키 (기본: M)
    public BaseCardData testCard;  // 추가할 테스트 카드

    void Update()
    {
       
        if (Input.GetKeyDown(addToMonsterZoneKey))
        {
            AddTestCardToMonsterZone();
        }
    }

    void AddTestCardToMonsterZone()
    {
        // 몬스터존에 카드 추가
        if (DuelZoneManager.Instance != null && DuelZoneManager.Instance.monsterZone != null)
        {
            DuelZoneManager.Instance.monsterZone.SendToMonsterZone(testCard);
            Debug.Log("몬스터존에 테스트 카드 추가됨");
        }
        else
        {
            Debug.LogWarning("DuelZoneManager 또는 MonsterZone을 찾을 수 없습니다!");
        }
    }
}
