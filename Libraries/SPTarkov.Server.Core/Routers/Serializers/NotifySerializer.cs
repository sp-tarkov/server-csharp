using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Serializers;

[Injectable]
public class NotifySerializer(
    NotifierController notifierController,
    JsonUtil jsonUtil,
    HttpServerHelper httpServerHelper
) : ISerializer
{
    public void Serialize(string sessionID, HttpRequest req, HttpResponse resp, object? body)
    {
        var splittedUrl = req.Path.Value.Split("/");
        var tmpSessionID = splittedUrl[^1].Split("?last_id")[0];

        /*
         * Take our array of JSON message objects and cast them to JSON strings, so that they can then
         *  be sent to client as NEWLINE separated strings... yup.
         */
        notifierController
            .NotifyAsync(tmpSessionID)
            .ContinueWith(messages =>
            {
                return messages.Result.Select(message =>
                {
                    return string.Join("\n", jsonUtil.Serialize(message));
                });
            })
            .ContinueWith(text =>
            {
                httpServerHelper.SendTextJson(resp, text);
            });
    }

    public bool CanHandle(string route)
    {
        return route.ToUpper() == "NOTIFY";
    }
}
