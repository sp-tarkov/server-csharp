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
public class InsuranceController
{
    private readonly ISptLogger<InsuranceController> _logger;
    private readonly ProfileHelper _profileHelper;
    private readonly InsuranceService _insuranceService;
    private readonly ConfigServer _configServer;
    private readonly InsuranceConfig _insuranceConfig;

    public InsuranceController(
        ISptLogger<InsuranceController> logger,
        ProfileHelper profileHelper,
        InsuranceService insuranceService,
        ConfigServer configServer
        )
    {
        _logger = logger;
        _profileHelper = profileHelper;
        _insuranceService = insuranceService;
        _configServer = configServer;

        _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();
    }

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

        foreach (var item in pmcData.Inventory.Items) {
            inventoryItemsHash[item.Id] = item;
        }

        // Loop over each trader in request
        foreach(var trader in request.Traders)
        {
            var items = new Dictionary<string, double>();

            foreach (var itemId in request.Items) {
                // Ensure hash has item in it
                if (!inventoryItemsHash.ContainsKey(itemId))
                {
                    _logger.Debug("Item with id: ${ itemId} missing from player inventory, skipping");
                    continue;
                }
                items[inventoryItemsHash[itemId].Template] = _insuranceService.GetRoublePriceToInsureItemWithTrader(pmcData, inventoryItemsHash[itemId], trader);
            }

            response[trader] = items;
        }

        return response;
    }
}
