using System;
using UnityEngine;

/// <summary>
/// 플레이어가 마우스로 대상 카드를 클릭해서 선택하도록 돕는 컴포넌트.
/// 싱글톤 패턴 사용.
/// </summary>
public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance;

    private Action<GameObject> onTargetSelected;
    private bool selecting = false;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 대상 선택 시작, 완료 시 콜백 호출
    /// </summary>
    public void StartSelecting(Action<GameObject> callback)
    {
        selecting = true;
        onTargetSelected = callback;
    }

    private void Update()
    {
        if (!selecting) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject target = hit.collider.gameObject;

                // TargetableCard 컴포넌트가 붙은 오브젝트만 선택 가능
                if (target.GetComponent<TargetableCard>() != null)
                {
                    selecting = false;
                    onTargetSelected?.Invoke(target);
                }
            }
        }
    }
}
