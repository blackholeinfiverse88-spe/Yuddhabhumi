using UnityEngine;

/// <summary>
/// TEMP: deterministic fallback until Roshan's behavior trace provider is integrated.
/// Provides fixed traces to prove karma mapping + determinism.
/// </summary>
public class MockBehaviorTraceSource : MonoBehaviour, IBehaviorTraceSource
{
    [Header("Identity")]
    [SerializeField] private string traceId = "mock_trace";
    [SerializeField] private int turnId = 1;

    [Header("Select which fixed trace to output")]
    [SerializeField] private MockPreset preset = MockPreset.ImpulsiveForce;

    public enum MockPreset
    {
        ImpulsiveForce,      // triggers aggression+risk rule
        DisciplinedControl,  // triggers patience+consistency rule
        Rigidity,            // triggers low adaptability rule
        Balanced             // triggers default
    }

    public BehaviorTrace GetFinalTrace()
    {
        switch (preset)
        {
            case MockPreset.ImpulsiveForce:
                return new BehaviorTrace
                {
                    aggression = 0.9f,
                    risk_taking = 0.85f,
                    patience = 0.2f,
                    consistency = 0.3f,
                    adaptability = 0.6f,
                    foresight = 0.3f
                };

            case MockPreset.DisciplinedControl:
                return new BehaviorTrace
                {
                    aggression = 0.3f,
                    risk_taking = 0.25f,
                    patience = 0.85f,
                    consistency = 0.8f,
                    adaptability = 0.7f,
                    foresight = 0.8f
                };

            case MockPreset.Rigidity:
                return new BehaviorTrace
                {
                    aggression = 0.5f,
                    risk_taking = 0.4f,
                    patience = 0.5f,
                    consistency = 0.5f,
                    adaptability = 0.2f,
                    foresight = 0.6f
                };

            default: // Balanced
                return new BehaviorTrace
                {
                    aggression = 0.5f,
                    risk_taking = 0.5f,
                    patience = 0.5f,
                    consistency = 0.5f,
                    adaptability = 0.5f,
                    foresight = 0.5f
                };
        }
    }

    public string GetTraceId() => traceId + "_" + preset.ToString();
    public int GetTurnId() => turnId;
}