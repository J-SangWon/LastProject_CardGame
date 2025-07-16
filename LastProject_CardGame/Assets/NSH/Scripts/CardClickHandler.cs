using UnityEngine;
using UnityEngine.EventSystems;

public class CardClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Transform handZone;

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.SetParent(handZone, false);
        transform.localScale = Vector3.one;
        transform.SetAsLastSibling();
    }
}
