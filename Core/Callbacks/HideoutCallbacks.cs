namespace Core.Callbacks;

public class HideoutCallbacks
{
    public ItemEventRouterRepsonse Upgrade(PmcData pmcData, HideoutUpgraderequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse UpgradeComplete(PmcData pmcData, HideoutUpgradeCompleterequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse SingleProductionStart(PmcData pmcData, HideoutSingleProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse ContinuousProductionStart(PmcData pmcData, HideoutContinuousProductionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterRepsonse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public bool Update(int timeSinceLastRun)
    {
        throw new NotImplementedException();
    }
}