using Core.Annotations;
using Core.Callbacks;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Routers.Static;

[Injectable(InjectableTypeOverride = typeof(StaticRouter))]
public class QuestStaticRouter : StaticRouter
{
    protected static QuestCallbacks _questCallbacks;
    
    public QuestStaticRouter(
        JsonUtil jsonUtil,
        QuestCallbacks questCallbacks
    ) : base(
        jsonUtil,
        [
            new RouteAction(
                "",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _questCallbacks.ListQuests(url, info as EmptyRequestData, sessionID)),
            new RouteAction(
                "",
                (
                    url, 
                    info, 
                    sessionID, 
                    output
                ) => _questCallbacks.GetBuilds(url, info as EmptyRequestData, sessionID)),]
    )
    {
        _questCallbacks = questCallbacks;
    }
}
