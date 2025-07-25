using UnityEngine;

public class MonsterEffectOnClick : MonoBehaviour
{
    // �ߺ� �ߵ� ������ �÷���
    private bool effectActivated = false;

    // ī�� �Ŵ��� ���� (ī�� ��ο� ����� ���� �ʿ�)
    public CardManager_test cardManager;

    // ī�尡 Ŭ���� �� �ߵ��ϴ� �Լ�
    private void OnMouseDown()
    {
        // �̹� ȿ���� �ߵ��Ǿ����� ����
        if (effectActivated)
        {
            Debug.Log($"{gameObject.name}�� ȿ���� �̹� �ߵ��Ǿ����ϴ�.");
            return;
        }

        Debug.Log($"{gameObject.name} Ŭ����! ȿ�� �ߵ�");
        ActivateEffect(); // Ŭ�� �� ȿ�� �ߵ�
    }

    // ���� ȿ�� �ߵ� �� ��ο� ó��
    private void ActivateEffect()
    {
        Debug.Log($"{gameObject.name}�� Ŭ�� ȿ�� �ߵ�: ��ο�!");

        if (cardManager != null)
        {
            cardManager.DrawCard(); // ī�� ��ο� ����
        }
        else
        {
            Debug.LogWarning("CardManager�� �������� �ʾҽ��ϴ�.");
        }

        effectActivated = true; // �ߺ� �ߵ� ����
    }
}
