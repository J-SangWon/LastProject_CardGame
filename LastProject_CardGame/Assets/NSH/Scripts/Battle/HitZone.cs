using UnityEngine;

public class HitZone : MonoBehaviour
{
    public enum TargetType { Player, Enemy }
    public TargetType targetType;    // 히트존 타입: 플레이어 또는 적

    private void OnMouseDown()
    {
        // BattleManager.Instance가 존재해야 처리
        if (BattleManager.Instance == null) return;

        // 현재 공격자가 존재하면
        if (BattleManager.Instance.HasAttacker())
        {
            var attackerGO = BattleManager.Instance.attacker; // 공격자 가져오기
            var attackerUI = attackerGO.GetComponent<CardUI>();
            if (attackerUI == null) return;

            int damage = attackerUI.attack;

            // 대상에 따라 처리
            if (targetType == TargetType.Player)
            {
                GameManager.Instance.TakeDamageToPlayer(damage);
            }
            else
            {
                GameManager.Instance.TakeDamageToEnemy(damage);
            }

            // 공격 카드 플래그 설정
            attackerUI.MarkAsAttacked();

            // 공격 후 공격자 초기화
            BattleManager.Instance.CancelAttack();
        }
    }
}
