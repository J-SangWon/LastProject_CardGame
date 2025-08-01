using UnityEngine;

public class AttackTargetSelectable : MonoBehaviour
{
    public GameManager gameManager;  // GameManager 참조

    // 대상을 클릭했을 때 호출되는 메서드
    void OnMouseDown()
    {
        // GameManager에서 선택된 대상을 공격하는 메서드 호출
        gameManager.OnTargetSelected(gameObject);
    }
}
