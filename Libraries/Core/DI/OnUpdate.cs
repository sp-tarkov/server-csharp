namespace Core.DI;

public interface OnUpdate
{
    bool OnUpdate(long timeSinceLastRun);
    string GetRoute();
}
