using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Insurance;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;

namespace Core.Controllers;

[Injectable]
public class InsuranceController(
    ISptLogger<InsuranceController> _logger,
    ProfileHelper _profileHelper,
    InsuranceService _insuranceService,
    ConfigServer _configServer
)
{
    protected InsuranceConfig _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();


    /**
     * Handle client/insurance/items/list/cost
     * Calculate insurance cost
     *
     * @param request request object
     * @param sessionID session id
     * @returns IGetInsuranceCostResponseData object to send to client
     */
    public GetInsuranceCostResponseData Cost(GetInsuranceCostRequestData request, string sessionId)
    {
        var response = new GetInsuranceCostResponseData();
        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var inventoryItemsHash = new Dictionary<string, Item>();

        foreach (var item in pmcData.Inventory.Items)
        {
            inventoryItemsHash[item.Id] = item;
        }

        // Loop over each trader in request
        foreach (var trader in request.Traders)
        {
            var items = new Dictionary<string, double>();

            foreach (var itemId in request.Items)
            {
                // Ensure hash has item in it
                if (!inventoryItemsHash.ContainsKey(itemId))
                {
                    _logger.Debug("Item with id: ${ itemId} missing from player inventory, skipping");
                    continue;
                }

                items[inventoryItemsHash[itemId].Template] =
                    _insuranceService.GetRoublePriceToInsureItemWithTrader(pmcData, inventoryItemsHash[itemId], trader);
            }

            response[trader] = items;
        }

        return response;
    }
}
