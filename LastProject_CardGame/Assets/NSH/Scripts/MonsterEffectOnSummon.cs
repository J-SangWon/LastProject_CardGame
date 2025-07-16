using UnityEngine;

public class MonsterEffectOnSummon : MonoBehaviour
{
    private bool effectActivated = false;

    public CardManager_test cardManager;

    public void OnSummon()
    {
        if (effectActivated) return;

        Debug.Log($"{gameObject.name} ��ȯ��! ��ο� ȿ�� �ߵ�");

        if (cardManager != null)
        {
            cardManager.DrawCard(); // ī�� 1�� ��ο�
        }
        else
        {
            Debug.LogWarning("CardManager�� �Ҵ���� �ʾҽ��ϴ�.");
        }

        effectActivated = true;
    }
}
