using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("카드 클릭됨: " + gameObject.name);
        // 원하는 로직 추가 (공격, 타겟 선택 등)
    }
}
