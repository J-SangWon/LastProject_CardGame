using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// ���� ī�尡 ��ȯ�� �� �ߵ��ϴ� ȿ�� ��ũ��Ʈ.
/// ����� �Ǵ� 'Enemy' �±װ� ���� ������Ʈ�� Ŭ���ϸ� ȿ���� �����.
/// </summary>
public class MonsterEffectOnSummon : MonoBehaviour
{
    private bool effectActivated = false;   // ȿ���� �̹� �ߵ��Ǿ����� ����
    private bool waitingForTarget = false;  // ��� Ŭ���� ��ٸ��� �������� ����

    public CardManager_test cardManager;    // ī�� ��ο� ����� ����ϴ� �Ŵ���
    public GraphicRaycaster raycaster;      // UI ��ҿ� ���� ����ĳ��Ʈ�� ���� ������Ʈ (Canvas�� �־�� ��)
    public EventSystem eventSystem;         // ���콺 �Է� ó���� ���� EventSystem (���� �־�� ��)

    void Start()
    {
        // GraphicRaycaster�� �������� �ʾҴٸ� �ڵ����� ������ ã�Ƽ� �Ҵ�
        if (raycaster == null)
            raycaster = Object.FindFirstObjectByType<GraphicRaycaster>();

        // EventSystem�� �������� �ʾҴٸ� �ڵ����� ������ ã�Ƽ� �Ҵ�
        if (eventSystem == null)
            eventSystem = Object.FindFirstObjectByType<EventSystem>();
    }

    /// <summary>
    /// ���� ī�尡 ��ȯ�� �� ȣ��Ǵ� �Լ�.
    /// ȿ�� �ߵ��� �˸��� ��� Ŭ���� ��ٸ��� ���·� ����.
    /// </summary>
    public void OnSummon()
    {
        // ȿ���� �̹� �ߵ��Ǿ��ٸ� �߰� �ߵ��� ����
        if (effectActivated) return;

        Debug.Log($"{gameObject.name}�� ��ȯ ȿ�� �ߵ�: ����� �����ϼ���!");
        waitingForTarget = true;  // ��� Ŭ���� ��ٸ��� ���·� ����
    }

    void Update()
    {
        // ��� ���� ��� ���̰� ���콺 ���� Ŭ�� �� ��� ���� �Լ� ȣ��
        if (waitingForTarget && Input.GetMouseButtonDown(0))
        {
            // TrySelectTarget() �Լ� ȣ��
            // �� �Լ��� ���콺�� Ŭ���� ��ġ�� ����ĳ��Ʈ�� Ȯ���ϰ�, 
            // 'Enemy' �±װ� ���� ������Ʈ�� Ŭ������ �� �ش� ������Ʈ�� ���� ȿ���� �ߵ���.
            TrySelectTarget();
        }
    }

    /// <summary>
    /// ���콺 Ŭ�� ��ġ�� ���� UI Raycast�� �����ϰ�,
    /// 'Enemy' �±װ� ���� ����� �����Ͽ� ȿ���� �����ϴ� �Լ�.
    /// </summary>
    private void TrySelectTarget()
    {
        // ���콺 ��ġ�� �̿��� Raycast ����� ��� ���� ������ ����
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        // Raycast ����� ������ ����Ʈ
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);  // Raycast ����

        // Ŭ���� UI ��ҵ� �߿��� ��('Enemy')�� ã�� ó��
        foreach (var result in results)
        {
            GameObject target = result.gameObject;

            // Ŭ���� ����� 'Enemy' �±װ� ���� ������Ʈ�� ���
            if (target.CompareTag("Enemy"))
            {
                // ���� �ı��ϴ� �Լ� ȣ��
                DestroyTarget(target);

                // ī�� ��ο� ó��
                if (cardManager != null)
                {
                    Debug.Log($"{gameObject.name}�� ȿ���� ī�带 ��ο��մϴ�.");
                    cardManager.DrawCard();  // ī�� 1�� ��ο�
                }
                else
                {
                    Debug.LogWarning("CardManager�� �������� �ʾҽ��ϴ�.");
                }

                // ȿ�� �ߵ� �Ϸ�
                effectActivated = true;
                waitingForTarget = false;  // ��� ���� ��� ���� ����
                break;  // ù ��° ���� ã���� �ݺ��� ����
            }
            else
            {
                // ���õ� ��ü�� 'Enemy'�� �ƴ� ���
                Debug.Log($"���õ� ��ü�� 'Enemy'�� �ƴմϴ�: {target.name}");
            }
        }
    }

    /// <summary>
    /// ���õ� ��� ������Ʈ�� �ı��ϴ� �Լ�.
    /// </summary>
    private void DestroyTarget(GameObject target)
    {
        Debug.Log($"{target.name}��(��) �ı��Ǿ����ϴ�!");
        Destroy(target);  // ��� ������Ʈ�� �ı�
    }
}
