using Core.Controllers;
using Core.DI;
using Core.Helpers;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Routers.Serializers;

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
        notifierController.NotifyAsync(tmpSessionID)
            .ContinueWith(messages => messages.Result.Select(message => string.Join("\n", jsonUtil.Serialize(message))))
            .ContinueWith(text => httpServerHelper.SendTextJson(resp, text));
    }

    public bool CanHandle(string route)
    {
        return route.ToUpper() == "NOTIFY";
    }
}
