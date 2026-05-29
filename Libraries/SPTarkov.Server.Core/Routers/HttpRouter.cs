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

    public async ValueTask<string?> GetResponseAsync(
        HttpRequest req,
        MongoId sessionID,
        string? body,
        CancellationToken cancellationToken = default
    )
    {
        var wrapper = new ResponseWrapper("");
        var handled = await HandleRouteAsync(req, sessionID, wrapper, staticRouters, false, body, cancellationToken);
        if (!handled)
        {
            await HandleRouteAsync(req, sessionID, wrapper, dynamicRoutes, true, body, cancellationToken);
        }

        return wrapper.Output;
    }

    protected async ValueTask<bool> HandleRouteAsync(
        HttpRequest request,
        MongoId sessionID,
        ResponseWrapper wrapper,
        IEnumerable<Router> routers,
        bool dynamic,
        string? body,
        CancellationToken cancellationToken = default
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
                    wrapper.Output =
                        await (route as DynamicRouter).HandleDynamicAsync(url, body, sessionID, wrapper.Output, cancellationToken)
                        as string;
                }
                else
                {
                    wrapper.Output =
                        await (route as StaticRouter).HandleStaticAsync(url, body, sessionID, wrapper.Output, cancellationToken) as string;
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
