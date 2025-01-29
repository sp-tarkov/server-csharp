using SptCommon.Annotations;

namespace Core.Services.Image;

[Injectable(InjectionType.Singleton)]
public class ImageRouterService
{
    protected Dictionary<string, string> routes = new();

    public void AddRoute(string urlKey, string route)
    {
        routes[urlKey] = route;
    }

    public string GetByKey(string urlKey)
    {
        return routes[urlKey];
    }

    public bool ExistsByKey(string urlKey)
    {
        return routes.ContainsKey(urlKey);
    }
}
