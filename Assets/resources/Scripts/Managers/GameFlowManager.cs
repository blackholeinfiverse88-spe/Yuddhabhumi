using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    [Header("Progression")]
    public int playerLevel = 1;
    public float currentXP = 0f;
    public float xpToNextLevel = 500f;
    public float difficultyMultiplier = 1.2f;

    [Header("Match Result")]
    public bool lastMatchWon;

    [Header("Match Stats")]
    public float totalDamageDealt;
    public float totalEnergySpent;
    public float matchDuration;

    [Header("Post Match Output")]
    public string finalKarmaType;
    public string finalStrategyType;
    public string finalMistake;
    public string finalSuggestion;

    private bool isMatchActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isMatchActive)
        {
            matchDuration += Time.deltaTime;
        }
    }

    public void StartMatch()
    {
        totalDamageDealt = 0f;
        totalEnergySpent = 0f;
        matchDuration = 0f;
        lastMatchWon = false;

        finalKarmaType = "";
        finalStrategyType = "";
        finalMistake = "";
        finalSuggestion = "";

        isMatchActive = true;
    }

    public void EndMatch(bool won)
    {
        isMatchActive = false;
        lastMatchWon = won;

        float xpReward = won ? 90f : 50f;
        GrantXP(xpReward);

        if (KarmaStateTracker.Instance != null)
            finalKarmaType = KarmaStateTracker.Instance.GetFinalKarmaState();
        else
            finalKarmaType = "Neutral";

        finalStrategyType = GetStrategyType();
        GeneratePostMatchFeedback();

        Debug.Log("=== POST MATCH SUMMARY ===");
        Debug.Log("Karma Type: " + finalKarmaType);
        Debug.Log("Strategy Type: " + finalStrategyType);
        Debug.Log("Mistake: " + finalMistake);
        Debug.Log("Suggestion: " + finalSuggestion);
    }

    private string GetStrategyType()
    {
        if (finalKarmaType == "Positive")
            return "Disciplined Commander";

        if (finalKarmaType == "Negative")
            return "Aggressive Overcommitter";

        return "Balanced Tactician";
    }

    private void GeneratePostMatchFeedback()
    {
        switch (finalStrategyType)
        {
            case "Disciplined Commander":
                finalMistake = "No major decision errors detected.";
                finalSuggestion = "Experiment with slightly earlier engagement next match.";
                break;

            case "Aggressive Overcommitter":
                finalMistake = "You committed heavily early and lost control mid-phase.";
                finalSuggestion = "Try a slower opening next match.";
                break;

            default:
                finalMistake = "Your pacing remained stable.";
                finalSuggestion = "Test a different opening tempo next match.";
                break;
        }
    }

    public void AddDamage(float amount)
    {
        totalDamageDealt += amount;
    }

    public void AddEnergySpent(float amount)
    {
        totalEnergySpent += amount;
    }

    public void GrantXP(float amount)
    {
        currentXP += amount;

        if (currentXP >= xpToNextLevel)
            LevelUp();
    }

    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        playerLevel++;
        xpToNextLevel *= difficultyMultiplier;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        Time.timeScale = 1f;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;
    }
}