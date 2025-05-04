using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Insurance = SPTarkov.Server.Core.Models.Eft.Profile.Insurance;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class InsuranceService(
    ISptLogger<InsuranceService> _logger,
    DatabaseService _databaseService,
    RandomUtil _randomUtil,
    ItemHelper _itemHelper,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    TraderHelper _traderHelper,
    ProfileHelper _profileHelper,
    LocalisationService _localisationService,
    MailSendService _mailSendService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected InsuranceConfig _insuranceConfig = _configServer.GetConfig<InsuranceConfig>();
    protected Dictionary<string, Dictionary<string, List<Item>>?> _insured = new();

    /// <summary>
    ///     Does player have insurance dictionary exists
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <returns>True if exists</returns>
    public bool InsuranceDictionaryExists(string sessionId)
    {
        return _insured.TryGetValue(sessionId, out _);
    }

    /// <summary>
    ///     Get all insured items by all traders for a profile
    /// </summary>
    /// <param name="sessionId">Profile id (session id)</param>
    /// <returns>Item list</returns>
    public Dictionary<string, List<Item>>? GetInsurance(string sessionId)
    {
        return _insured[sessionId];
    }

    public void ResetInsurance(string sessionId)
    {
        if (!_insured.TryAdd(sessionId, new Dictionary<string, List<Item>>()))
        {
            _insured[sessionId] = new Dictionary<string, List<Item>>();
        }
    }

    /// <summary>
    ///     Sends 'I will go look for your stuff' trader message +
    ///     Store lost insurance items inside profile for later retrieval
    /// </summary>
    /// <param name="pmcData">Profile to send insured items to</param>
    /// <param name="sessionID">SessionId of current player</param>
    /// <param name="mapId">Id of the location player died/exited that caused the insurance to be issued on</param>
    public void StartPostRaidInsuranceLostProcess(PmcData pmcData, string sessionID, string mapId)
    {
        // Get insurance items for each trader
        var globals = _databaseService.GetGlobals();
        foreach (var traderKvP in GetInsurance(sessionID))
        {
            var traderBase = _traderHelper.GetTrader(traderKvP.Key, sessionID);
            if (traderBase is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "insurance-unable_to_find_trader_by_id",
                        traderKvP.Key
                    )
                );

                continue;
            }

            var dialogueTemplates = _databaseService.GetTrader(traderKvP.Key).Dialogue;
            if (dialogueTemplates is null)
            {
                _logger.Error(
                    _localisationService.GetText(
                        "insurance-trader_lacks_dialogue_property",
                        traderKvP.Key
                    )
                );

                continue;
            }

            var systemData = new SystemData
            {
                Date = _timeUtil.GetDateMailFormat(),
                Time = _timeUtil.GetTimeMailFormat(),
                Location = mapId,
            };

            // Send "i will go look for your stuff" message from trader to player
            _mailSendService.SendLocalisedNpcMessageToPlayer(
                sessionID,
                traderKvP.Key,
                MessageType.NPC_TRADER,
                _randomUtil.GetArrayValue(
                    dialogueTemplates["insuranceStart"] ?? ["INSURANCE START MESSAGE MISSING"]
                ),
                null,
                _timeUtil.GetHoursAsSeconds(
                    (int)globals.Configuration?.Insurance?.MaxStorageTimeInHour
                ),
                systemData
            );

            // Store insurance to send to player later in profile
            // Store insurance return details in profile + "hey i found your stuff, here you go!" message details to send to player at a later date
            _saveServer
                .GetProfile(sessionID)
                .InsuranceList.Add(
                    new Insurance
                    {
                        ScheduledTime = (int)GetInsuranceReturnTimestamp(pmcData, traderBase),
                        TraderId = traderKvP.Key,
                        MaxStorageTime = (int)GetMaxInsuranceStorageTime(traderBase),
                        SystemData = systemData,
                        MessageType = MessageType.INSURANCE_RETURN,
                        MessageTemplateId = _randomUtil.GetArrayValue(
                            dialogueTemplates["insuranceFound"]
                        ),
                        Items = GetInsurance(sessionID)[traderKvP.Key],
                    }
                );
        }

        ResetInsurance(sessionID);
    }

    /// <summary>
    ///     Get a timestamp of when insurance items should be sent to player based on trader used to insure
    ///     Apply insurance return bonus if found in profile
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="trader">Trader base used to insure items</param>
    /// <returns>Timestamp to return items to player in seconds</returns>
    protected double GetInsuranceReturnTimestamp(PmcData pmcData, TraderBase trader)
    {
        // If override in config is non-zero, use that instead of trader values
        if (_insuranceConfig.ReturnTimeOverrideSeconds > 0)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(
                    $"Insurance override used: returning in {_insuranceConfig.ReturnTimeOverrideSeconds} seconds"
                );
            }

            return _timeUtil.GetTimeStamp() + _insuranceConfig.ReturnTimeOverrideSeconds;
        }

        var insuranceReturnTimeBonusSum = _profileHelper.GetBonusValueFromProfile(
            pmcData,
            BonusType.InsuranceReturnTime
        );

        // A negative bonus implies a faster return, since we subtract later, invert the value here
        var insuranceReturnTimeBonusPercent = -(insuranceReturnTimeBonusSum / 100);

        var traderMinReturnAsSeconds = trader.Insurance.MinReturnHour * TimeUtil.OneHourAsSeconds;
        var traderMaxReturnAsSeconds = trader.Insurance.MaxReturnHour * TimeUtil.OneHourAsSeconds;
        var randomisedReturnTimeSeconds = _randomUtil.GetDouble(
            traderMinReturnAsSeconds.Value,
            traderMaxReturnAsSeconds.Value
        );

        // Check for Mark of The Unheard in players special slots (only slot item can fit)
        var globals = _databaseService.GetGlobals();
        var hasMarkOfUnheard = _itemHelper.HasItemWithTpl(
            pmcData.Inventory.Items,
            ItemTpl.MARKOFUNKNOWN_MARK_OF_THE_UNHEARD,
            "SpecialSlot"
        );
        if (hasMarkOfUnheard)
        // Reduce return time by globals multiplier value
        {
            randomisedReturnTimeSeconds *= globals
                .Configuration
                .Insurance
                .CoefOfHavingMarkOfUnknown
                .Value;
        }

        // EoD has 30% faster returns
        if (
            globals.Configuration.Insurance.EditionSendingMessageTime.TryGetValue(
                pmcData.Info.GameVersion,
                out var editionModifier
            )
        )
        {
            randomisedReturnTimeSeconds *= editionModifier.Multiplier.Value;
        }

        // Calculate the final return time based on our bonus percent
        var finalReturnTimeSeconds =
            randomisedReturnTimeSeconds * (1d - insuranceReturnTimeBonusPercent);
        return _timeUtil.GetTimeStamp() + finalReturnTimeSeconds;
    }

    protected double GetMaxInsuranceStorageTime(TraderBase traderBase)
    {
        if (_insuranceConfig.StorageTimeOverrideSeconds > 0)
        // Override exists, use instead of traders value
        {
            return _insuranceConfig.StorageTimeOverrideSeconds;
        }

        return _timeUtil.GetHoursAsSeconds((int)traderBase.Insurance.MaxStorageTime);
    }

    /// <summary>
    ///     Store lost gear post-raid inside profile, ready for later code to pick it up and mail it
    /// </summary>
    /// <param name="equipmentPkg">Gear to store - generated by GetGearLostInRaid()</param>
    public void StoreGearLostInRaidToSendLater(
        string sessionID,
        List<InsuranceEquipmentPkg> equipmentPkg
    )
    {
        // Process all insured items lost in-raid
        foreach (var gear in equipmentPkg)
        {
            AddGearToSend(gear);
        }
    }

    /// <summary>
    ///     For the passed in items, find the trader it was insured against
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="lostInsuredItems">Insured items lost in a raid</param>
    /// <param name="pmcProfile">Player profile</param>
    /// <returns>InsuranceEquipmentPkg list</returns>
    public List<InsuranceEquipmentPkg> MapInsuredItemsToTrader(
        string sessionId,
        List<Item> lostInsuredItems,
        PmcData pmcProfile
    )
    {
        List<InsuranceEquipmentPkg> result = [];

        foreach (var lostItem in lostInsuredItems)
        {
            var insuranceDetails = pmcProfile.InsuredItems.FirstOrDefault(insuredItem =>
            {
                return insuredItem.ItemId == lostItem.Id;
            });
            if (insuranceDetails is null)
            {
                _logger.Error(
                    $"unable to find insurance details for item id: {lostItem.Id} with tpl: {lostItem.Template}"
                );

                continue;
            }

            if (ItemCannotBeLostOnDeath(lostItem, pmcProfile.Inventory.Items))
            {
                continue;
            }

            // Add insured item + details to return array
            result.Add(
                new InsuranceEquipmentPkg
                {
                    SessionId = sessionId,
                    ItemToReturnToPlayer = lostItem,
                    PmcData = pmcProfile,
                    TraderId = insuranceDetails.TId,
                }
            );
        }

        return result;
    }

    /// <summary>
    ///     Some items should never be returned in insurance but BSG send them in the request
    /// </summary>
    /// <param name="lostItem">Item being returned in insurance</param>
    /// <param name="inventoryItems">Player inventory</param>
    /// <returns>True if item</returns>
    protected bool ItemCannotBeLostOnDeath(Item lostItem, List<Item> inventoryItems)
    {
        if (lostItem.SlotId?.ToLower().StartsWith("specialslot") ?? false)
        {
            return true;
        }

        // We check secure container items even tho they are omitted from lostInsuredItems, just in case
        if (_itemHelper.ItemIsInsideContainer(lostItem, "SecuredContainer", inventoryItems))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Add gear item to InsuredItems list in player profile
    /// </summary>
    /// <param name="gear">Gear to send</param>
    protected void AddGearToSend(InsuranceEquipmentPkg gear)
    {
        var sessionId = gear.SessionId;
        var pmcData = gear.PmcData;
        var itemToReturnToPlayer = gear.ItemToReturnToPlayer;
        var traderId = gear.TraderId;

        // Ensure insurance array is init
        if (!InsuranceDictionaryExists(sessionId))
        {
            ResetInsurance(sessionId);
        }

        // init trader insurance array
        if (!InsuranceTraderArrayExists(sessionId, traderId))
        {
            ResetInsuranceTraderArray(sessionId, traderId);
        }

        AddInsuranceItemToArray(sessionId, traderId, itemToReturnToPlayer);

        // Remove item from insured items array as its been processed
        pmcData.InsuredItems = pmcData
            .InsuredItems.Where(item =>
            {
                return item.ItemId != itemToReturnToPlayer.Id;
            })
            .ToList();
    }

    /// <summary>
    ///     Does insurance exist for a player and by trader
    /// </summary>
    /// <param name="sessionId">Player id (session id)</param>
    /// <param name="traderId">Trader items insured with</param>
    /// <returns>True if exists</returns>
    protected bool InsuranceTraderArrayExists(string sessionId, string traderId)
    {
        return _insured[sessionId].GetValueOrDefault(traderId) is not null;
    }

    /// <summary>
    ///     Empty out list holding insured items by sessionid + traderid
    /// </summary>
    /// <param name="sessionId">Player id (session id)</param>
    /// <param name="traderId">Trader items insured with</param>
    public void ResetInsuranceTraderArray(string sessionId, string traderId)
    {
        _insured[sessionId][traderId] = [];
    }

    /// <summary>
    ///     Store insured item
    /// </summary>
    /// <param name="sessionId">Player id (session id)</param>
    /// <param name="traderId">Trader item insured with</param>
    /// <param name="itemToAdd">Insured item (with children)</param>
    public void AddInsuranceItemToArray(string sessionId, string traderId, Item itemToAdd)
    {
        _insured[sessionId][traderId].Add(itemToAdd);
    }

    /// <summary>
    ///     Get price of insurance * multiplier from config
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="inventoryItem">Item to be insured</param>
    /// <param name="traderId">Trader item is insured with</param>
    /// <returns>price in roubles</returns>
    public double GetRoublePriceToInsureItemWithTrader(
        PmcData? pmcData,
        Item inventoryItem,
        string traderId
    )
    {
        var price =
            _itemHelper.GetStaticItemPrice(inventoryItem.Template)
            * (_traderHelper.GetLoyaltyLevel(traderId, pmcData).InsurancePriceCoefficient / 100);

        return Math.Ceiling(price ?? 1);
    }
}
