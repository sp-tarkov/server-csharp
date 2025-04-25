namespace SPTarkov.Server.Core.DI;

public abstract class Router
{
    protected List<HandledRoute> handledRoutes = [];

    public virtual string GetTopLevelRoute()
    {
        return "spt";
    }

    protected abstract List<HandledRoute> GetHandledRoutes();

    protected List<HandledRoute> GetInternalHandledRoutes()
    {
        if (handledRoutes.Count == 0)
        {
            handledRoutes = GetHandledRoutes();
        }

        return handledRoutes;
    }

    public bool CanHandle(string url, bool partialMatch = false)
    {
        if (partialMatch)
        {
            return GetInternalHandledRoutes()
                .Where(r => r.dynamic)
                .Any(r => url.Contains(r.route));
        }

        return GetInternalHandledRoutes()
            .Where(r => !r.dynamic)
            .Any(r => r.route == url);
    }
}
