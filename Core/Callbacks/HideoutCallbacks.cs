using Core.Models.Eft.Common;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Spt.Config;

namespace Core.Callbacks;

public class HideoutCallbacks
{
    private HideoutConfig _hideoutConfig;
    
    public HideoutCallbacks()
    {
        
    }
    
    public ItemEventRouterResponse Upgrade(PmcData pmcData, HideoutUpgradeRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse UpgradeComplete(PmcData pmcData, HideoutUpgradeCompleteRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse PutItemsInAreaSlots(PmcData pmcData, HideoutPutItemInRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse TakeItemsFromAreaSlots(PmcData pmcData, HideoutTakeItemOutRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ToggleArea(PmcData pmcData, HideoutToggleAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse SingleProductionStart(PmcData pmcData, HideoutSingleProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ScavCaseProductionStart(PmcData pmcData, HideoutScavCaseStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ContinuousProductionStart(PmcData pmcData, HideoutContinuousProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse TakeProduction(PmcData pmcData, HideoutTakeProductionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HandleQTEEvent(PmcData pmcData, HandleQTEEventRequestData info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse RecordShootingRangePoints(PmcData pmcData, RecordShootingRangePoints info, string sessionID, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse ImproveArea(PmcData pmcData, HideoutImproveAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CancelProduction(PmcData pmcData, HideoutImproveAreaRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse CicleOfCultistProductionStart(PmcData pmcData, HideoutCircleOfCultistProductionStartRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HideoutDeleteProductionRequestData(PmcData pmcData, HideoutDeleteProductionRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public ItemEventRouterResponse HideoutCustomizationApplyCommand(PmcData pmcData, HideoutCustomizationApplyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
    
    public async Task<bool> OnUpdate(int timeSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}