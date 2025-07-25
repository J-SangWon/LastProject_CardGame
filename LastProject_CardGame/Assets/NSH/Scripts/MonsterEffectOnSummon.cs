using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 몬스터 카드가 소환될 때 발동하는 효과 스크립트.
/// 대상이 되는 'Enemy' 태그가 붙은 오브젝트를 클릭하면 효과가 적용됨.
/// </summary>
public class MonsterEffectOnSummon : MonoBehaviour
{
    private bool effectActivated = false;   // 효과가 이미 발동되었는지 여부
    private bool waitingForTarget = false;  // 대상 클릭을 기다리는 상태인지 여부

    public CardManager_test cardManager;    // 카드 드로우 기능을 담당하는 매니저
    public GraphicRaycaster raycaster;      // UI 요소에 대한 레이캐스트를 위한 컴포넌트 (Canvas에 있어야 함)
    public EventSystem eventSystem;         // 마우스 입력 처리를 위한 EventSystem (씬에 있어야 함)

    void Start()
    {
        // GraphicRaycaster가 설정되지 않았다면 자동으로 씬에서 찾아서 할당
        if (raycaster == null)
            raycaster = Object.FindFirstObjectByType<GraphicRaycaster>();

        // EventSystem이 설정되지 않았다면 자동으로 씬에서 찾아서 할당
        if (eventSystem == null)
            eventSystem = Object.FindFirstObjectByType<EventSystem>();
    }

    /// <summary>
    /// 몬스터 카드가 소환될 때 호출되는 함수.
    /// 효과 발동을 알리고 대상 클릭을 기다리는 상태로 진입.
    /// </summary>
    public void OnSummon()
    {
        // 효과가 이미 발동되었다면 추가 발동을 막음
        if (effectActivated) return;

        Debug.Log($"{gameObject.name}의 소환 효과 발동: 대상을 선택하세요!");
        waitingForTarget = true;  // 대상 클릭을 기다리는 상태로 진입
    }

    void Update()
    {
        // 대상 선택 대기 중이고 마우스 왼쪽 클릭 시 대상 지정 함수 호출
        if (waitingForTarget && Input.GetMouseButtonDown(0))
        {
            // TrySelectTarget() 함수 호출
            // 이 함수는 마우스를 클릭한 위치를 레이캐스트로 확인하고, 
            // 'Enemy' 태그가 붙은 오브젝트를 클릭했을 때 해당 오브젝트에 대한 효과를 발동함.
            TrySelectTarget();
        }
    }

    /// <summary>
    /// 마우스 클릭 위치에 대해 UI Raycast를 실행하고,
    /// 'Enemy' 태그가 붙은 대상을 선택하여 효과를 적용하는 함수.
    /// </summary>
    private void TrySelectTarget()
    {
        // 마우스 위치를 이용해 Raycast 결과를 얻기 위한 데이터 설정
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        // Raycast 결과를 저장할 리스트
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);  // Raycast 실행

        // 클릭된 UI 요소들 중에서 적('Enemy')을 찾아 처리
        foreach (var result in results)
        {
            GameObject target = result.gameObject;

            // 클릭한 대상이 'Enemy' 태그가 붙은 오브젝트인 경우
            if (target.CompareTag("Enemy"))
            {
                // 적을 파괴하는 함수 호출
                DestroyTarget(target);

                // 카드 드로우 처리
                if (cardManager != null)
                {
                    Debug.Log($"{gameObject.name}의 효과로 카드를 드로우합니다.");
                    cardManager.DrawCard();  // 카드 1장 드로우
                }
                else
                {
                    Debug.LogWarning("CardManager가 설정되지 않았습니다.");
                }

                // 효과 발동 완료
                effectActivated = true;
                waitingForTarget = false;  // 대상 선택 대기 상태 종료
                break;  // 첫 번째 적을 찾으면 반복문 종료
            }
            else
            {
                // 선택된 객체가 'Enemy'가 아닌 경우
                Debug.Log($"선택된 객체는 'Enemy'가 아닙니다: {target.name}");
            }
        }
    }

    /// <summary>
    /// 선택된 대상 오브젝트를 파괴하는 함수.
    /// </summary>
    private void DestroyTarget(GameObject target)
    {
        Debug.Log($"{target.name}이(가) 파괴되었습니다!");
        Destroy(target);  // 대상 오브젝트를 파괴
    }
}
