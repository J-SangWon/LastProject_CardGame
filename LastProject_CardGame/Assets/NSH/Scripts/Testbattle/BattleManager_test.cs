using UnityEngine;

public class BattleManager_test : MonoBehaviour
{
    public static BattleManager_test Instance;

    private GameObject attacker;
    private GameObject target;

    private void Awake()
    {
        if (Instance == null) 
            Instance = this;
        else Destroy(gameObject);
    }

    public bool HasAttacker() => attacker != null;

    public void SetAttacker(GameObject card)
    {
        FildMonster cardScript = card.GetComponent<FildMonster>();
        if (cardScript == null) return;

        if (cardScript.HasAttackedThisTurn())
        {
            Debug.Log("이 카드는 이미 공격했습니다.");
            return;
        }

        attacker = card;
        Debug.Log($"공격자 설정됨: {cardScript.monsterCardData.cardName}");
    }

    public void SetTarget(GameObject card)
    {
        if (attacker == null) return;

        target = card;
        ExecuteBattle();
    }

    private void ExecuteBattle()
    {
		FildMonster atkCard = attacker.GetComponent<FildMonster>();
		FildMonster tgtCard = target.GetComponent<FildMonster>();

        if (atkCard == null || tgtCard == null) return;

        Debug.Log($"{atkCard.monsterCardData.cardName} 이(가) {tgtCard.monsterCardData.cardName} 을(를) 공격!");

        tgtCard.TakeDamage(atkCard.Attack);
        atkCard.TakeDamage(tgtCard.Attack);

        atkCard.SetAttackedThisTurn(true);

        attacker = null;
        target = null;
    }

    public void ResetBattleState()
    {
        attacker = null;
        target = null;
    }
}
