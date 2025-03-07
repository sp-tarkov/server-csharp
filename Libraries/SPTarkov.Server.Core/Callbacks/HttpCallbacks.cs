using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.HttpCallbacks)]
public class HttpCallbacks(HttpServer _httpServer, ApplicationContext _applicationContext) : IOnLoad
{
    public Task OnLoad()
    {
        _httpServer.Load(_applicationContext.GetLatestValue(ContextVariableType.APP_BUILDER)?.GetValue<WebApplicationBuilder>());
        _applicationContext.ClearValues(ContextVariableType.APP_BUILDER);

        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-http";
    }

    public string GetImage()
    {
        return "";
    }
}
