using System.Collections.Generic;
using UnityEngine;

public enum CardPackType
{
    Default, // �⺻ ī�� ��
    Dragon, // �巡�� ī�� ��
    GreeceRoman, // �׸��� �θ� ī�� ��
    Undead, // �𵥵� ī�� ��
}

[CreateAssetMenu(menuName = "CardPack")]
public class CardPackData : ScriptableObject
{
    public CardPackType packType; // ī�� �� �̸�
    public string description; // ī�� �� ����
    public Sprite packArtwork; // ī�� �� ��Ʈ��ũ
    public int packCost; // ī�� �� ����
    public List<BaseCardData> cards; // ī�� �ѿ� ���Ե� ī�� ���

}
