using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 필드마법 존을 관리하는 스크립트
/// </summary>
public class FieldSpellZone : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 요소")]
    public Image fieldSpellImage;
    public TextMeshProUGUI fieldSpellNameText;
    public GameObject fieldSpellCardObject;

    [Header("카드 프리팹")]
    public GameObject cardPrefab;

    private BaseCardData currentFieldSpell = null;
    private GameObject currentFieldSpellCardObj = null;

    void Start()
    {
        RemoveFieldSpell();
        cardPrefab = PlayerCardManager.Instance.cardPrefab;

    }

    /// <summary>
    /// 필드마법 발동
    /// </summary>
    public bool ActivateFieldSpell(BaseCardData fieldSpell)
    {
        if (fieldSpell == null || fieldSpell.cardType != CardType.Spell)
        {
            Debug.LogWarning("필드마법이 아닌 카드입니다!");
            return false;
        }

        var spellCard = fieldSpell as SpellCardData;
        if (spellCard == null || spellCard.spellType != SpellType.Field)
        {
            Debug.LogWarning("필드마법 타입이 아닙니다!");
            return false;
        }

        // 기존 필드마법 제거
        RemoveFieldSpell();

        // 새 필드마법 설정
        currentFieldSpell = fieldSpell;
        CreateFieldSpellVisual();

        Debug.Log($"필드마법 발동: {fieldSpell.cardName}");
        if(currentFieldSpellCardObj != null)
        {
            // 필드마법 카드 오브젝트가 생성되었을 때
            var objDragHandler = currentFieldSpellCardObj.GetComponent<CardDragHandler>();
            if (objDragHandler != null)
            {
                objDragHandler.isSummoned = true;
            }
            else
            {
                Debug.LogWarning("필드마법 카드 오브젝트에 드래그 핸들러가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("필드마법 카드 오브젝트가 생성되지 않았습니다.");
        }

        return true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("FieldSpellZone 클릭됨!");

        if (currentFieldSpell != null)
        {
            
        }
        else
        {
            Debug.Log("현재 발동된 필드마법이 없습니다.");
        }
    }
    /// <summary>
    /// 필드마법 표현 생성
    /// </summary>
    void CreateFieldSpellVisual()
    {
        if (currentFieldSpell != null && cardPrefab != null)
        {
            currentFieldSpellCardObj = Instantiate(cardPrefab, transform);
            currentFieldSpellCardObj.transform.localScale = Vector3.one;
            currentFieldSpellCardObj.transform.localPosition = Vector3.zero;

            // 카드 UI 설정
            var cardUI = currentFieldSpellCardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetCard(currentFieldSpell);
            }

            // 카드 플립 기능 비활성화 (필드마법은 플립 불필요)
            var cardUIComponent = currentFieldSpellCardObj.GetComponent<CardUI>();
            if (cardUIComponent != null)
                cardUIComponent.EnableCardFlip = false;

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
        if (currentFieldSpellCardObj != null)
        {
            DuelZoneManager.Instance.SendToGraveyard(currentFieldSpell);
            Destroy(currentFieldSpellCardObj);
            currentFieldSpellCardObj = null;
        }
        currentFieldSpell = null;

        if (fieldSpellNameText != null)
            fieldSpellNameText.text = "필드마법 없음";
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

    public void OnCardDropped(GameObject cardObject)
    {
        var cardUI = cardObject.GetComponent<CardUI>();
        if (cardUI != null && cardUI.cardData != null)
        {
            bool success = ActivateFieldSpell(cardUI.cardData);
            if (success)
            {
                // 드롭 성공 시 처리
                currentFieldSpellCardObj = cardObject;
                // 기존 카드 오브젝트 제거
                Destroy(cardObject);
            }
        }
    }

}