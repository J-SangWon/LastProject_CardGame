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
        // 선택 카드 재사용 흐름이 아닌 경우(오브젝트가 없는 경우)에는 새로 생성
        if (currentFieldSpellCardObj == null)
        {
            CreateFieldSpellVisual();
        }
        else
        {
            // 현재 선택된 카드 오브젝트를 재사용하므로 별도 재생성하지 않음
            // AuraTracker 보장
            if (currentFieldSpellCardObj.GetComponent<AuraTracker>() == null)
            {
                currentFieldSpellCardObj.AddComponent<AuraTracker>();
            }
            // FildMonster는 제거하지 않으며, 없으면 보장
            if (currentFieldSpellCardObj.GetComponent<FildMonster>() == null)
            {
                currentFieldSpellCardObj.AddComponent<FildMonster>();
            }
            // 카드 UI 동기화 보장
            var reusedCardUI = currentFieldSpellCardObj.GetComponent<CardUI>();
            if (reusedCardUI != null)
            {
                reusedCardUI.SetCard(currentFieldSpell);
                reusedCardUI.isOnField = true;
                reusedCardUI.ownerType = OwnerType.Player;
                reusedCardUI.SetFace(true);
                reusedCardUI.EnableCardFlip = false;
            }
        }

        Debug.Log($"필드마법 발동: {fieldSpell.cardName}");

        // 필드마법 효과 발동
        // ActivateFieldSpellEffect(); // 즉시 발동하지 않음

        // 지속 효과 등록: 필드마법은 지속형으로 간주하고 AuraManager에 등록한다 (즉시 발동 아님)
        var cardUIComp = currentFieldSpellCardObj != null ? currentFieldSpellCardObj.GetComponent<CardUI>() : null;
        var fildMonster = currentFieldSpellCardObj != null ? currentFieldSpellCardObj.GetComponent<FildMonster>() : null;
        var spellDataForReg = currentFieldSpell as SpellCardData;
        if (spellDataForReg != null && fildMonster != null)
        {
            try
            {
                fildMonster.ActivateContinuousSpell(spellDataForReg);
                Debug.Log($"[FieldSpellZone] 지속 효과 등록 완료: {spellDataForReg.cardName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FieldSpellZone] 지속 효과 등록 실패: {spellDataForReg.cardName}, 오류: {e.Message}");
            }
        }
        // 현재 필드에 진입했음을 AuraManager에 통지 (즉시 오라 적용 필요 시)
        if (cardUIComp != null && AuraManager.Instance != null)
        {
            AuraManager.Instance.NotifyCardEnteredField(cardUIComp);
        }

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
            // 진행 중인 트윈 정리 후 파괴 (DOTween 에러 방지)
            DOTween.Kill(currentFieldSpellCardObj, true);
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

            // FildMonster 컴포넌트를 제거하지 않습니다.
            // AuraManager가 턴 시작/종료 시 FildMonster를 통해 턴 트리거를 호출합니다.

            // 카드 상태: 앞면, 필드 표시, 플립 비활성화
            var cardUIComponent = currentFieldSpellCardObj.GetComponent<CardUI>();
            if (cardUIComponent != null)
            {
                cardUIComponent.SetFace(true);
                cardUIComponent.isOnField = true;
                cardUIComponent.EnableCardFlip = false;
            }

            // AuraTracker 보장
            if (currentFieldSpellCardObj.GetComponent<AuraTracker>() == null)
            {
                currentFieldSpellCardObj.AddComponent<AuraTracker>();
            }
            // FildMonster 보장 (필요 시)
            if (currentFieldSpellCardObj.GetComponent<FildMonster>() == null)
            {
                currentFieldSpellCardObj.AddComponent<FildMonster>();
            }

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

            // 오브젝트 제거 전 진행 중 트윈 정리 (DOTween 에러 방지)
            DOTween.Kill(currentFieldSpellCardObj, true);
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
                    // 필드마법인 경우에만 처리: 드롭된 카드 오브젝트를 그대로 재사용하여 배치
                    if (currentFieldSpell != null)
                    {
                        RemoveFieldSpell();
                    }

                    PlaceFieldSpellObject(cardObject, cardUI);
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

        // 선택된 카드 오브젝트를 그대로 배치 (DOMove) 및 등록 처리
        PlaceFieldSpellObject(selected, cardUI);

        // 턴 트리거 조건이 있는 경우: 즉시 발동하지 않음 (OnTurnStart/OnTurnEnd)
        bool hasTurnTrigger = false;
        var spellData = currentFieldSpell as SpellCardData;
        var cond = spellData != null && spellData.cardAbility != null ? spellData.cardAbility.condition : null;
        if (cond != null && cond.conditionType != null)
        {
            foreach (var t in cond.conditionType)
            {
                if (t == ConditionType.OnTurnStart || t == ConditionType.OnTurnEnd)
                {
                    hasTurnTrigger = true;
                    break;
                }
            }
        }

        if (!hasTurnTrigger)
        {
            // 즉시 발동형 필드마법만 바로 발동
            ActivateFieldSpellEffect();
        }
        else
        {
            Debug.Log($"필드마법 {currentFieldSpell.cardName}은 턴 트리거 조건이 있어 배치 시 즉시 발동하지 않습니다.");
        }
        return true;
    }

    // 드롭/선택 공통: 기존 카드 오브젝트를 재사용해 필드마법 존으로 이동시키고 상태/등록을 완료한다.
    private void PlaceFieldSpellObject(GameObject cardObj, CardUI cardUI)
    {
        // 이동 및 부모 설정
        cardObj.transform.SetParent(transform);
        cardObj.transform.localScale = Vector3.one;
        cardObj.transform.DOMove(transform.position, 0.5f).SetEase(Ease.OutQuad);

        // 카드 UI 상태 갱신
        cardUI.SetFace(true);
        cardUI.isOnField = true;
        if (cardUI.ownerType != OwnerType.Player)
        {
            cardUI.ownerType = OwnerType.Player;
        }
        cardUI.EnableCardFlip = false;

        // 손패 플래그 해제
        var handCard = cardObj.GetComponent<HandCard>();
        if (handCard != null) handCard.isInHand = false;

        // 드래그 플래그 갱신
        var drag = cardObj.GetComponent<CardDragHandler>();
        if (drag != null) drag.isSummoned = true;

        // 내부 상태 반영
        currentFieldSpell = cardUI.cardData;
        currentFieldSpellCardObj = cardObj;
        isOccupied = true;

        // UI 텍스트 업데이트
        if (fieldSpellNameText != null)
            fieldSpellNameText.text = currentFieldSpell.cardName;

        // 선택/손패 레이아웃 갱신
        CardSummonManager.Instance?.DeselectCard();
        PlayerCardManager.Instance?.UpdateHandLayout();

        // 지속 효과 등록 (즉시 발동 없음)
        var fm = cardObj.GetComponent<FildMonster>();
        if (fm == null) fm = cardObj.AddComponent<FildMonster>(); // 안전망
        var spellDataForReg = currentFieldSpell as SpellCardData;
        if (spellDataForReg != null && fm != null)
        {
            try
            {
                fm.ActivateContinuousSpell(spellDataForReg);
                Debug.Log($"[FieldSpellZone] 지속 효과 등록 완료(재사용): {spellDataForReg.cardName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FieldSpellZone] 지속 효과 등록 실패(재사용): {spellDataForReg.cardName}, 오류: {e.Message}");
            }
        }
        if (AuraManager.Instance != null)
        {
            AuraManager.Instance.NotifyCardEnteredField(cardUI);
        }

        // AuraTracker 안전망 (프리팹에 이미 있으면 추가하지 않음)
        if (cardObj.GetComponent<AuraTracker>() == null)
        {
            cardObj.AddComponent<AuraTracker>();
        }
    }
}