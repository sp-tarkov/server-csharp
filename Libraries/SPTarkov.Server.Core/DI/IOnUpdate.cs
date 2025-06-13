namespace SPTarkov.Server.Core.DI;

public interface IOnUpdate
{
    Task<bool> OnUpdate(long secondsSinceLastRun);
}
