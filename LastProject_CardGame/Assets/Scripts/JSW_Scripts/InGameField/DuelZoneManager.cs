using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 모든 듀얼 존들을 통합 관리하는 매니저
/// </summary>
public class DuelZoneManager : MonoBehaviour
{
    public static DuelZoneManager Instance { get; private set; }

    [Header("존 참조")]
    public FieldSpellZone fieldSpellZone;
    public ExtraDeckZone extraDeckZone;
    public EnemyExtraDeckZone enemyExtraDeckZone;
    public PlayerGraveyardZone graveyardZone;
    public EnemyGraveyardZone enemyGraveyardZone;

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
        // 적 엑스트라 덱 초기화를 위한 지연 실행
        StartCoroutine(InitializeEnemyExtraDeckDelayed());
    }

    private IEnumerator InitializeEnemyExtraDeckDelayed()
    {
        // OpponentCardManager가 준비될 때까지 대기
        yield return new WaitForSeconds(0.1f);
        
        if (OpponentCardManager.Instance != null && enemyExtraDeckZone != null)
        {
            var deckData = DeckTransferManager.Instance?.GetDeck();
            if (deckData != null && deckData.extraDeck != null)
            {
                enemyExtraDeckZone.InitializeExtraDeck(deckData.extraDeck);
                Debug.Log($"[DuelZoneManager] 적 엑스트라 덱 초기화 완료: {deckData.extraDeck.Count}개 엔트리");
            }
        }
    }

    /// <summary>
    /// DeckTransferManager에서 엑스트라 덱 데이터 로드
    /// </summary>
    void LoadExtraDeckFromTransfer()
    {
        var deckData = DeckTransferManager.Instance?.GetDeck();
        if (deckData != null)
        {
            // 플레이어 엑스트라 덱 초기화
            if (extraDeckZone != null)
                extraDeckZone.InitializeExtraDeck(deckData.extraDeck);

            // 적 엑스트라 덱 초기화 - OpponentCardManager에서 처리하도록 변경
            // 적 엑스트라 덱은 OpponentCardManager.Start()에서 자동으로 초기화됨
            if (enemyExtraDeckZone != null)
            {
                Debug.Log("[DuelZoneManager] 적 엑스트라 덱 존 준비 완료 - OpponentCardManager에서 데이터 로드 예정");
            }
        }
        else
        {
            Debug.LogWarning("[DuelZoneManager] DeckTransferManager에서 덱 데이터를 가져올 수 없습니다.");
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
    /// 엑스트라 덱에서 카드 제거
    /// </summary>
    public BaseCardData RemoveFromExtraDeck()
    {
        if (extraDeckZone != null)
        {
            return extraDeckZone.RemoveFromExtraDeck();
        }
        return null;
    }

    public BaseCardData EnemyRemoveFromExtraDeck()
    {
        if (enemyExtraDeckZone != null)
        {
            return enemyExtraDeckZone.RemoveFromExtraDeck();
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

    public void EnemyAddToExtraDeck(BaseCardData card)
    {
        if (enemyExtraDeckZone != null)
        {
            enemyExtraDeckZone.AddToExtraDeck(card);
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

    public bool EnemyRemoveSpecificCardFromExtraDeck(BaseCardData card)
    {
        if (enemyExtraDeckZone != null)
        {
            return enemyExtraDeckZone.RemoveSpecificCard(card);
        }
        return false;
    }

    /// <summary>
    /// 카드를 묘지로 보내기
    /// </summary>
    public void SendToGraveyard(BaseCardData card, OwnerType ownerType)
    {
        Debug.Log($"SendToGraveyard / {card.cardName}, OwnerType: {ownerType}");
        if (ownerType == OwnerType.Player)
        {
            if (graveyardZone != null)
                graveyardZone.SendToGraveyard(card);
        }
        else if (ownerType == OwnerType.Opponent)
        {
            if (enemyGraveyardZone != null)
                enemyGraveyardZone.SendToGraveyard(card);
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

    public int GetEnemyExtraDeckCount()
    {
        if (enemyExtraDeckZone != null)
        {
            return enemyExtraDeckZone.GetCount();
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

    /// <summary>
    /// 묘지에서 특정 태그를 가진 카드 개수 반환
    /// </summary>
    public int GetGraveyardCardCountByTag(OwnerType owner, string tag)
    {
        if (string.IsNullOrEmpty(tag)) return 0;
        
        System.Collections.Generic.List<DeckCardEntry> entries = null;
        if (owner == OwnerType.Player)
        {
            if (graveyardZone == null) return 0;
            entries = graveyardZone.GetAllGraveyardCards();
        }
        else if (owner == OwnerType.Opponent)
        {
            if (enemyGraveyardZone == null) return 0;
            entries = enemyGraveyardZone.GetAllGraveyardCards();
        }

        if (entries == null) return 0;

        int count = 0;
        foreach (var entry in entries)
        {
            var card = entry?.card;
            if (card != null && card.tags != null)
            {
                for (int i = 0; i < card.tags.Count; i++)
                {
                    if (card.tags[i].Equals(tag, System.StringComparison.OrdinalIgnoreCase))
                    {
                        // 동일 카드가 여러 장이면 개수만큼 합산
                        count += entry.count;
                        break;
                    }
                }
            }
        }

        return count;
    }
}
