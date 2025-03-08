namespace SPTarkov.Server.Core.DI;

public interface IOnUpdate
{
    bool OnUpdate(long timeSinceLastRun);
    string GetRoute();
}
