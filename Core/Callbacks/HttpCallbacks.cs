using Core.Annotations;
using Core.Context;
using Core.DI;
using Core.Servers;

namespace Core.Callbacks;

[Injectable(InjectionType.Singleton, InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.HttpCallbacks)]
public class HttpCallbacks : OnLoad
{
    private readonly HttpServer _httpServer;
    private readonly ApplicationContext _applicationContext;
    public HttpCallbacks(HttpServer httpServer, ApplicationContext applicationContext)
    {
        _httpServer = httpServer;
        _applicationContext = applicationContext;
    }
    
    public async Task OnLoad()
    {
        _httpServer.Load( _applicationContext.GetLatestValue(ContextVariableType.APP_BUILDER).GetValue<WebApplicationBuilder>());
        _applicationContext.ClearValues(ContextVariableType.APP_BUILDER);
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
