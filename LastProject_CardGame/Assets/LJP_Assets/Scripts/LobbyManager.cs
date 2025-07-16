using System.Collections;
using UnityEngine;

public enum LobbyType
{
    Lobby,
    DeckSelect,
    DeckEdit,
    Shop,
    MyPage,
	Background
}

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
	private LobbyType currentLobbyType;
	private bool isTransitioning = false;

	[SerializeField] private float transitionDelay = 1f;

	public void ChangeLobbyType(LobbyType targetType)
	{
		if (isTransitioning || currentLobbyType == targetType)
			return;

		isTransitioning = true;
		currentLobbyType = targetType;

		UIManager.Instance.ShowLobby(targetType, transitionDelay, () =>
		{
			isTransitioning = false;
		});
	}
}
