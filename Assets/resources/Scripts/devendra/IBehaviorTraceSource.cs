public interface IBehaviorTraceSource
{
    BehaviorTrace GetFinalTrace();
    string GetTraceId();
    int GetTurnId();
}