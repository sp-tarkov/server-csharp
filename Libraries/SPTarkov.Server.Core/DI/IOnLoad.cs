namespace SPTarkov.Server.Core.DI;

public interface IOnLoad
{
    Task OnLoad();
    string GetRoute();
}
