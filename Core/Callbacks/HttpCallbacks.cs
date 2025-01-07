using Core.Annotations;
using Core.Context;
using Core.DI;
using Core.Servers;

namespace Core.Callbacks;

[Injectable(InjectionType.Singleton, typePriority: 1)]
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
        _httpServer.Load((WebApplicationBuilder) _applicationContext.GetLatestValue(ContextVariableType.APP_BUILDER).Value);
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