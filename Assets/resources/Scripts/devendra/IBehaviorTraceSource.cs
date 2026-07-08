public interface IBehaviorTraceSource
{
    string GetTraceId();
    BehaviorTrace GetCurrentTrace();
}