using UnityEngine;
using TMPro;

public class ResultUIManager : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI karmaSummaryText;   // NEW - shows only Karma
    public TextMeshProUGUI damageText;         // NOW shows Mistake
    public TextMeshProUGUI energyText;         // NOW shows Suggestion
    public TextMeshProUGUI timeText;           // stays Time Survived

    public GameObject playAgainButton;
    public GameObject nextLevelButton;
    public GameObject exitButton;

    void Start()
    {
        if (!GameFlowManager.Instance)
            return;

        bool won = GameFlowManager.Instance.lastMatchWon;

        // Win / Loss
        if (resultText != null)
            resultText.text = won ? "WIN!" : "LOSE!";

        // Karma (separate small box)
        if (karmaSummaryText != null)
        {
            string karma = string.IsNullOrEmpty(GameFlowManager.Instance.finalKarmaType)
                ? "Neutral"
                : GameFlowManager.Instance.finalKarmaType;

            karmaSummaryText.text = $"Karma: {karma}";
        }

        // Mistake (replaces Damage Dealt)
        if (damageText != null)
        {
            string mistake = string.IsNullOrEmpty(GameFlowManager.Instance.finalMistake)
                ? "No major mistakes recorded."
                : GameFlowManager.Instance.finalMistake;

            damageText.text = $"Mistake: {mistake}";
        }

        // Suggestion (replaces Energy Spent)
        if (energyText != null)
        {
            string suggestion = string.IsNullOrEmpty(GameFlowManager.Instance.finalSuggestion)
                ? "Keep refining your pacing next match."
                : GameFlowManager.Instance.finalSuggestion;

            energyText.text = $"Suggestion: {suggestion}";
        }

        // Time Survived (unchanged)
        if (timeText != null)
        {
            int t = Mathf.RoundToInt(GameFlowManager.Instance.matchDuration);
            timeText.text = $"Time Survived: {t / 60:00}:{t % 60:00}";
        }

        // Buttons
        if (playAgainButton != null)
            playAgainButton.SetActive(!won);

        if (nextLevelButton != null)
            nextLevelButton.SetActive(won);

        if (exitButton != null)
            exitButton.SetActive(true);
    }
}