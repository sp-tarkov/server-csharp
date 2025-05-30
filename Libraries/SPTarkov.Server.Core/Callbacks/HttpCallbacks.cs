using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Callbacks;
[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.HttpCallbacks)]
public class HttpCallbacks(HttpServer _httpServer) : IOnLoad
{
    public Task OnLoad()
    {
        _httpServer.Load();

        return Task.CompletedTask;
    }

    public string GetImage()
    {
        return "";
    }
}
