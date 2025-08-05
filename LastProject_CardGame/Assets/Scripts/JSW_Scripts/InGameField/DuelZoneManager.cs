using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 듀얼 존들을 통합 관리하는 매니저
/// </summary>
public class DuelZoneManager : MonoBehaviour
{
    public static DuelZoneManager Instance { get; private set; }

    [Header("존 참조")]
    public FieldSpellZone fieldSpellZone;
    public ExtraDeckZone extraDeckZone;
    public GraveyardZone graveyardZone;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        LoadExtraDeckFromTransfer();
    }

    /// <summary>
    /// DeckTransferManager에서 엑스트라 덱 데이터 로드
    /// </summary>
    void LoadExtraDeckFromTransfer()
    {
        var deckData = DeckTransferManager.Instance?.GetDeck();
        if (deckData != null && extraDeckZone != null)
        {
            // DeckCardEntry 리스트를 그대로 전달
            extraDeckZone.InitializeExtraDeck(deckData.extraDeck);
        }
    }

    /// <summary>
    /// 필드마법 발동
    /// </summary>
    public bool ActivateFieldSpell(BaseCardData fieldSpell)
    {
        if (fieldSpellZone != null)
        {
            return fieldSpellZone.ActivateFieldSpell(fieldSpell);
        }
        return false;
    }

    /// <summary>
    /// 필드마법 제거
    /// </summary>
    public void RemoveFieldSpell()
    {
        if (fieldSpellZone != null)
        {
            fieldSpellZone.RemoveFieldSpell();
        }
    }

    /// <summary>
    /// 엑스트라 덱에서 카드 제거 (DrawFromExtraDeck으로 메서드명 변경)
    /// </summary>
    public BaseCardData RemoveFromExtraDeck()
    {
        if (extraDeckZone != null)
        {
            return extraDeckZone.RemoveFromExtraDeck();
        }
        return null;
    }

    /// <summary>
    /// 엑스트라 덱에 카드 추가
    /// </summary>
    public void AddToExtraDeck(BaseCardData card)
    {
        if (extraDeckZone != null)
        {
            extraDeckZone.AddToExtraDeck(card);
        }
    }

    /// <summary>
    /// 엑스트라 덱에서 특정 카드 제거
    /// </summary>
    public bool RemoveSpecificCardFromExtraDeck(BaseCardData card)
    {
        if (extraDeckZone != null)
        {
            return extraDeckZone.RemoveSpecificCard(card);
        }
        return false;
    }

    /// <summary>
    /// 카드를 묘지로 보내기
    /// </summary>
    public void SendToGraveyard(BaseCardData card)
    {
        if (graveyardZone != null)
        {
            graveyardZone.SendToGraveyard(card);
        }
    }

    /// <summary>
    /// 묘지 확인
    /// </summary>
    public void ShowGraveyard()
    {
        if (graveyardZone != null)
        {
            graveyardZone.ShowGraveyard();
        }
    }

    /// <summary>
    /// 현재 필드마법 반환
    /// </summary>
    public BaseCardData GetCurrentFieldSpell()
    {
        if (fieldSpellZone != null)
        {
            return fieldSpellZone.GetCurrentFieldSpell();
        }
        return null;
    }

    /// <summary>
    /// 엑스트라 덱 카드 수 반환 (GetCount로 메서드명 통일)
    /// </summary>
    public int GetExtraDeckCount()
    {
        if (extraDeckZone != null)
        {
            return extraDeckZone.GetCount();
        }
        return 0;
    }

    /// <summary>
    /// 엑스트라 덱의 모든 엔트리 반환
    /// </summary>
    public List<DeckCardEntry> GetExtraDeckEntries()
    {
        if (extraDeckZone != null)
        {
            return extraDeckZone.GetAllEntries();
        }
        return new List<DeckCardEntry>();
    }

    /// <summary>
    /// 묘지 카드 수 반환
    /// </summary>
    public int GetGraveyardCount()
    {
        if (graveyardZone != null)
        {
            return graveyardZone.GetGraveyardCount();
        }
        return 0;
    }
}