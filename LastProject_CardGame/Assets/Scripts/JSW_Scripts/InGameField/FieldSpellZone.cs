using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

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
    [Header("상태")]
    public bool isOccupied = false;

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

        // 기존 필드마법이 있다면 제거
        if (currentFieldSpell != null)
        {
            Debug.Log($"기존 필드마법 '{currentFieldSpell.cardName}'을(를) 제거하고 새 필드마법 '{fieldSpell.cardName}'을(를) 발동합니다.");
            RemoveFieldSpell();
        }

        // 새 필드마법 설정
        currentFieldSpell = fieldSpell;
        CreateFieldSpellVisual();

        Debug.Log($"필드마법 발동: {fieldSpell.cardName}");

        // 필드마법 효과 발동
        ActivateFieldSpellEffect();

        if (currentFieldSpellCardObj != null)
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
        // 클릭 기반: 선택된 카드가 필드마법이면 이 존에 발동
        var selected = CardSummonManager.Instance != null ? CardSummonManager.Instance.GetSelectedCard() : null;
        if (selected != null)
        {
            if (TryActivateFieldSpellFromSelectedCard(selected))
                return; // 성공 시 여기서 종료
        }

        // 정보 로그
        if (currentFieldSpell != null)
        {
            Debug.Log($"현재 발동된 필드마법: {currentFieldSpell.cardName}");
        }
        else
        {
            Debug.Log("현재 발동된 필드마법이 없습니다.");
        }
    }

    /// <summary>
    /// 필드마법 효과 발동
    /// </summary>
    private void ActivateFieldSpellEffect()
    {
        if (currentFieldSpell == null || currentFieldSpell.cardAbility == null)
        {
            Debug.LogWarning("필드마법 또는 카드 능력이 null입니다!");
            return;
        }

        Debug.Log($"필드마법 효과 발동 시도: {currentFieldSpell.cardName}");

        try
        {
            // 조건 확인
            if (currentFieldSpell.cardAbility.condition != null)
            {
                bool conditionMet = EffectConditionEvaluator.IsConditionMet(
                    currentFieldSpell.cardAbility.condition,
                    GameManager.Instance.CurrentPhase,
                    ConditionType.OnCardPlayed,
                    currentFieldSpell.cardId,
                    0
                );

                Debug.Log($"필드마법 조건 확인 결과: {conditionMet}");

                if (!conditionMet)
                {
                    Debug.Log("필드마법 효과 조건이 충족되지 않았습니다.");
                    return;
                }
            }

            // 효과 발동
            AbilityParameter param = new AbilityParameter();
            param.value = currentFieldSpell.abilityValue;

            Debug.Log($"필드마법 효과 발동: {currentFieldSpell.cardName}, abilityValue: {currentFieldSpell.abilityValue}");

            currentFieldSpell.cardAbility.Activate(null, param);
            Debug.Log($"필드마법 효과 발동 성공: {currentFieldSpell.cardName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"필드마법 효과 발동 실패: {currentFieldSpell.cardName}, 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 필드마법 표현 생성
    /// </summary>
    void CreateFieldSpellVisual()
    {
        // 기존 필드마법 카드 오브젝트가 있다면 제거
        if (currentFieldSpellCardObj != null)
        {
            Destroy(currentFieldSpellCardObj);
            currentFieldSpellCardObj = null;
        }

        if (currentFieldSpell != null && cardPrefab != null)
        {
            // 새 필드마법 카드 오브젝트 생성
            currentFieldSpellCardObj = Instantiate(cardPrefab, transform);
            currentFieldSpellCardObj.transform.localScale = Vector3.one;
            currentFieldSpellCardObj.transform.localPosition = Vector3.zero;

            // 카드 UI 설정
            var cardUI = currentFieldSpellCardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.SetCard(currentFieldSpell);
                cardUI.isOnField = true; // 필드에 있다고 표시
                cardUI.ownerType = OwnerType.Player; // 플레이어 소유로 설정
            }

            // FildMonster 컴포넌트 제거
            var existingFieldMonster = currentFieldSpellCardObj.GetComponent<FildMonster>();
            if (existingFieldMonster != null)
            {
                DestroyImmediate(existingFieldMonster);
                Debug.Log("필드마법 카드에서 FildMonster 컴포넌트를 제거했습니다.");
            }

            // 카드 플립 기능 비활성화 (필드마법은 플립 불필요)
            var cardUIComponent = currentFieldSpellCardObj.GetComponent<CardUI>();
            if (cardUIComponent != null)
                cardUIComponent.EnableCardFlip = false;

            // UI 텍스트 업데이트
            if (fieldSpellNameText != null)
                fieldSpellNameText.text = currentFieldSpell.cardName;

            isOccupied = true;
        }
    }

    /// <summary>
    /// 필드마법 제거
    /// </summary>
    public void RemoveFieldSpell()
    {
        if (currentFieldSpell != null && currentFieldSpellCardObj != null)
        {
            // 필드마법 제거 시 효과 (필요시 구현)
            if (currentFieldSpell.cardAbility != null)
            {
                Debug.Log($"필드마법 제거: {currentFieldSpell.cardName}");
                // 제거 시 효과가 있다면 여기서 처리
            }

            // 카드 소유자 확인
            var cardUI = currentFieldSpellCardObj.GetComponent<CardUI>();
            OwnerType ownerType = cardUI != null ? cardUI.ownerType : OwnerType.Player;

            // 묘지로 보내기
            if (DuelZoneManager.Instance != null)
            {
                DuelZoneManager.Instance.SendToGraveyard(currentFieldSpell, ownerType);
                Debug.Log($"필드마법 '{currentFieldSpell.cardName}'을(를) {ownerType}의 묘지로 보냈습니다.");
            }
            else
            {
                Debug.LogWarning("DuelZoneManager.Instance가 null입니다. 묘지로 보내지 못했습니다.");
            }

            // 오브젝트 제거
            Destroy(currentFieldSpellCardObj);
            currentFieldSpellCardObj = null;
        }

        // 데이터 정리
        currentFieldSpell = null;

        // UI 텍스트 업데이트
        if (fieldSpellNameText != null)
            fieldSpellNameText.text = "필드마법 없음";

        Debug.Log("기존 필드마법이 제거되었습니다.");

        isOccupied = false;
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
            // 필드마법 타입인지 먼저 확인
            if (cardUI.cardData.cardType == CardType.Spell)
            {
                var spellCard = cardUI.cardData as SpellCardData;
                if (spellCard != null && spellCard.spellType == SpellType.Field)
                {
                    // 필드마법인 경우에만 처리
                    bool success = ActivateFieldSpell(cardUI.cardData);
                    if (success)
                    {
                        // 드롭 성공 시 원본 카드 제거 (손패에서)
                        Debug.Log($"필드마법 드롭 성공: {cardUI.cardData.cardName}");

                        // 원본 카드가 손패에 있다면 제거
                        if (cardUI.ownerType == OwnerType.Player)
                        {
                            // PlayerCardManager에서 손패에서 제거 (간단하게 Destroy로 처리)
                            Debug.Log($"플레이어 손패에서 필드마법 카드 제거: {cardUI.cardData.cardName}");
                        }
                        else
                        {
                            // OpponentCardManager에서 손패에서 제거 (간단하게 Destroy로 처리)
                            Debug.Log($"상대방 손패에서 필드마법 카드 제거: {cardUI.cardData.cardName}");
                        }

                        // 원본 카드 오브젝트 제거
                        Destroy(cardObject);
                    }
                }
                else
                {
                    // 일반 마법카드인 경우 - 필드에 소환하지 않음
                    Debug.Log($"일반 마법카드 '{cardUI.cardData.cardName}'은(는) 필드마법 존에 드롭할 수 없습니다.");

                    // 카드를 원래 위치로 되돌리기 (손패로)
                    if (cardUI.ownerType == OwnerType.Player)
                    {
                        cardObject.transform.SetParent(PlayerCardManager.Instance.playerHandZone, false);
                        cardObject.transform.localScale = Vector3.one;
                        cardObject.transform.localPosition = Vector3.zero;
                        Debug.Log($"일반 마법카드 '{cardUI.cardData.cardName}'을(를) 손패로 되돌렸습니다.");
                    }
                    else
                    {
                        cardObject.transform.SetParent(OpponentCardManager.Instance.handZone, false);
                        cardObject.transform.localScale = Vector3.one;
                        cardObject.transform.localPosition = Vector3.zero;
                        Debug.Log($"일반 마법카드 '{cardUI.cardData.cardName}'을(를) 손패로 되돌렸습니다.");
                    }
                }
            }
            else
            {
                // 마법카드가 아닌 경우
                Debug.Log($"마법카드가 아닌 '{cardUI.cardData.cardName}'은(는) 필드마법 존에 드롭할 수 없습니다.");
            }
        }
    }

    // 선택된 카드에서 필드마법 발동 시도 (클릭 흐름용)
    private bool TryActivateFieldSpellFromSelectedCard(GameObject selected)
    {
        if (selected == null) { return false; }
        if (!(GameManager.Instance?.IsPlayerTurn() ?? false)) { return false; }
        if (GameManager.Instance.CurrentPhase != GamePhase.MainPhase) { return false; }
        var cardUI = selected.GetComponent<CardUI>();
        if (cardUI == null || cardUI.cardData == null) { return false; }
        if (cardUI.isOnField) { return false; }
        if (cardUI.ownerType != OwnerType.Player) { return false; }

        if (cardUI.cardData.cardType != CardType.Spell) { return false; }
        var spellCard = cardUI.cardData as SpellCardData;
        if (spellCard == null || spellCard.spellType != SpellType.Field) { return false; }


        // 교체 정책: 기존 필드마법이 있으면 제거 후 교체
        if (currentFieldSpell != null)
        {
            RemoveFieldSpell();
        }

        // 실제 선택된 카드 오브젝트를 필드마법 존으로 이동 (MonsterZoneSlot과 동일 패턴)
        selected.transform.SetParent(transform);
        selected.transform.localScale = Vector3.one;
        selected.transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutQuad);

        // 카드 UI 상태 갱신: 앞면, 필드표시, 소유자, 플립 비활성화
        cardUI.SetFace(true);
        cardUI.isOnField = true;
        cardUI.ownerType = OwnerType.Player;
        cardUI.EnableCardFlip = false;

        // 손패 상태 해제
        var handCard = selected.GetComponent<HandCard>();
        if (handCard != null) handCard.isInHand = false;

        // 몬스터 전용 컴포넌트 제거 (있을 경우)
        var fm = selected.GetComponent<FildMonster>();
        if (fm != null)
        {
            DestroyImmediate(fm);
        }

        // 내부 상태 반영
        currentFieldSpell = cardUI.cardData;
        currentFieldSpellCardObj = selected;
        isOccupied = true;

        // UI 텍스트 업데이트
        if (fieldSpellNameText != null)
            fieldSpellNameText.text = currentFieldSpell.cardName;

        // 선택 해제/손패 레이아웃 갱신 (코스트 소모는 다른 곳에서 처리)
        CardSummonManager.Instance?.DeselectCard();
        PlayerCardManager.Instance?.UpdateHandLayout();

        // 효과 발동
        ActivateFieldSpellEffect();
        return true;
    }
}