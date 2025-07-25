using UnityEngine;

public class CardTestLoader : MonoBehaviour
{
    public string cardIdToLoad = "84789399-d353-4f3b-ad2c-ec4f25cf3158"; // �׽�Ʈ�� ī�� ID
    public CardUI_N cardUIPrefab; // CardUI ������
    public Transform spawnParent; // UI�� ��ġ�� �θ� ������Ʈ

    void Start()
    {
        // ��� ī�� �ҷ�����
        BaseCardData[] allCards = Resources.LoadAll<BaseCardData>("CardData");

        // �ش� ID�� ī�� ã��
        foreach (var card in allCards)
        {
            if (card.cardId == cardIdToLoad)
            {
                Debug.Log($"ī�� '{card.cardName}' �ε� ����");

                // ī�� UI �ν��Ͻ� ����
                CardUI_N ui = Instantiate(cardUIPrefab, spawnParent);
                ui.SetCard(card);
                return;
            }
        }

        Debug.LogWarning("��ġ�ϴ� ī�� ID�� ���� ī�尡 �����ϴ�.");
    }
}
