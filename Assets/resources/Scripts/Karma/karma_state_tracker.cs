using UnityEngine;

/// <summary>
/// Passive match-level karma tracker.
/// Observes gameplay events and maintains a running karma score.
/// Does NOT modify gameplay, stats, or combat behavior.
/// </summary>
public class KarmaStateTracker : MonoBehaviour
{
    public static KarmaStateTracker Instance { get; private set; }

    [Header("Karma Thresholds")]
    public int positiveThreshold = 10;
    public int negativeThreshold = -10;

    [Header("Karma Weights")]
    public int balancedDeploymentWeight = 2;
    public int defensivePatienceWeight = 1;
    public int offensivePlayWeight = 1;
    public int overAggressionWeight = -2;
    public int wastefulElixirWeight = -3;

    private int matchKarmaScore = 0;
    private int consecutiveOffensivePlays = 0;
    private float lastElixirValue = 0f;

    public int CurrentKarma => matchKarmaScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ✅ ADD THIS METHOD
    public void ResetMatchKarma()
    {
        matchKarmaScore = 0;
        consecutiveOffensivePlays = 0;
        lastElixirValue = 0f;

        Debug.Log("[KARMA] Match karma reset.");
    }

    // PUBLIC OBSERVATION METHODS

    public void OnCardPlayed(bool isOffensive, float currentElixir)
    {
        Debug.Log("OnCardPlayed triggered");

        if (isOffensive)
        {
            ApplyKarma(offensivePlayWeight, "Offensive Play");
        }

        EvaluateAggression(isOffensive);
        EvaluateElixirUsage(currentElixir);
    }

    public void OnDefensiveAction()
    {
        ApplyKarma(defensivePatienceWeight, "Defensive Patience");
        consecutiveOffensivePlays = 0;
    }

    public void OnBalancedPlay()
    {
        ApplyKarma(balancedDeploymentWeight, "Balanced Deployment");
        consecutiveOffensivePlays = 0;
    }

    public void OnWastefulElixir()
    {
        ApplyKarma(wastefulElixirWeight, "Wasteful Elixir Usage");
    }

    // INTERNAL LOGIC

    private void EvaluateAggression(bool isOffensive)
    {
        if (isOffensive)
        {
            consecutiveOffensivePlays++;

            if (consecutiveOffensivePlays >= 3)
            {
                ApplyKarma(overAggressionWeight, "Over-Aggression");
                consecutiveOffensivePlays = 0;
            }
        }
        else
        {
            consecutiveOffensivePlays = 0;
        }
    }

    private void EvaluateElixirUsage(float currentElixir)
    {
        if (currentElixir <= 0.5f && lastElixirValue <= 0.5f)
        {
            ApplyKarma(overAggressionWeight, "Low Elixir Risk");
        }

        lastElixirValue = currentElixir;
    }

    private void ApplyKarma(int value, string reason)
    {
        matchKarmaScore += value;
        Debug.Log($"[KARMA] {reason} | Delta: {value} | Total: {matchKarmaScore}");
    }

    // FINAL STATE

    public string GetFinalKarmaState()
    {
        if (matchKarmaScore >= positiveThreshold)
            return "Positive";

        if (matchKarmaScore <= negativeThreshold)
            return "Negative";

        return "Neutral";
    }
}