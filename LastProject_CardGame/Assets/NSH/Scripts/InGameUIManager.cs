using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    [Header("UI 참조")]
    public TMP_Text logText;
    public TMP_Text timerText;
    public GameObject settingsPanel;

    private string logHistory = "";
    private const int maxLogLines = 10;

    public void LogAction(string text)
    {
        logHistory += $"\u25B6 {text}\n";

        // 로그가 너무 길면 줄 수 제한
        string[] lines = logHistory.Split('\n');
        if (lines.Length > maxLogLines)
        {
            logHistory = string.Join("\n", lines, lines.Length - maxLogLines, maxLogLines);
        }

        if (logText != null)
            logText.text = logHistory;
    }

    public void UpdateTurnTimer(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60);

        if (timerText != null)
            timerText.text = $"{minutes:00}:{secs:00}";
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
        }
    }
}
