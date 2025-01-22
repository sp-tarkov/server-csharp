using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Insurance;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using Insurance = Core.Models.Eft.Profile.Insurance;

namespace Core.Controllers;

[Injectable]
public class InsuranceController(
    ISptLogger<InsuranceController> _logger,
    RandomUtil _randomUtil,
    MathUtil _mathUtil,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    EventOutputHolder _eventOutputHolder,
    ItemHelper _itemHelper,
    ProfileHelper _profileHelper,
    WeightedRandomHelper _weightedRandomHelper,
    PaymentService _paymentService,
    InsuranceService _insuranceService,
    DatabaseService _databaseService,
    MailSendService _mailSendService,
    RagfairPriceService _ragfairPriceService,
    LocalisationService _localisationService,
    SaveServer _saveServer,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected InsuranceConfig _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();


    public void ProcessReturn()
    {
        // Process each installed profile.
        foreach (var sessionId in _saveServer.GetProfiles()) ProcessReturnByProfile(sessionId.Key);
    }

    public void ProcessReturnByProfile(string sessionId)
    {
        // Filter out items that don't need to be processed yet.
        var insuranceDetails = FilterInsuredItems(sessionId);

        // Skip profile if no insured items to process
        if (insuranceDetails.Count == 0) return;

        ProcessInsuredItems(insuranceDetails, sessionId);
    }

    private List<Insurance> FilterInsuredItems(string sessionId, long? time = null)
    {
        // Use the current time by default.
        var insuranceTime = time ?? _timeUtil.GetTimeStamp();

        var profileInsuranceDetails = _saveServer.GetProfile(sessionId).InsuranceList;
        if (profileInsuranceDetails.Count > 0)
            _logger.Debug($"Found {profileInsuranceDetails.Count} insurance packages in profile {sessionId}");

        return profileInsuranceDetails.Where(insured => insuranceTime >= insured.ScheduledTime).ToList();
    }

    /**
     * This method orchestrates the processing of insured items in a profile.
     * 
     * @param insuranceDetails The insured items to process.
     * @param sessionID The session ID that should receive the processed items.
     * @returns void
     */
    protected void ProcessInsuredItems(List<Insurance> insuranceDetails, string sessionId)
    {
        _logger.Debug(
            $"Processing {insuranceDetails.Count} insurance packages, which includes a total of ${CountAllInsuranceItems(insuranceDetails)} items, in profile ${sessionId}"
        );

        // Iterate over each of the insurance packages.
        foreach (var insured in insuranceDetails)
        {
            // Create a new root parent ID for the message we'll be sending the player
            var rootItemParentID = _hashUtil.Generate();

            // Update the insured items to have the new root parent ID for root/orphaned items
            insured.Items = _itemHelper.AdoptOrphanedItems(rootItemParentID, insured.Items);

            var simulateItemsBeingTaken = _insuranceConfig.SimulateItemsBeingTaken;
            if (simulateItemsBeingTaken)
            {
                // Find items that could be taken by another player off the players body
                var itemsToDelete = FindItemsToDelete(rootItemParentID, insured);

                // Actually remove them.
                RemoveItemsFromInsurance(insured, itemsToDelete);

                // There's a chance we've orphaned weapon attachments, so adopt any orphaned items again
                insured.Items = _itemHelper.AdoptOrphanedItems(rootItemParentID, insured.Items);
            }

            // Send the mail to the player.
            SendMail(sessionId, insured);

            // Remove the fully processed insurance package from the profile.
            RemoveInsurancePackageFromProfile(sessionId, insured);
        }
    }

    private string CountAllInsuranceItems(List<Insurance> insuranceDetails)
    {
        throw new NotImplementedException();
    }

    private object FindItemsToDelete(string rootItemParentId, Insurance insured)
    {
        throw new NotImplementedException();
    }

    private void RemoveItemsFromInsurance(Insurance insured, object itemsToDelete)
    {
        throw new NotImplementedException();
    }

    private void SendMail(string sessionId, Insurance insured)
    {
        throw new NotImplementedException();
    }

    private void RemoveInsurancePackageFromProfile(string sessionId, Insurance insured)
    {
        throw new NotImplementedException();
    }

    /**
     * Ensure soft inserts of Armor that has soft insert slots
     * Allows armors to come back after being lost correctly
     * @param item Armor item to be insured
     * @param pmcData Player profile
     * @param body Insurance request data
     */
    public void InsureSoftInserts(Item item, PmcData pmcData, InsureRequestData body)
    {
        var softInsertIds = _itemHelper.GetSoftInsertSlotIds();
        var softInsertSlots = pmcData.Inventory.Items.Where(
            item => item.ParentId == item.Id && softInsertIds.Contains(item.SlotId.ToLower())
        );

        foreach (var softInsertSlot in softInsertSlots)
        {
            _logger.Debug($"SoftInsertSlots: ${softInsertSlot.SlotId}");
            pmcData.InsuredItems.Add(new InsuredItem { TId = body.TransactionId, ItemId = softInsertSlot.Id });
        }
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

        foreach (var item in pmcData?.Inventory?.Items ?? []) inventoryItemsHash[item.Id] = item;

        // Loop over each trader in request
        foreach (var trader in request.Traders ?? [])
        {
            var items = new Dictionary<string, double>();

            foreach (var itemId in request.Items ?? [])
            {
                // Ensure hash has item in it
                if (!inventoryItemsHash.ContainsKey(itemId))
                {
                    _logger.Debug($"Item with id: {itemId} missing from player inventory, skipping");
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
