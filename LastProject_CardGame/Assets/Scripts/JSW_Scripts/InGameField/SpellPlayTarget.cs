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
                        // 일반/속공: 발동 후 제거
                        fm.ActivateSpellEffect(spell);
                        Object.Destroy(selected);
                    }
                }
                else
                {
                    // FildMonster가 없으면 안전하게 제거 처리(연결 누락 대비)
                    if (spell.spellType != SpellType.Continuous)
                        Object.Destroy(selected);
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
