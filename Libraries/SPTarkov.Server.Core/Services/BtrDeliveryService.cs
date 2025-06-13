using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BtrDeliveryService(
    ISptLogger<BtrDeliveryService> _logger,
    DatabaseService _databaseService,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    MailSendService _mailSendService,
    ConfigServer _configServer,
    LocalisationService _localisationService
)
{
    protected BtrDeliveryConfig _btrDeliveryConfig = _configServer.GetConfig<BtrDeliveryConfig>();
    protected TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();

    protected static List<string> _transferTypes = new()
    {
        "btr",
        "transit"
    };

    /// <summary>
    ///     Check if player used BTR or transit item sending service and send items to player via mail if found
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="request"> End raid request from client </param>
    public void HandleItemTransferEvent(string sessionId, EndLocalRaidRequestData request)
    {
        foreach (var transferType in _transferTypes)
        {
            var rootId = $"{Traders.BTR}_{transferType}";
            List<Item>? itemsToSend = null;

            // if rootId doesnt exist in TransferItems, skip
            if (!request?.TransferItems?.TryGetValue(rootId, out itemsToSend) ?? false)
            {
                continue;
            }

            // Filter out the btr container item from transferred items before delivering
            itemsToSend = itemsToSend?.Where(item => item.Id != Traders.BTR).ToList();
            if (itemsToSend?.Count == 0)
            {
                continue;
            }

            HandleTransferItemDelivery(sessionId, itemsToSend);
        }
    }

    protected void HandleTransferItemDelivery(string sessionId, List<Item> items)
    {
        var serverProfile = _saveServer.GetProfile(sessionId);
        var pmcData = serverProfile.CharacterData.PmcData;

        // Remove any items that were returned by the item delivery, but also insured, from the player's insurance list
        // This is to stop items being duplicated by being returned from both item delivery and insurance
        var deliveredItemIds = items.Select(item => item.Id);
        pmcData.InsuredItems = pmcData.InsuredItems
            .Where(insuredItem => !deliveredItemIds.Contains(insuredItem.ItemId))
            .ToList();

        if (_saveServer.GetProfile(sessionId).BtrDeliveryList == null)
        {
            _saveServer.GetProfile(sessionId).BtrDeliveryList = new List<BtrDelivery>();
        }

        // Store delivery to send to player later in profile
        _saveServer.GetProfile(sessionId).BtrDeliveryList.Add(
                new BtrDelivery
                {
                    Id = _hashUtil.Generate(),
                    ScheduledTime = (int) GetBTRDeliveryReturnTimestamp(),
                    Items = items
                }
            );
    }

    public void SendBTRDelivery(string sessionId, List<Item> items)
    {
        var dialogueTemplates = _databaseService.GetTrader(Traders.BTR).Dialogue;
        if (dialogueTemplates is null)
        {
            _logger.Error(_localisationService.GetText("inraid-unable_to_deliver_item_no_trader_found", Traders.BTR));
            return;
        }

        if (!dialogueTemplates.TryGetValue("itemsDelivered", out var itemsDelivered))
        {
            _logger.Error(_localisationService.GetText("btr-unable_to_find_items_in_dialog_template", sessionId));

            return;
        }

        var messageId = _randomUtil.GetArrayValue(itemsDelivered);
        var messageStoreTime = _timeUtil.GetHoursAsSeconds(_traderConfig.Fence.BtrDeliveryExpireHours);

        // Send the items to the player
        _mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionId,
            Traders.BTR,
            MessageType.BtrItemsDelivery,
            messageId,
            items,
            messageStoreTime
        );
    }

    /// <summary>
    /// Remove a BTR delivery package from a profile using the package's ID.
    /// </summary>
    /// <param name="sessionId">The session ID of the profile to remove the package from.</param>
    /// <param name="delivery">The BTR delivery package to remove.</param>
    public void RemoveBTRDeliveryPackageFromProfile(string sessionId, BtrDelivery delivery)
    {
        var profile = _saveServer.GetProfile(sessionId);
        profile.BtrDeliveryList = profile.BtrDeliveryList
            .Where(package => package.Id != delivery.Id)
            .ToList();

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Removed processed BTR delivery package. Remaining packages: {profile.BtrDeliveryList.Count}");
        }
    }

    /// <summary>
    /// Get a timestamp of when items given to the BTR driver should be sent to player.
    /// </summary>
    /// <returns>Timestamp to return items to player in seconds</returns>
    protected double GetBTRDeliveryReturnTimestamp()
    {
        // If override in config is non-zero, use that
        if (_btrDeliveryConfig.ReturnTimeOverrideSeconds > 0)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"BTR delivery override used: returning in {_btrDeliveryConfig.ReturnTimeOverrideSeconds} seconds");
            }

            return _timeUtil.GetTimeStamp() + _btrDeliveryConfig.ReturnTimeOverrideSeconds;
        }

        return _timeUtil.GetTimeStamp();
    }
}
