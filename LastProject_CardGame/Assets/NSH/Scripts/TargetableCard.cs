using UnityEngine;
using TMPro;
using System;

public class TargetableCard : MonoBehaviour
{
    public int health = 1000;
    public TextMeshProUGUI healthText;

    // 카드가 파괴될 때 호출되는 이벤트
    public event Action OnDestroyed;

    private void Start()
    {
        UpdateHealthText();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateHealthText();

        if (health <= 0)
        {
            // 이벤트 호출 전에 null 체크 필수
            OnDestroyed?.Invoke();

            Destroy(gameObject);
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = health.ToString();
    }
}
