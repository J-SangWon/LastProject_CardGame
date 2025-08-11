using UnityEngine;
using UnityEngine.EventSystems;

public class HandCard : MonoBehaviour, IPointerClickHandler
{
    public bool isInHand = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInHand) return;

        CardSummonManager.Instance.SelectCard(this.gameObject);
        Debug.Log("Ä«µå ¼±ÅÃµÊ: " + gameObject.name);
    }
}
