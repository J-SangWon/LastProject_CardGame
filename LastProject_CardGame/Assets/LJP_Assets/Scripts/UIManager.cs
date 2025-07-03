using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : SingletonBehaviour<UIManager>
{
	[SerializeField] private Image fadeImage;

	[Header("Fade In/Out Setting")]
	[SerializeField] float fadeDuration = 1f;

	private Coroutine currentFade;
	private enum FadeDirection { In, Out }

	public void FadeIn()
	{
		StartFade(FadeDirection.In);
	}

	public void FadeOut()
	{
		StartFade(FadeDirection.Out);
	}

	private void StartFade(FadeDirection direction)
	{
		if (currentFade != null)
		{
			StopCoroutine(currentFade);
		}

		currentFade = StartCoroutine(FadeRoutine(direction));
	}

	private IEnumerator FadeRoutine(FadeDirection direction)
	{
		float time = 0f;
		Color color = fadeImage.color;

		float startAlpha = (direction == FadeDirection.In) ? 1f : 0f;
		float endAlpha = (direction == FadeDirection.In) ? 0f : 1f;

		color.a = startAlpha;
		fadeImage.color = color;

		while (time < fadeDuration)
		{
			time += Time.deltaTime;
			float t = Mathf.Clamp01(time / fadeDuration);
			color.a = Mathf.Lerp(startAlpha, endAlpha, t);
			fadeImage.color = color;
			yield return null;
		}

		color.a = endAlpha;
		fadeImage.color = color;
		currentFade = null;
	}
}
