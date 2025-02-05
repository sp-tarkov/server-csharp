using SptCommon.Annotations;
using Core.DI;

namespace Core.Routers;

[Injectable]
public class HttpRouter
{
    protected IEnumerable<StaticRouter> _staticRouters;
    protected IEnumerable<DynamicRouter> _dynamicRoutes;

    public HttpRouter(
        IEnumerable<StaticRouter> staticRouters,
        IEnumerable<DynamicRouter> dynamicRoutes
    )
    {
        _staticRouters = staticRouters;
        _dynamicRoutes = dynamicRoutes;
    }

    /*
    protected groupBy<T>(list: T[], keyGetter: (t: T) => string): Map<string, T[]> {
        const map: Map<string, T[]> = new Map();
        for (const item of list) {
            const key = keyGetter(item);
            const collection = map.get(key);
            if (!collection) {
                map.set(key, [item]);
            } else {
                collection.push(item);
            }
        }
        return map;
    }
    */

    public string? GetResponse(HttpRequest req, string sessionID, string? body, out object deserializedObject)
    {
        var wrapper = new ResponseWrapper("");

        var handled = HandleRoute(req, sessionID, wrapper, _staticRouters, false, body, out deserializedObject);
        if (!handled) HandleRoute(req, sessionID, wrapper, _dynamicRoutes, true, body, out deserializedObject);

        // TODO: Temporary hack to change ItemEventRouter response sessionID binding to what client expects
        if (wrapper.Output?.Contains("\"profileChanges\":{") ?? false) wrapper.Output = wrapper.Output.Replace(sessionID, sessionID);

        //var filepath = $"c:\\SharpServer\\{req.Path.ToString().Substring(1).Replace("/", ".")}.json";
        //File.WriteAllText(filepath, wrapper.Output);

        return wrapper.Output;
    }

    protected bool HandleRoute(
        HttpRequest request,
        string sessionID,
        ResponseWrapper wrapper,
        IEnumerable<Router> routers,
        bool dynamic,
        string? body,
        out object deserializedObject
    )
    {
        var url = request.Path.Value;
        deserializedObject = null;

        // remove retry from url
        if (url?.Contains("?retry=") ?? false) url = url.Split("?retry=")[0];
        var matched = false;
        foreach (var route in routers)
            if (route.CanHandle(url, dynamic))
            {
                if (dynamic)
                    wrapper.Output = (route as DynamicRouter).HandleDynamic(url, body, sessionID, wrapper.Output) as string;
                else
                    wrapper.Output = (route as StaticRouter).HandleStatic(url, body, sessionID, wrapper.Output) as string;
                matched = true;
            }

        return matched;
    }

    protected class ResponseWrapper(string? output)
    {
        public string? Output { get; set; } = output;
    }
}
