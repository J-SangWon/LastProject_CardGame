using UnityEngine;

public class HitZone : MonoBehaviour
{
    public enum TargetType { Player, Enemy }
    public TargetType targetType;    // 히트존 타입: 플레이어 또는 적

    private void OnMouseDown()
    {
        if (BattleManager.Instance == null) return;

        // 공격자가 존재할 때만
        if (BattleManager.Instance.HasAttacker())
        {
            var attackerGO = BattleManager.Instance.attacker;
            var attackerUI = attackerGO.GetComponent<CardUI>();
            if (attackerUI == null) return;

            int damage = attackerUI.attack;

            // 대상에 따라 체력 감소
            if (targetType == TargetType.Player)
            {
                GameManager.Instance.TakeDamageToPlayer(damage);
            }
            else if (targetType == TargetType.Enemy)
            {
                GameManager.Instance.TakeDamageToEnemy(damage);
            }

            // 공격자 상태 업데이트
            attackerUI.MarkAsAttacked();

            // 공격 후 초기화
            BattleManager.Instance.CancelAttack();
        }
    }
}
