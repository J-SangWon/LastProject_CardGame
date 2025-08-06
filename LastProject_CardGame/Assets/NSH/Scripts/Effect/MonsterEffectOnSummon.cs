using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MonsterEffectOnSummon : MonoBehaviour
{
    private CardEffectType effectType;
    private int effectValue;
    private PlayerController_N ownerPlayer;

    // 누락된 public 필드 선언
    public BaseCardData cardData;
    public PlayerCardManager cardManager;

    public void SetEffect(CardEffectType effect, int value, PlayerController_N owner)
    {
        effectType = effect;
        effectValue = value;
        ownerPlayer = owner;

        TryActivateEffect();
    }

    public void OnSummon()
    {
        // 카드 소환 시 실행할 효과 예시
        SetEffect(CardEffectType.DealDamageToTargetOnSummon, 3, /*플레이어 전달*/ null);
    }


    private void TryActivateEffect()
    {
        if (effectType == CardEffectType.DealDamageToTargetOnSummon)
        {
            StartCoroutine(WaitForTargetSelection());
        }
    }

    private IEnumerator WaitForTargetSelection()
    {
        Debug.Log("대상을 클릭하여 효과를 발동하세요.");

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) yield return null;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    TargetableCard target = hit.collider.GetComponent<TargetableCard>();
                    if (target != null)
                    {
                        target.TakeDamage(effectValue);
                        Debug.Log("데미지를 " + effectValue + "만큼 입혔습니다.");
                        break;
                    }
                }
            }

            yield return null;
        }
    }
}
