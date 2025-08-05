using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExtraDeckListPanel : MonoBehaviour
{
    public Transform contentRoot;
    public GameObject cardPrefab;
    public Button closeButton;

    public void Show(List<DeckCardEntry> entries)
    {
        gameObject.SetActive(true);

        // 기존 썸네일 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        // 카드 개수만큼 썸네일 생성
        foreach (var entry in entries)
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
        gameObject.SetActive(false);
    }

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }
}