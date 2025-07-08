using TMPro;
using UnityEngine;

public class InGameUIManager : MonoBehaviour
{
    [Header("패널")]
    public GameObject actionLogPanel;
    public GameObject settingsPanel;

    [Header("로그 UI")]
    public TextMeshProUGUI actionLogText;

    private string logHistory = "";

    public void ToggleActionLogPanel()
    {
        actionLogPanel.SetActive(!actionLogPanel.activeSelf);
    }

    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void LogAction(string message)
    {
        logHistory += message + "\n";
        actionLogText.text = logHistory;

        Debug.Log("[LOG] " + message);
    }
}
