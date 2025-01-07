using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.ItemEvent;

namespace Core.Callbacks;

public class ItemEventCallbacks
{
    public async Task<GetBodyResponseData<ItemEventRouterResponse>> HandleEvents(string url, ItemEventRouterRequest info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public bool IsCriticalError(List<Warning> warnings)
    {
        throw new NotImplementedException();
    }

    public int GetErrorCode(List<Warning> warnings)
    {
        throw new NotImplementedException();
    }
}