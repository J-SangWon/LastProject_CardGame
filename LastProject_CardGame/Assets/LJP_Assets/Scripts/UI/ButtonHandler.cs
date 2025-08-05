using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LobbyButtonHandler : MonoBehaviour
{
	[SerializeField] private LobbyType targetLobbyType;

	private void Start()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			LobbyManager.Instance.ChangeLobbyType(targetLobbyType);
		});
	}
}