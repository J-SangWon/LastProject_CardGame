using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class CardThumbnail : MonoBehaviour, IPointerClickHandler
{
    public Image artworkImage;
    public TMP_Text countText;
    private Animator anim;
    AnimatorStateInfo stateInfo;

    public BaseCardData cardData;

    private Action<BaseCardData> rightClickAction;

    void Awake()
    {
        anim = GetComponent<Animator>();
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);
    }
    public void SetRightClickAction(Action<BaseCardData> action)
    {
        rightClickAction = action;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 우클릭 처리
            rightClickAction?.Invoke(cardData);
        }
    }

    public void PlayInOutAnimation()
    {
        if (anim != null)
            anim.SetTrigger("InOut");
    }
    // 카드가 이동하듯이 들어가는 애니메이션
    public void PlayMoveInAnimation(Vector2 startPos, Vector2 endPos)
    {
        float duration = stateInfo.length;
        StartCoroutine(MoveInCoroutine(startPos, endPos, duration));
    }

    private IEnumerator MoveInCoroutine(Vector2 startPos, Vector2 endPos, float duration)
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = startPos;
        float time = 0f;
        PlayInOutAnimation();
        while (time < duration)
        {
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }

    public void SetCard(BaseCardData card, int count)
    {
        cardData = card;
        artworkImage.sprite = card.artwork;
        countText.text = count.ToString();
    }

    public void SetCard(BaseCardData card, int owned, int available)
    {
        cardData = card;
        artworkImage.sprite = card.artwork;
        countText.text = $"{available}";
    }

    public void SetUnavailableVisual()
    {
        var img = GetComponent<Image>();
        if (img != null) img.color = new Color(0.7f, 0.7f, 0.7f, 0.5f); // 회색+반투명
        if (countText != null) countText.color = Color.gray;
    }
}
