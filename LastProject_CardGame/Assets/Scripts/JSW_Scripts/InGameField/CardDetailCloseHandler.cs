using UnityEngine;
using UnityEngine.EventSystems;

public class CardDetailCloseHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 좌클릭
        {
            if (InGameCardDetailPanel.Instance != null && InGameCardDetailPanel.Instance.IsOpen)
            {
                InGameCardDetailPanel.Instance.Hide();
            }
        }
    }
}
