using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.BtrDeliveryCallbacks)]
public class BtrDeliveryCallbacks(
    ISptLogger<BtrDeliveryCallbacks> _logger,
    BtrDeliveryService _btrDeliveryService,
    TimeUtil _timeUtil,
    ConfigServer _configServer,
    SaveServer _saveServer,
    HashUtil _hashUtil,
    ItemHelper _itemHelper
)
    : IOnUpdate
{
    private readonly BtrDeliveryConfig _btrDeliveryConfig = _configServer.GetConfig<BtrDeliveryConfig>();

    public Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun < _btrDeliveryConfig.RunIntervalSeconds)
        {
            return Task.FromResult(false);
        }

        ProcessDeliveries();

        return Task.FromResult(true);
    }

    /// <summary>
    /// Process BTR delivery items of all profiles prior to being given back to the player through the mail service
    /// </summary>
    protected void ProcessDeliveries()
    {
        // Process each installed profile.
        foreach (var sessionId in _saveServer.GetProfiles())
        {
            ProcessDeliveryByProfile(sessionId.Key);
        }
    }

    /// <summary>
    /// Process delivery items of a single profile prior to being given back to the player through the mail service
    /// </summary>
    /// <param name="sessionId">Player id</param>
    public void ProcessDeliveryByProfile(string sessionId)
    {
        // Filter out items that don't need to be processed yet.
        var toBeProcessed = FilterDeliveryItems(sessionId);

        // Do nothing if no items to process
        if (toBeProcessed.Count == 0)
        {
            return;
        }

        ProcessDeliveryItems(toBeProcessed, sessionId);
    }

    /// <summary>
    /// Get all delivery items that are ready to be processed in a specific profile
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>All delivery items that are ready to be processed</returns>
    protected List<BtrDelivery> FilterDeliveryItems(string sessionId)
    {
        var currentTime = _timeUtil.GetTimeStamp();

        var deliveryList = _saveServer.GetProfile(sessionId).BtrDeliveryList;
        if (deliveryList != null && deliveryList!.Count > 0)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Found {deliveryList.Count} BTR delivery package(s) in profile {sessionId}");
            }
            return deliveryList.Where(toBeDelivered => currentTime >= toBeDelivered.ScheduledTime).ToList();
        }

        return [];
    }

    /// <summary>
    /// This method orchestrates the processing of delivery items in a profile
    /// </summary>
    /// <param name="packagesToBeDelivered">The delivery items to process</param>
    /// <param name="sessionId">session ID that should receive the processed items</param>
    protected void ProcessDeliveryItems(List<BtrDelivery> packagesToBeDelivered, string sessionId)
    {
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug(
                $"Processing {packagesToBeDelivered.Count} BTR delivery package(s), which include a total of: {packagesToBeDelivered.Select(items => items.Items).Count()} items, in profile: {sessionId}"
            );
        }

        // Iterate over each of the insurance packages.
        foreach (var package in packagesToBeDelivered)
        {
            // Create a new root parent ID for the message we'll be sending the player
            var rootItemParentId = _hashUtil.Generate();

            // Update the delivery items to have the new root parent ID for root/orphaned items
            package.Items = _itemHelper.AdoptOrphanedItems(rootItemParentId, package.Items);

            _btrDeliveryService.SendBTRDelivery(sessionId, package.Items);

            // Remove the fully processed BTR delivery package from the profile.
            _btrDeliveryService.RemoveBTRDeliveryPackageFromProfile(sessionId, package);
        }
    }
}
