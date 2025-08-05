using UnityEngine;

/// <summary>
/// 키 입력으로 묘지에 테스트 카드를 추가하는 아주 단순한 스크립트
/// </summary>
public class GraveyardTestManager : MonoBehaviour
{
    public KeyCode addToGraveyardKey = KeyCode.G;
    public BaseCardData testCard;

    void Update()
    {
        if (Input.GetKeyDown(addToGraveyardKey))
        {
            AddTestCardToGraveyard();
        }
    }

    void AddTestCardToGraveyard()
    {
        // 묘지존에 추가
        if (DuelZoneManager.Instance != null && DuelZoneManager.Instance.graveyardZone != null)
        {
            DuelZoneManager.Instance.graveyardZone.SendToGraveyard(testCard);
            Debug.Log("묘지에 테스트 카드 추가됨");
        }
        else
        {
            Debug.LogWarning("DuelZoneManager 또는 GraveyardZone을 찾을 수 없습니다!");
        }
    }
}
