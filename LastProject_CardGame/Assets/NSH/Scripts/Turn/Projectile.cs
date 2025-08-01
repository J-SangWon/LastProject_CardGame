using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;  // 발사체 속도
    public int damage = 0;     // 발사체가 주는 데미지, 하지만 실제로는 데미지 처리 안 함

    private Transform target;  // 목표 대상 (대상 카드)

    // 발사체가 타겟을 향해 날아가도록 설정
    public void Initialize(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    void Update()
    {
        if (target != null)
        {
            // 목표 지점으로 발사체 이동
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            // 목표 지점에 도달하면 타겟에 데미지 주기
            if (transform.position == target.position)
            {
                DealDamage();  // 데미지 주기
                Destroy(gameObject);  // 발사체 제거
            }
        }
    }

    // 데미지 처리 함수
    private void DealDamage()
    {
        if (target != null)
        {
            MonsterCardData targetCard = target.GetComponent<MonsterCardData>();  // 타겟이 MonsterCardData를 가지고 있다고 가정
            if (targetCard != null)
            {
                // 카드에 데미지 적용
                targetCard.TakeDamage(damage);
                Debug.Log($"{targetCard.cardName} takes {damage} damage!");
            }
        }
    }
}
