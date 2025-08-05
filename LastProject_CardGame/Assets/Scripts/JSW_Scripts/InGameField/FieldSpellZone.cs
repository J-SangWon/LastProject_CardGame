using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 필드마법 존을 관리하는 스크립트
/// </summary>
public class FieldSpellZone : MonoBehaviour
{
    [Header("UI 요소")]
    public Image fieldSpellImage;
    public TextMeshProUGUI fieldSpellNameText;
    public GameObject fieldSpellCardObject;
    
    [Header("카드 프리팹")]
    public GameObject cardPrefab;
    
    private BaseCardData currentFieldSpell = null;
    private GameObject currentFieldSpellCard = null;

    void Start()
    {
        ClearFieldSpell();
    }

    /// <summary>
    /// 필드마법 발동
    /// </summary>
    public bool ActivateFieldSpell(BaseCardData fieldSpell)
    {
        if (fieldSpell == null || fieldSpell.cardType != CardType.Spell)
            return false;
            
        var spellCard = fieldSpell as SpellCardData;
        if (spellCard == null || spellCard.spellType != SpellType.Field)
            return false;

        // 기존 필드마법 제거
        RemoveFieldSpell();

        // 새 필드마법 설정
        currentFieldSpell = fieldSpell;
        CreateFieldSpellVisual();
        
        Debug.Log($"필드마법 발동: {fieldSpell.cardName}");
        return true;
    }

    /// <summary>
    /// 필드마법 시각적 표현 생성
    /// </summary>
    void CreateFieldSpellVisual()
    {
        if (currentFieldSpell != null && cardPrefab != null)
        {
            currentFieldSpellCard = Instantiate(cardPrefab, transform);
            currentFieldSpellCard.transform.localScale = Vector3.one;
            currentFieldSpellCard.transform.localPosition = Vector3.zero;
            
            // 카드 UI 설정
            var cardUI = currentFieldSpellCard.GetComponent<CardUI_N>();
            if (cardUI != null)
            {
                cardUI.SetCard(currentFieldSpell);
            }
            
            // 필드마법은 특별한 시각적 효과
            currentFieldSpellCard.transform.localScale = Vector3.one * 1.2f;
            
            // UI 텍스트 업데이트
            if (fieldSpellNameText != null)
                fieldSpellNameText.text = currentFieldSpell.cardName;
        }
    }

    /// <summary>
    /// 필드마법 제거
    /// </summary>
    public void RemoveFieldSpell()
    {
        if (currentFieldSpellCard != null)
        {
            Destroy(currentFieldSpellCard);
            currentFieldSpellCard = null;
        }
        currentFieldSpell = null;
        
        if (fieldSpellNameText != null)
            fieldSpellNameText.text = "필드마법 없음";
    }

    /// <summary>
    /// 필드마법 초기화
    /// </summary>
    public void ClearFieldSpell()
    {
        RemoveFieldSpell();
    }

    /// <summary>
    /// 현재 필드마법 반환
    /// </summary>
    public BaseCardData GetCurrentFieldSpell()
    {
        return currentFieldSpell;
    }

    /// <summary>
    /// 필드마법이 있는지 확인
    /// </summary>
    public bool HasFieldSpell()
    {
        return currentFieldSpell != null;
    }
}