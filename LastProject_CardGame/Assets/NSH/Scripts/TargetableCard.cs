using UnityEngine;
using TMPro;

/// <summary>
/// 카드(몬스터 등) 체력과 데미지 처리 담당 컴포넌트.
/// 체력 텍스트를 가지고 있고, 데미지 입으면 체력 차감 후 0이하 시 파괴.
/// </summary>
public class TargetableCard : MonoBehaviour
{
    public int health = 1000;
    public TextMeshProUGUI healthText;  // 자식 오브젝트에 체력 표시 텍스트

    private void Start()
    {
        UpdateHealthText();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateHealthText();

        Debug.Log($"[TargetableCard] {gameObject.name} 데미지 {damage} 입음. 남은 체력 {health}");

        if (health <= 0)
        {
            Debug.Log($"[TargetableCard] {gameObject.name} 파괴됨 (체력 0 이하)");
            Destroy(gameObject);
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = health.ToString();
    }
}
