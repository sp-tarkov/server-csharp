using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;

namespace Core.Callbacks;

public class ItemEventCallbacks
{
    public ItemEventCallbacks()
    {
    }

    public async Task<GetBodyResponseData<ItemEventRouterResponse>> HandleEvents(string url, ItemEventRouterRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return true if the passed in list of warnings contains critical issues
    /// </summary>
    /// <param name="warnings">The list of warnings to check for critical errors</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool IsCriticalError(List<Warning> warnings)
    {
        throw new NotImplementedException();
    }

    public int GetErrorCode(List<Warning> warnings)
    {
        throw new NotImplementedException();
    }
}