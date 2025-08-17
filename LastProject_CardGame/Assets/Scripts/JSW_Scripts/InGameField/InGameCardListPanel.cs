using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InGameCardListPanel : MonoBehaviour
{
    public Transform contentRoot;
    public GameObject cardPrefab;
    public Button closeButton;
    public TextMeshProUGUI titleText;

    public void Show(List<DeckCardEntry> entries, string title, bool reverseOrder = false)
    {
        SoundManager.Instance.PlaySFX("MENUSELEET_01");

        gameObject.SetActive(true);

        // 제목 설정
        if (titleText != null)
            titleText.text = title;

        // 기존 썸네일 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        // 표시할 카드 리스트 결정 (수정된 부분)
        List<DeckCardEntry> displayEntries;
        if (reverseOrder)
        {
            displayEntries = new List<DeckCardEntry>(entries);
            displayEntries.Reverse();
        }
        else
        {
            displayEntries = entries;
        }

        // 카드 개수만큼 썸네일 생성
        foreach (var entry in displayEntries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                GameObject obj = Instantiate(cardPrefab, contentRoot);
                var cardUI = obj.GetComponent<CardUI>();
                if (cardUI != null)
                {
                    cardUI.SetCard(entry.card);
                    cardUI.EnableCardFlip = false;
                }
            }
        }
    }

    public void Hide()
    {
        SoundManager.Instance.PlaySFX("MENUSELEET_01");

        gameObject.SetActive(false);
    }

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }
}