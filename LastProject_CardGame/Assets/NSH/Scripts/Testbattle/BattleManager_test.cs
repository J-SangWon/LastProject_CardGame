using UnityEngine;

public class BattleManager_test : MonoBehaviour
{
    public static BattleManager_test Instance;
    [SerializeField] private GameObject uiProjectilePrefab;
    [SerializeField] private RectTransform projectileParent;

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
        SimpleCard cardScript = card.GetComponent<SimpleCard>();
        if (cardScript == null) return;

        if (cardScript.HasAttackedThisTurn())
        {
            Debug.Log("이 카드는 이미 공격했습니다.");
            return;
        }

        attacker = card;
        Debug.Log($"공격자 설정됨: {cardScript.cardData.cardName}");
    }

    public void SetTarget(GameObject card)
    {
        if (attacker == null) return;

        target = card;

        LaunchUIProjectile(); // 실제 전투는 여기서만
    }
    private void LaunchUIProjectile()
    {
        RectTransform attackerRT = attacker.GetComponent<RectTransform>();
        RectTransform targetRT = target.GetComponent<RectTransform>();

        GameObject proj = Instantiate(uiProjectilePrefab, projectileParent);
        RectTransform projRT = proj.GetComponent<RectTransform>();
        projRT.anchoredPosition = attackerRT.anchoredPosition;

        UIProjectile projScript = proj.GetComponent<UIProjectile>();
        projScript.Initialize(targetRT);
    }

    public void ResetBattleState()
    {
        attacker = null;
        target = null;
    }
    public void OnProjectileHit()
    {
        if (attacker == null || target == null) return;

        SimpleCard atkCard = attacker.GetComponent<SimpleCard>();
        SimpleCard tgtCard = target.GetComponent<SimpleCard>();

        if (atkCard == null || tgtCard == null) return;

        Debug.Log($"{atkCard.cardData.cardName} 이(가) {tgtCard.cardData.cardName} 을(를) 공격!");

        tgtCard.ReduceHealth(atkCard.cardData.attack);
        atkCard.ReduceHealth(tgtCard.cardData.attack);

        atkCard.SetAttackedThisTurn(true);

        attacker = null;
        target = null;
    }
}
