using System.Collections.Generic;
using UnityEngine;

public enum CardPackType
{
    Default, // 기본 카드 팩
    Dragon, // 드래곤 카드 팩
    GreeceRoman, // 그리스 로마 카드 팩
    Undead, // 언데드 카드 팩
    Ailen   // 외계인 카드 팩
}

[CreateAssetMenu(menuName = "CardPack")]
public class CardPackData : ScriptableObject
{
    public CardPackType packType; // 카드 팩 이름
    public Sprite packArtwork; // 카드 팩 아트워크
    public Sprite packUpImg; // 카드 팩 업 이미지
    public Sprite packDownImg; // 카드 팩 다운 이미지
    public List<BaseCardData> cards; // 카드 팩에 포함된 카드 목록
}
