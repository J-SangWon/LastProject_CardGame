using Kalkatos.DottedArrow;
using System.Collections;
using UnityEngine;

public class BattleManager_test : MonoBehaviour
{
    public static BattleManager_test Instance;
    [SerializeField] private GameObject uiProjectilePrefab;
    [SerializeField] private RectTransform projectileParent;

    private GameObject attacker;
    private GameObject target;

   
    public Arrow Arrow { get => arrow; set => arrow = value; }
    [Header("Arrow Effects")]
    [SerializeField] private Arrow arrow;
    [SerializeField] private AnimationCurve attackAnimCurve;


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

    #region Attack Effect
    private IEnumerator AttackAnimationCoroutine(GameObject _attacker, GameObject _receiver)
    {
        Vector3 originalUp = _attacker.transform.up;
        Vector3 startPos = _attacker.transform.position;
        yield return MoveTo(_attacker.transform, startPos + Vector3.back, 0.2f);
        yield return new WaitForSeconds(0.1f);
        Vector3 distance = _receiver.transform.position - startPos;
        distance = Vector3.MoveTowards(distance, distance * 0.001f, 1f);
        _attacker.transform.up = distance;
        yield return MoveTo(_attacker.transform, startPos + distance, 0.3f, attackAnimCurve);

        yield return MoveTo(_attacker.transform, startPos, 0.3f);
        _attacker.transform.up = originalUp;
    }

    private IEnumerator MoveTo(Transform transform, Vector3 endPos, float time, AnimationCurve curve = null)
    {
        float startTime = Time.time;
        float elapsed = 0;
        Vector3 startPos = transform.position;
        while (elapsed < time)
        {
            elapsed = Time.time - startTime;
            float t = curve != null ? curve.Evaluate(elapsed / time) : elapsed / time;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;
    }

    public void BeginAttack(GameObject card)
    {
        CancelAttack();
        arrow.SetupAndActivate(card.transform);
        attacker = card;
    }

    public void EndAttack(GameObject target)
    {
        arrow.Deactivate();
        StartCoroutine(AttackAnimationCoroutine(attacker, target));
    }

    public void CancelAttack()
    {
        arrow.Deactivate();
        if (attacker != null)
        {
            attacker = null;
        }
    }
    #endregion
}
