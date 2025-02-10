using Core.DI;
using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;

namespace _10CustomRoute;

// Flag our mod as a type of static router
[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class CustomStaticRouter : StaticRouter
{
    public CustomStaticRouter(
        JsonUtil jsonUtil) : base(
        jsonUtil,
        // Add an array of routes we want to add
        GetCustomRoutes()
    )
    {
    }

    private static List<RouteAction> GetCustomRoutes()
    {
        return
        [
            new RouteAction(
                "/example/route/static",
                (
                    url,
                    info,
                    sessionId,
                    output
                ) => HandleRoute(url, info as ExampleStaticRequestData, sessionId)
            )
        ];
    }

    private static string HandleRoute(string url, ExampleStaticRequestData info, string sessionId)
    {
        // Stuff goes here

        return string.Empty;
    }
}
public class ExampleStaticRequestData : IRequestData
{
}
