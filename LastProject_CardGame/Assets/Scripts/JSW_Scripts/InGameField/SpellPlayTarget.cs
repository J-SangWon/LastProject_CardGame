using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class SpellPlayTarget : MonoBehaviour, IPointerClickHandler
{
    [Header("Optional: 타겟 앵커(미지정 시 자기 자신)")]
    public Transform targetAnchor;

    [Header("연출 설정")]
    [SerializeField] private float moveDuration = 0.4f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    public void OnPointerClick(PointerEventData eventData)
    {
        var selected = CardSummonManager.Instance != null ? CardSummonManager.Instance.GetSelectedCard() : null;
        if (selected == null) return;

        var cardUI = selected.GetComponent<CardUI>();
        if (cardUI == null || cardUI.cardData == null) return;
        if (!cardUI.isFront) return; // 앞면만 발동 허용
        if (cardUI.cardData.cardType != CardType.Spell) return; // 마법만

        var spell = cardUI.cardData as SpellCardData;
        if (spell == null) return;
        // 필드 마법은 제외 (전용 존에서 처리)
        if (spell.spellType == SpellType.Field) return;
        
        // 조건 선체크: 조건 미충족이면 손패 유지(코스트 미소모/이동 없음)
        if (spell.spellType != SpellType.Continuous && spell.cardAbility != null && spell.cardAbility.condition != null)
        {
            bool cond = EffectConditionEvaluator.IsConditionMet(
                spell.cardAbility.condition,
                GameManager.Instance.CurrentPhase,
                ConditionType.OnCardPlayed,
                spell.cardId,
                0,
                cardUI != null ? cardUI.ownerType : OwnerType.Player
            );
            if (!cond)
            {
                Debug.Log("[SpellPlayTarget] 조건 미충족: 마법 카드는 손패에 유지됩니다.");
                return;
            }
        }
        
        // 코스트 체크 및 소모 (카드 이동 전에 먼저 체크)
        int cost = spell.cost;
        if (cardUI.ownerType == OwnerType.Player)
        {
            if (!GameManager.Instance.TrySpendPlayerCost(cost))
            {
                Debug.Log($"플레이어 코스트 부족: 필요 {cost}, 현재 {GameManager.Instance.playerCurrentCost}");
                return;
            }
        }
        else if (cardUI.ownerType == OwnerType.Opponent)
        {
            if (!GameManager.Instance.SpendEnemyCost(cost))
            {
                Debug.Log($"적 코스트 부족: 필요 {cost}, 현재 {GameManager.Instance.enemyCurrentCost}");
                return;
            }
        }

        // 애니메이션 타겟 및 부모 지정 (FieldSpellZone 패턴과 유사)
        Transform target = targetAnchor != null ? targetAnchor : transform;
        selected.transform.SetParent(target, true);
        Vector3 targetPos = target.position;

        // 트윈 시작 전 레이아웃 간섭 방지
        var layout = selected.GetComponent<LayoutElement>();
        if (layout != null) layout.ignoreLayout = true;

        // 기존 트윈 정리 후 이동
        selected.transform.DOKill();
        selected.transform.DOMove(targetPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                // UI 좌표 스냅 정렬
                var rt = selected.transform as RectTransform;
                if (rt != null)
                    rt.anchoredPosition3D = Vector3.zero;
                else
                    selected.transform.localPosition = Vector3.zero;

                // 카드 UI 상태 갱신: 앞면, 필드표시, 플립 비활성화
                cardUI.SetFace(true);
                cardUI.isOnField = true;
                cardUI.EnableCardFlip = false;

                // 선택 해제 & 손패 레이아웃 갱신
                CardSummonManager.Instance?.DeselectCard();
                PlayerCardManager.Instance?.UpdateHandLayout();

                // FildMonster를 통해 마법 효과 실행
                var fm = selected.GetComponent<FildMonster>();
                if (fm != null)
                {
                    if (spell.spellType == SpellType.Continuous)
                    {
                        // 지속 마법: 필드에 남기고 지속 효과 활성화
                        fm.ActivateContinuousSpell(spell);
                    }
                    else
                    {
                        // 단일 타겟 마법이면 타겟팅 모드로 전환하고 즉시 발동/제거하지 않음
                        if (spell.cardAbility != null && spell.cardAbility.targetType == TargetType.Single)
                        {
                            Debug.Log($"[SpellPlayTarget] Single 타겟팅 모드 진입: {spell.cardName}");
                            BattleManager.Instance.IsAbilityTargeting = true;
                            BattleManager.Instance.SetAbilityCaster(selected);
                        }
                        else
                        {
                            // 비타겟/다중 타겟: 즉시 발동 후 묘지로 이동
                            Debug.Log($"[SpellPlayTarget] 즉시 발동 경로: {spell.cardName}, targetType={spell.cardAbility?.targetType}");
                            fm.ActivateSpellEffect(spell);
                            var dz = DuelZoneManager.Instance;
                            if (dz != null)
                            {
                                if (cardUI.ownerType == OwnerType.Player)
                                    dz.graveyardZone?.SendToGraveyard(spell);
                                else if (cardUI.ownerType == OwnerType.Opponent)
                                    dz.enemyGraveyardZone?.SendToGraveyard(spell);
                            }
                            Object.Destroy(selected);
                        }
                    }
                }
                else
                {
                    // FildMonster가 없으면 안전하게 제거 처리(연결 누락 대비)
                    if (spell.spellType != SpellType.Continuous)
                    {
                        // 단일 타겟이면 타겟팅 모드 진입만 시도 (FildMonster가 없으므로 폴백 처리)
                        if (spell.cardAbility != null && spell.cardAbility.targetType == TargetType.Single)
                        {
                            BattleManager.Instance.IsAbilityTargeting = true;
                            BattleManager.Instance.SetAbilityCaster(selected);
                        }
                        else
                        {
                            var dz = DuelZoneManager.Instance;
                            if (dz != null)
                            {
                                if (cardUI.ownerType == OwnerType.Player)
                                    dz.graveyardZone?.SendToGraveyard(spell);
                                else if (cardUI.ownerType == OwnerType.Opponent)
                                    dz.enemyGraveyardZone?.SendToGraveyard(spell);
                            }
                            Object.Destroy(selected);
                        }
                    }
                }
            });
    }

    /// <summary>
    /// AI가 손패에 있는 특정 스펠 카드를 이 타겟 존으로 발동하려고 할 때 호출
    /// </summary>
    public bool TryPlaySpellFromCard(GameObject cardObj)
    {
        if (cardObj == null) return false;
        var cardUI = cardObj.GetComponent<CardUI>();
        if (cardUI == null || cardUI.cardData == null) return false;
        if (!cardUI.isFront) return false;
        if (cardUI.cardData.cardType != CardType.Spell) return false;

        var spell = cardUI.cardData as SpellCardData;
        if (spell == null) return false;
        if (spell.spellType == SpellType.Field) return false; // 필드 마법은 FieldSpellZone에서 처리

        // 코스트 체크(적)
        int cost = spell.cost;
        if (cardUI.ownerType == OwnerType.Opponent)
        {
            if (!GameManager.Instance.SpendEnemyCost(cost))
            {
                Debug.Log($"[AI] 적 코스트 부족: 필요 {cost}, 현재 {GameManager.Instance.enemyCurrentCost}");
                return false;
            }
        }
        else if (cardUI.ownerType == OwnerType.Player)
        {
            if (!GameManager.Instance.TrySpendPlayerCost(cost))
            {
                Debug.Log($"[AI] 플레이어 코스트 부족");
                return false;
            }
        }

        Transform target = targetAnchor != null ? targetAnchor : transform;
        cardObj.transform.SetParent(target, true);
        Vector3 targetPos = target.position;

        var layout = cardObj.GetComponent<LayoutElement>();
        if (layout != null) layout.ignoreLayout = true;

        cardObj.transform.DOKill();
        cardObj.transform.DOMove(targetPos, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() =>
            {
                var rt = cardObj.transform as RectTransform;
                if (rt != null) rt.anchoredPosition3D = Vector3.zero; else cardObj.transform.localPosition = Vector3.zero;

                cardUI.SetFace(true);
                cardUI.isOnField = true;
                cardUI.EnableCardFlip = false;

                // 손패 레이아웃 갱신(소유자 구분)
                if (cardUI.ownerType == OwnerType.Player)
                    PlayerCardManager.Instance?.UpdateHandLayout();
                else
                    OpponentCardManager.Instance?.UpdateHandLayout();

                // FildMonster 통해 효과 실행
                var fm = cardObj.GetComponent<FildMonster>();
                if (fm != null)
                {
                    if (spell.spellType == SpellType.Continuous)
                        fm.ActivateContinuousSpell(spell);
                    else
                    {
                        Debug.Log($"[SpellPlayTarget.AI] 즉시 발동 경로: {spell.cardName}, targetType={spell.cardAbility?.targetType}");
                        fm.ActivateSpellEffect(spell);
                        Object.Destroy(cardObj);
                    }
                }
                else
                {
                    if (spell.spellType != SpellType.Continuous)
                        Object.Destroy(cardObj);
                }
            });

        return true;
    }
}
