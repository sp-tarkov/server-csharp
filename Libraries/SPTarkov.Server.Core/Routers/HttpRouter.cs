using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class HttpRouter(IEnumerable<StaticRouter> staticRouters, IEnumerable<DynamicRouter> dynamicRoutes)
{
    public bool CanHandle(HttpContext context)
    {
        return staticRouters.Any(sr => sr.CanHandle(context.Request.Path.Value, false))
            || dynamicRoutes.Any(dr => dr.CanHandle(context.Request.Path.Value, true));
    }

    public async ValueTask<string?> GetResponse(HttpRequest req, MongoId sessionID, string? body)
    {
        var wrapper = new ResponseWrapper("");

        var handled = await HandleRoute(req, sessionID, wrapper, staticRouters, false, body);
        if (!handled)
        {
            await HandleRoute(req, sessionID, wrapper, dynamicRoutes, true, body);
        }

        return wrapper.Output;
    }

    protected async ValueTask<bool> HandleRoute(
        HttpRequest request,
        MongoId sessionID,
        ResponseWrapper wrapper,
        IEnumerable<Router> routers,
        bool dynamic,
        string? body
    )
    {
        var url = request.Path.Value;

        // remove retry from url
        if (url?.Contains("?retry=") ?? false)
        {
            url = url.Split("?retry=")[0];
        }

        var matched = false;
        foreach (var route in routers)
        {
            if (route.CanHandle(url, dynamic))
            {
                if (dynamic)
                {
                    wrapper.Output = await (route as DynamicRouter).HandleDynamic(url, body, sessionID, wrapper.Output) as string;
                }
                else
                {
                    wrapper.Output = await (route as StaticRouter).HandleStatic(url, body, sessionID, wrapper.Output) as string;
                }

                matched = true;
            }
        }

        return matched;
    }

    protected class ResponseWrapper(string? output)
    {
        public string? Output { get; set; } = output;
    }
}
