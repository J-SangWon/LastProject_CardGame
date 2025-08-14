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
        // 필드/지속 마법은 제외 (요청사항)
        if (spell.spellType == SpellType.Field) return;
        if (spell.spellType == SpellType.Continuous) return;

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
                    fm.ActivateSpellEffect(spell);
                }

                // 효과 실행 후 제거(묘지 시스템이 있다면 교체)
                // 사라지게 처리(묘지 시스템이 있다면 그쪽으로 이동시키도록 교체 가능)
                Object.Destroy(selected);
            });
    }
}
