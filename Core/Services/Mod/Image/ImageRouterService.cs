using Core.Annotations;

namespace Core.Services.Mod.Image;

[Injectable(InjectionType.Singleton)]
public class ImageRouterService
{
    protected Dictionary<string, string> routes = new();

    public void addRoute(string urlKey, string route)
    {
        routes[urlKey] = route;
    }

    public string getByKey(string urlKey)
    {
        return routes[urlKey];
    }

    public bool ExistsByKey(string urlKey)
    {
        return routes.ContainsKey(urlKey);
    }
}
