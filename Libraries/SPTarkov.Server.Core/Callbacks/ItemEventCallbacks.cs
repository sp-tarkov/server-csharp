using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class ItemEventCallbacks(HttpResponseUtil _httpResponseUtil, ItemEventRouter _itemEventRouter)
{
    public string HandleEvents(string url, ItemEventRouterRequest info, string sessionID)
    {
        var eventResponse = _itemEventRouter.HandleEvents(info, sessionID);
        var result = IsCriticalError(eventResponse.Warnings)
            ? _httpResponseUtil.GetBody(eventResponse, GetErrorCode(eventResponse.Warnings), eventResponse.Warnings[0].ErrorMessage)
            : _httpResponseUtil.GetBody(eventResponse);

        return result;
    }

    /// <summary>
    ///     Return true if the passed in list of warnings contains critical issues
    /// </summary>
    /// <param name="warnings">The list of warnings to check for critical errors</param>
    /// <returns></returns>
    public bool IsCriticalError(List<Warning>? warnings)
    {
        if (warnings is null)
        {
            return false;
        }

        // List of non-critical error codes, we return true if any error NOT included is passed in
        var nonCriticalErrorCodes = new HashSet<BackendErrorCodes>
        {
            BackendErrorCodes.NotEnoughSpace
        };

        foreach (var warning in warnings)
        {
            if (!nonCriticalErrorCodes.Contains(warning.Code ?? BackendErrorCodes.None))
            {
                return true;
            }
        }

        return false;
    }

    public BackendErrorCodes GetErrorCode(List<Warning> warnings)
    {
        // Cast int to string to get the error code of 220 for Unknown Error.
        return warnings.FirstOrDefault()?.Code is null ? BackendErrorCodes.UnknownError : warnings.FirstOrDefault()?.Code ?? BackendErrorCodes.UnknownError;
    }
}
