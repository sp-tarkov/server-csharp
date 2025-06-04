namespace SPTarkov.Server.Core.DI;

public interface IOnUpdate
{
    Task OnUpdate(long timeSinceLastRun);
}
