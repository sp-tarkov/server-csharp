namespace Core.DI;

public interface OnUpdate
{
    Task<bool> OnUpdate(long timeSinceLastRun);
    string GetRoute();
}