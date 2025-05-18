using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;


namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class TraderHelper(
    ISptLogger<TraderHelper> _logger,
    DatabaseService _databaseService,
    ProfileHelper _profileHelper,
    HandbookHelper _handbookHelper,
    ItemHelper _itemHelper,
    PlayerService _playerService,
    LocalisationService _localisationService,
    FenceService _fenceService,
    TraderStore _traderStore,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    ConfigServer _configServer
)
{
    protected List<string> _gameVersions = [GameEditions.EDGE_OF_DARKNESS, GameEditions.UNHEARD];
    protected Dictionary<string, double> _highestTraderPriceItems = new();
    protected TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();

    public TraderBase? GetTraderByNickName(string traderName, string? sessionId = null)
    {
        return _databaseService.GetTraders().Select(dict => dict.Value.Base).First(t => t?.Nickname != null && string.Equals(t.Nickname, traderName, StringComparison.CurrentCultureIgnoreCase));
    }


    /// <summary>
    ///     Get a trader base object, update profile to reflect players current standing in profile
    ///     when trader not found in profile
    /// </summary>
    /// <param name="traderID">Traders Id to get</param>
    /// <param name="sessionID">Players id</param>
    /// <returns>Trader base</returns>
    public TraderBase? GetTrader(string traderID, string sessionID)
    {
        if (traderID == "ragfair")
        {
            return new TraderBase
            {
                Currency = CurrencyType.RUB
            };
        }

        var pmcData = _profileHelper.GetPmcProfile(sessionID);
        if (pmcData == null)
        {
            throw new Exception(_localisationService.GetText("trader-unable_to_find_profile_with_id", sessionID));
        }

        // Profile has traderInfo dict (profile beyond creation stage) but no requested trader in profile
        if (pmcData?.TradersInfo != null && !(pmcData?.TradersInfo?.ContainsKey(traderID) ?? false))
        {
            // Add trader values to profile
            ResetTrader(sessionID, traderID);
            LevelUp(traderID, pmcData);
        }

        var traderBase = _databaseService.GetTrader(traderID).Base;
        if (traderBase == null)
        {
            _logger.Error(_localisationService.GetText("trader-unable_to_find_trader_by_id", traderID));
        }

        return traderBase;
    }

    /// <summary>
    ///     Get all assort data for a particular trader
    /// </summary>
    /// <param name="traderId">Trader to get assorts for</param>
    /// <returns>TraderAssort</returns>
    public TraderAssort GetTraderAssortsByTraderId(string traderId)
    {
        return traderId == Traders.FENCE
            ? _fenceService.GetRawFenceAssorts()
            : _databaseService.GetTrader(traderId).Assort;
    }

    /// <summary>
    ///     Retrieve the Item from a traders assort data by its id
    /// </summary>
    /// <param name="traderId">Trader to get assorts for</param>
    /// <param name="assortId">Id of assort to find</param>
    /// <returns>Item object</returns>
    public Item? GetTraderAssortItemByAssortId(string traderId, string assortId)
    {
        var traderAssorts = GetTraderAssortsByTraderId(traderId);
        if (traderAssorts is null)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"No assorts on trader: {traderId} found");
            }

            return null;
        }

        // Find specific assort in traders data
        var purchasedAssort = traderAssorts.Items.FirstOrDefault(item => item.Id == assortId);
        if (purchasedAssort is null)
        {
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"No assort {assortId} on trader: {traderId} found");
            }

            return null;
        }

        return purchasedAssort;
    }

    /// <summary>
    ///     Reset a profiles trader data back to its initial state as seen by a level 1 player
    ///     Does NOT take into account different profile levels
    /// </summary>
    /// <param name="sessionID">session id of player</param>
    /// <param name="traderID">trader id to reset</param>
    public void ResetTrader(string sessionID, string traderID)
    {
        var profiles = _databaseService.GetProfiles();
        var trader = _databaseService.GetTrader(traderID);

        var fullProfile = _profileHelper.GetFullProfile(sessionID);
        if (fullProfile is null)
        {
            throw new Exception(_localisationService.GetText("trader-unable_to_find_profile_by_id", sessionID));
        }

        var pmcData = fullProfile.CharacterData.PmcData;
        var rawProfileTemplate = profiles.GetByJsonProp<ProfileSides>(fullProfile.ProfileInfo.Edition)
            .GetByJsonProp<TemplateSide>(pmcData.Info.Side.ToLower())
            .Trader;

        var newTraderData = new TraderInfo
        {
            Disabled = false,
            LoyaltyLevel = rawProfileTemplate.InitialLoyaltyLevel.GetValueOrDefault(traderID, 1),
            SalesSum = rawProfileTemplate.InitialSalesSum,
            Standing = GetStartingStanding(traderID, rawProfileTemplate),
            NextResupply = trader.Base.NextResupply,
            Unlocked = trader.Base.UnlockedByDefault
        };

        if (!pmcData.TradersInfo.TryAdd(traderID, newTraderData))
        {
            pmcData.TradersInfo[traderID] = newTraderData;
        }


        // Check if trader should be locked by default
        if (rawProfileTemplate.LockedByDefaultOverride?.Contains(traderID) ?? false)
        {
            pmcData.TradersInfo[traderID].Unlocked = true;
        }

        if (rawProfileTemplate.PurchaseAllClothingByDefaultForTrader?.Contains(traderID) ?? false)
        {
            // Get traders clothing
            var clothing = _databaseService.GetTrader(traderID).Suits;
            if (clothing?.Count > 0)
                // Force suit ids into profile
            {
                AddSuitsToProfile(
                    fullProfile,
                    clothing.Select(suit => suit.SuiteId).ToList()
                );
            }
        }

        if ((rawProfileTemplate.FleaBlockedDays ?? 0) > 0)
        {
            var newBanDateTime = _timeUtil.GetTimeStampFromNowDays(rawProfileTemplate.FleaBlockedDays ?? 0);
            var existingBan = pmcData.Info.Bans.FirstOrDefault(ban => ban.BanType == BanType.RAGFAIR);
            if (existingBan is not null)
            {
                existingBan.DateTime = newBanDateTime;
            }
            else
            {
                pmcData.Info.Bans.Add(
                    new Ban
                    {
                        BanType = BanType.RAGFAIR,
                        DateTime = newBanDateTime
                    }
                );
            }
        }

        if (traderID == Traders.JAEGER)
        {
            pmcData.TradersInfo[traderID].Unlocked = rawProfileTemplate.JaegerUnlocked;
        }
    }

    /// <summary>
    ///     Get the starting standing of a trader based on the current profiles type (e.g. EoD, Standard etc)
    /// </summary>
    /// <param name="traderId">Trader id to get standing for</param>
    /// <param name="rawProfileTemplate">Raw profile from profiles.json to look up standing from</param>
    /// <returns>Standing value</returns>
    protected double? GetStartingStanding(string traderId, ProfileTraderTemplate rawProfileTemplate)
    {
        if (rawProfileTemplate.InitialStanding.TryGetValue(traderId, out var standing))
        {
            // Edge case for Lightkeeper, 0 standing means seeing `Make Amends - Buyout` quest
            if (traderId == Traders.LIGHTHOUSEKEEPER && standing == 0)
            {
                return 0.01;
            }

            return standing;
        }

        return rawProfileTemplate.InitialStanding["default"];
    }

    /// <summary>
    ///     Add a list of suit ids to a profiles suit list, no duplicates
    /// </summary>
    /// <param name="fullProfile">Profile to add clothing to</param>
    /// <param name="clothingIds">Clothing Ids to add to profile</param>
    public void AddSuitsToProfile(SptProfile fullProfile, List<string> clothingIds)
    {
        fullProfile.CustomisationUnlocks ??= [];

        foreach (var suitId in clothingIds)
        {
            if (!fullProfile.CustomisationUnlocks.Exists(customisation => customisation.Id == suitId))
            {
                // Clothing item doesn't exist in profile, add it
                fullProfile.CustomisationUnlocks.Add(new CustomisationStorage
                {
                    Id = suitId,
                    Source = CustomisationSource.UNLOCKED_IN_GAME,
                    Type = CustomisationType.SUITE
                });
            }
        }
    }

    /// <summary>
    ///     Alter a traders unlocked status
    /// </summary>
    /// <param name="traderId">Trader to alter</param>
    /// <param name="status">New status to use</param>
    /// <param name="sessionId">Session id of player</param>
    public void SetTraderUnlockedState(string traderId, bool status, string sessionId)
    {
        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var profileTraderData = pmcData.TradersInfo[traderId];
        if (profileTraderData is null)
        {
            _logger.Error($"Unable to set trader {traderId} unlocked state to: {status} as trader cannot be found in profile");

            return;
        }

        profileTraderData.Unlocked = status;
    }

    /// <summary>
    ///     Add standing to a trader and level them up if exp goes over level threshold
    /// </summary>
    /// <param name="sessionId">Session id of player</param>
    /// <param name="traderId">Traders id to add standing to</param>
    /// <param name="standingToAdd">Standing value to add to trader</param>
    public void AddStandingToTrader(string sessionId, string traderId, double standingToAdd)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var pmcTraderInfo = fullProfile.CharacterData.PmcData.TradersInfo[traderId];

        // Add standing to trader
        pmcTraderInfo.Standing = AddStandingValuesTogether(pmcTraderInfo.Standing, standingToAdd);

        if (traderId == Traders.FENCE)
            // Must add rep to scav profile to ensure consistency
        {
            fullProfile.CharacterData.ScavData.TradersInfo[traderId].Standing = pmcTraderInfo.Standing;
        }

        LevelUp(traderId, fullProfile.CharacterData.PmcData);
    }

    /// <summary>
    ///     Add standing to current standing and clamp value if it goes too low
    /// </summary>
    /// <param name="currentStanding">current trader standing</param>
    /// <param name="standingToAdd">standing to add to trader standing</param>
    /// <returns>current standing + added standing (clamped if needed)</returns>
    protected double? AddStandingValuesTogether(double? currentStanding, double standingToAdd)
    {
        var newStanding = currentStanding + standingToAdd;

        // Never let standing fall below 0
        return newStanding < 0 ? 0 : newStanding;
    }

    /// <summary>
    ///     Iterate over a profile's traders and ensure they have the correct loyalty level for the player.
    /// </summary>
    /// <param name="sessionId">Profile to check.</param>
    public void ValidateTraderStandingsAndPlayerLevelForProfile(string sessionId)
    {
        var profile = _profileHelper.GetPmcProfile(sessionId);
        var traders = _databaseService.GetTraders();
        foreach (var trader in traders)
        {
            LevelUp(trader.Key, profile);
        }
    }

    /// <summary>
    ///     Calculate trader's level based on experience amount and increments level if over threshold.
    ///     Also validates and updates player level if not correct based on XP value.
    /// </summary>
    /// <param name="traderID">Trader to check standing of.</param>
    /// <param name="pmcData">Profile to update trader in.</param>
    public void LevelUp(string traderID, PmcData pmcData)
    {
        var loyaltyLevels = _databaseService.GetTrader(traderID).Base.LoyaltyLevels;

        // Level up player
        pmcData.Info.Level = _playerService.CalculateLevel(pmcData);

        // Level up traders
        var targetLevel = 0;

        // Round standing to 2 decimal places to address floating point inaccuracies
        pmcData.TradersInfo[traderID].Standing = Math.Round(pmcData.TradersInfo[traderID].Standing * 100 ?? 0, 2) / 100;

        foreach (var loyaltyLevel in loyaltyLevels)
        {
            if (loyaltyLevel.MinLevel <= pmcData.Info.Level &&
                loyaltyLevel.MinSalesSum <= pmcData.TradersInfo[traderID].SalesSum &&
                loyaltyLevel.MinStanding <= pmcData.TradersInfo[traderID].Standing &&
                targetLevel < 4
               )
                // level reached
            {
                targetLevel++;
            }
        }

        // set level
        pmcData.TradersInfo[traderID].LoyaltyLevel = targetLevel;
    }

    /// <summary>
    ///     Get the next update timestamp for a trader.
    /// </summary>
    /// <param name="traderID">Trader to look up update value for.</param>
    /// <returns>Future timestamp.</returns>
    public long GetNextUpdateTimestamp(string traderID)
    {
        var time = _timeUtil.GetTimeStamp();
        var updateSeconds = GetTraderUpdateSeconds(traderID) ?? 0;
        return time + updateSeconds;
    }

    /// <summary>
    ///     Get the reset time between trader assort refreshes in seconds.
    /// </summary>
    /// <param name="traderId">Trader to look up.</param>
    /// <returns>Time in seconds.</returns>
    public long? GetTraderUpdateSeconds(string traderId)
    {
        var traderDetails = _traderConfig.UpdateTime.FirstOrDefault(x => x.TraderId == traderId);
        if (traderDetails?.Seconds?.Min is null || traderDetails.Seconds?.Max is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "trader-missing_trader_details_using_default_refresh_time",
                    new
                    {
                        traderId,
                        updateTime = _traderConfig.UpdateTimeDefault
                    }
                )
            );

            _traderConfig.UpdateTime.Add(
                new UpdateTime
                    // create temporary entry to prevent logger spam
                    {
                        TraderId = traderId,
                        Seconds = new MinMax<int>(_traderConfig.UpdateTimeDefault, _traderConfig.UpdateTimeDefault)
                    }
            );

            return null;
        }

        return _randomUtil.GetInt(traderDetails.Seconds.Min, traderDetails.Seconds.Max);
    }

    public TraderLoyaltyLevel GetLoyaltyLevel(string traderID, PmcData pmcData)
    {
        var traderBase = _databaseService.GetTrader(traderID).Base;

        int? loyaltyLevel = null;
        if (pmcData.TradersInfo.TryGetValue(traderID, out var traderInfo))
        {
            loyaltyLevel = traderInfo.LoyaltyLevel;
        }

        if (loyaltyLevel is null or < 1)
        {
            loyaltyLevel = 1;
        }

        if (loyaltyLevel > traderBase.LoyaltyLevels.Count)
        {
            loyaltyLevel = traderBase.LoyaltyLevels.Count;
        }

        return traderBase.LoyaltyLevels[loyaltyLevel - 1 ?? 1];
    }

    /// <summary>
    ///     Store the purchase of an assort from a trader in the player profile
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="newPurchaseDetails">New item assort id + count</param>
    public void AddTraderPurchasesToPlayerProfile(
        string sessionID,
        PurchaseDetails newPurchaseDetails,
        Item itemPurchased)
    {
        var profile = _profileHelper.GetFullProfile(sessionID);
        var traderId = newPurchaseDetails.TraderId;

        // Iterate over assorts bought and add to profile
        foreach (var purchasedItem in newPurchaseDetails.Items)
        {
            var currentTime = _timeUtil.GetTimeStamp();

            // Nullguard traderPurchases
            profile.TraderPurchases ??= new Dictionary<string, Dictionary<string, TraderPurchaseData>?>();
            // Nullguard traderPurchases for this trader
            profile.TraderPurchases[traderId] ??= new Dictionary<string, TraderPurchaseData>();

            // Null guard when dict doesnt exist

            if (profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseCount is null ||
                profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseTimestamp is null)
            {
                profile.TraderPurchases[traderId][purchasedItem.ItemId] = new TraderPurchaseData
                {
                    PurchaseCount = purchasedItem.Count,
                    PurchaseTimestamp = currentTime
                };

                continue;
            }

            if (profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseCount + purchasedItem.Count >
                GetAccountTypeAdjustedTraderPurchaseLimit(
                    (double) itemPurchased.Upd.BuyRestrictionMax,
                    profile.CharacterData.PmcData.Info.GameVersion
                )
               )
            {
                throw new Exception(
                    _localisationService.GetText(
                        "trader-unable_to_purchase_item_limit_reached",
                        new
                        {
                            traderId,
                            limit = itemPurchased.Upd.BuyRestrictionMax
                        }
                    )
                );
            }

            profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseCount += purchasedItem.Count;
            profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseTimestamp = currentTime;
        }
    }

    /// <summary>
    ///     EoD and Unheard get a 20% bonus to personal trader limit purchases
    /// </summary>
    /// <param name="buyRestrictionMax">Existing value from trader item</param>
    /// <param name="gameVersion">Profiles game version</param>
    /// <returns>buyRestrictionMax value</returns>
    public double GetAccountTypeAdjustedTraderPurchaseLimit(double buyRestrictionMax, string gameVersion)
    {
        if (_gameVersions.Contains(gameVersion))
        {
            return Math.Floor(buyRestrictionMax * 1.2);
        }

        return buyRestrictionMax;
    }

    /// <summary>
    ///     Get the highest rouble price for an item from traders
    ///     UNUSED
    /// </summary>
    /// <param name="tpl">Item to look up highest price for</param>
    /// <returns>highest rouble cost for item</returns>
    public double GetHighestTraderPriceRouble(string tpl)
    {
        if (_highestTraderPriceItems is not null)
        {
            return _highestTraderPriceItems[tpl];
        }

        // Init dict and fill
        foreach (var trader in _traderStore.GetAllTraders())
        {
            // Skip some traders
            if (trader.Id == Traders.FENCE)
            {
                continue;
            }

            // Get assorts for trader, skip trader if no assorts found
            var traderAssorts = _databaseService.GetTrader(trader.Id).Assort;
            if (traderAssorts is null)
            {
                continue;
            }

            // Get all item assorts that have parentId of hideout (base item and not a mod of other item)
            foreach (var item in traderAssorts.Items.Where(x => x.ParentId == "hideout"))
            {
                // Get barter scheme (contains cost of item)
                var barterScheme = traderAssorts.BarterScheme[item.Id].FirstOrDefault().FirstOrDefault();

                // Convert into roubles
                var roubleAmount = barterScheme.Template == Money.ROUBLES
                    ? barterScheme.Count
                    : _handbookHelper.InRUB(barterScheme.Count ?? 1, barterScheme.Template);

                // Existing price smaller in dict than current iteration, overwrite
                if (_highestTraderPriceItems[item.Template] < roubleAmount)
                {
                    _highestTraderPriceItems[item.Template] = roubleAmount.Value;
                }
            }
        }

        return _highestTraderPriceItems[tpl];
    }

    /// <summary>
    ///     Get the highest price item can be sold to trader for (roubles)
    /// </summary>
    /// <param name="tpl">Item to look up best trader sell-to price</param>
    /// <returns>Rouble price</returns>
    public double GetHighestSellToTraderPrice(string tpl)
    {
        // Find largest trader price for item
        var highestPrice = 1d; // Default price
        foreach (var trader in _traderStore.GetAllTraders())
        {
            // Get trader and check buy category allows tpl
            var traderBase = _databaseService.GetTrader(trader.Id).Base;

            // Skip traders that don't sell this category of item
            if (traderBase is null || !_itemHelper.IsOfBaseclasses(tpl, traderBase.ItemsBuy.Category))
            {
                continue;
            }

            // Get loyalty level details player has achieved with this trader
            // Uses lowest loyalty level as this function is used before a player has logged into server
            // We have no idea what player loyalty is with traders
            var traderBuyBackPricePercent = 100 - traderBase.LoyaltyLevels.FirstOrDefault().BuyPriceCoefficient;

            var itemHandbookPrice = _handbookHelper.GetTemplatePrice(tpl);
            var priceTraderBuysItemAt = _randomUtil.GetPercentOfValue(traderBuyBackPricePercent ?? 0, itemHandbookPrice, 0);

            // Price from this trader is higher than highest found, update
            if (priceTraderBuysItemAt > highestPrice)
            {
                highestPrice = priceTraderBuysItemAt;
            }
        }

        return highestPrice;
    }

    /// <summary>
    ///     Accepts a trader id
    /// </summary>
    /// <param name="traderId">Trader id</param>
    /// <returns>True if a Trader exists with given ID</returns>
    public bool TraderExists(string traderId)
    {
        return _traderStore.GetTraderById(traderId) != null;
    }
}
