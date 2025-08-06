using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class TargetableCard : MonoBehaviour
{
    public int health = 1000;
    public TextMeshProUGUI healthText;

    public bool interactable = true; //  추가: 클릭 또는 타겟 가능 여부

    public event Action OnDestroyed;

    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    private void Start()
    {
        UpdateHealthText();
    }

    public void TakeDamage(int damage)
    {
        if (!interactable) return; //  추가: 비활성 카드에 데미지 무시

        health -= damage;
        UpdateHealthText();

        if (health <= 0)
        {
            OnDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = health.ToString();
    }

    //  선택: 카드 강조 표시 (예: 마우스 오버 or 타겟 선택 시)
    public void SetHighlight(bool on)
    {
        if (outline != null)
            outline.enabled = on && interactable;
    }
}
