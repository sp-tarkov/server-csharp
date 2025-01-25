using SptCommon.Annotations;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Routers;
using Core.Utils;

namespace Core.Callbacks;

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
    /// Return true if the passed in list of warnings contains critical issues
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
        var nonCriticalErrorCodes = new List<BackendErrorCodes> { BackendErrorCodes.NotEnoughSpace };

        foreach (var warning in warnings)
        {
            if (!Enum.TryParse(warning.Code, out BackendErrorCodes code))
                throw new Exception($"Unable to parse [{warning.Code}] to BackendErrorCode.");
            
            if (!nonCriticalErrorCodes.Contains(code))
                return true;
        }
        
        return false;
    }

    public int GetErrorCode(List<Warning> warnings)
    {
        // Cast int to string to get the error code of 220 for Unknown Error.
        return int.Parse((warnings[0].Code is null || warnings[0].Code == "None" 
            ? ((int) BackendErrorCodes.UnknownError).ToString()
            : warnings.FirstOrDefault()?.Code) ?? string.Empty);
    }
}
