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
        if (cardUI.ownerType != OwnerType.Player) return; // 내 카드만
        if (cardUI.cardData.cardType != CardType.Spell) return; // 마법만

        var spell = cardUI.cardData as SpellCardData;
        if (spell == null) return;
        // 필드 마법은 제외 (전용 존에서 처리)
        if (spell.spellType == SpellType.Field) return;

        // 애니메이션 타겟 및 부모 지정 (FieldSpellZone 패턴과 유사)
        Transform target = targetAnchor != null ? targetAnchor : transform;
        selected.transform.SetParent(target, true);
        Vector3 targetPos = target.position;

        // 트윈 시작 전 레이아웃 간섭 방지
        var layout = selected.GetComponent<LayoutElement>();
        if (layout != null) layout.ignoreLayout = true;

        // 부모를 잠시 최상위로 올려 연출 안정화(필요 시)
        // 그대로 두어도 되지만, 캔버스 레이어가 다르면 어긋날 수 있어 선택지 제공
        // selected.transform.SetParent(target.root, true);

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
}
