using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBehaviour<UIManager>
{
	[SerializeField] private GameObject LobbyUI;
	[SerializeField] private GameObject DeckSelectUI;
	[SerializeField] private GameObject DeckEditUI;
	[SerializeField] private GameObject ShopUI;
	[SerializeField] private GameObject MyPageUI;
	[SerializeField] private Image fadeImage;

	[Header("Fade In/Out Setting")]
	[SerializeField] private float fadeDuration = 1f;

	private Coroutine currentFade;
	private Dictionary<LobbyType, GameObject> lobbyUIMap;

	protected override void Awake()
	{
		base.Awake();
		InitLobbyUIMap();
	}

	private void InitLobbyUIMap()
	{
		lobbyUIMap = new Dictionary<LobbyType, GameObject>
		{
			{ LobbyType.Lobby, LobbyUI },
			{ LobbyType.DeckSelect, DeckSelectUI },
			{ LobbyType.DeckEdit, DeckEditUI },
			{ LobbyType.Shop, ShopUI },
			{ LobbyType.MyPage, MyPageUI },
		};
	}

	public void ShowLobby(LobbyType type, float delay, System.Action onComplete = null)
	{
		StartCoroutine(TransitionRoutine(type, delay, onComplete));
	}

	private IEnumerator TransitionRoutine(LobbyType type, float delay, System.Action onComplete)
	{
		yield return Fade(FadeDirection.Out);

		SetAllUI(false);
		if (lobbyUIMap.TryGetValue(type, out var targetUI))
			targetUI.SetActive(true);

		yield return new WaitForSeconds(delay);

		yield return Fade(FadeDirection.In);

		onComplete?.Invoke();
	}

	private enum FadeDirection { In, Out }

	private IEnumerator Fade(FadeDirection direction)
	{
		if (fadeImage == null) yield break;

		fadeImage.gameObject.SetActive(true);

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
		if(direction == FadeDirection.In)
			fadeImage.gameObject.SetActive(false);
		else
			fadeImage.gameObject.SetActive(true);
	}

	private void SetAllUI(bool active)
	{
		foreach (var kvp in lobbyUIMap)
		{
			kvp.Value.SetActive(active);
		}
	}
}