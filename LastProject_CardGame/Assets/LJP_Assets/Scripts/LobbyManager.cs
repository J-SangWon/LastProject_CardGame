using UnityEngine;

public enum LobbyType
{
    Lobby,
    DeckSelect,
    DeckEdit,
    Shop,
    MyPage
}

public class LobbyManager : SingletonBehaviour<LobbyManager>
{
    public LobbyType LobbyType;

	private void Start()
	{
		LobbyType = LobbyType.Lobby;
	}
}
