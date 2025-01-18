using Core.Annotations;
using Core.Context;
using Core.DI;
using Core.Servers;

namespace Core.Callbacks;

[Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.HttpCallbacks)]
public class HttpCallbacks(HttpServer _httpServer, ApplicationContext _applicationContext) : OnLoad
{
    public Task OnLoad()
    {
        _httpServer.Load( _applicationContext.GetLatestValue(ContextVariableType.APP_BUILDER)?.GetValue<WebApplicationBuilder>());
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
