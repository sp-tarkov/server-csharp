using Core.Annotations;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class ItemEventCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    // protected ItemEventRouter _itemEventRouter;

    public ItemEventCallbacks
    (
        HttpResponseUtil httpResponseUtil
        // TODO: Implement ItemEventRouter
        // ItemEventRouter itemEventRouter
    )
    {
        _httpResponseUtil = httpResponseUtil;
        // _itemEventRouter = itemEventRouter;
    }

    public async Task<GetBodyResponseData<ItemEventRouterResponse>> HandleEvents(string url, ItemEventRouterRequest info, string sessionID)
    {
        // var eventResponse = await _itemEventRouter.HandleEvents(info, sessionID);
        // var result = IsCriticalError(ItemEventRouterResponse.Warnings)
        //     ? _httpResponseUtil.GetBody(eventResponse, GetErrorCode(eventResponse.Warnings), eventResponse.Warnings[0].Errmsg)
        //     : _httpResponseUtil.GetBody(eventResponse);
        // TODO: Implement ItemEventRouter
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return true if the passed in list of warnings contains critical issues
    /// </summary>
    /// <param name="warnings">The list of warnings to check for critical errors</param>
    /// <returns></returns>
    public bool IsCriticalError(List<Warning> warnings)
    {
        // List of non-critical error codes, we return true if any error NOT included is passed in
        var nonCriticalErrorCodes = new List<BackendErrorCodes>() { BackendErrorCodes.NOTENOUGHSPACE };

        foreach (var warning in warnings)
        {
            BackendErrorCodes code;
            
            if (!BackendErrorCodes.TryParse(warning.Code, out code))
                throw new Exception($"Unable to parse [{warning.Code}] to BackendErrorCode.");
            
            if (!nonCriticalErrorCodes.Contains(code))
                return true;
        }
        
        return false;
    }

    public int? GetErrorCode(List<Warning> warnings)
    {
        if (warnings[0].Code is null)
        {
            return int.Parse(BackendErrorCodes.UNKNOWN_ERROR.ToString());
        }

        return int.Parse(warnings[0]?.Code);
    }
}
