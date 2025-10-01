using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
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

[Injectable(InjectionType.Singleton)]
public class TraderHelper(
    ISptLogger<TraderHelper> logger,
    DatabaseService databaseService,
    ProfileHelper profileHelper,
    HandbookHelper handbookHelper,
    ServerLocalisationService serverLocalisationService,
    FenceService fenceService,
    TimeUtil timeUtil,
    RandomUtil randomUtil,
    ConfigServer configServer
)
{
    protected readonly FrozenSet<string> GameVersionsWithHigherBuyRestrictions = [GameEditions.EDGE_OF_DARKNESS, GameEditions.UNHEARD];
    protected readonly Dictionary<MongoId, double> HighestTraderPriceItems = new();
    protected readonly TraderConfig TraderConfig = configServer.GetConfig<TraderConfig>();

    protected readonly Lock _highestPriceLock = new Lock();

    /// <summary>
    /// Get a traders base data from its nickname, case insensitive
    /// </summary>
    /// <param name="traderName">Nickname of trader, e.g. prapor</param>
    /// <returns>TraderBase</returns>
    public TraderBase? GetTraderByNickName(string traderName)
    {
        return databaseService
            .GetTraders()
            .Select(dict => dict.Value.Base)
            .FirstOrDefault(t => t?.Nickname != null && string.Equals(t.Nickname, traderName, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    ///     Get a trader base object, update profile to reflect players current standing in profile (if session id provided)
    ///     when trader not found in profile
    /// </summary>
    /// <param name="traderId">Traders Id to return base of</param>
    /// <param name="sessionId">OPTIONAL - Players id</param>
    /// <returns>Trader base</returns>
    public TraderBase? GetTrader(MongoId traderId, MongoId? sessionId = null)
    {
        if (sessionId is not null)
        {
            var pmcData = profileHelper.GetPmcProfile(sessionId.Value);
            if (pmcData == null)
            {
                throw new Exception(serverLocalisationService.GetText("trader-unable_to_find_profile_with_id", sessionId));
            }

            // Profile has traderInfo dict (profile beyond creation stage) but no requested trader in profile
            if (pmcData?.TradersInfo != null && !(pmcData?.TradersInfo?.ContainsKey(traderId) ?? false))
            {
                // Add trader values to profile
                ResetTrader(sessionId.Value, traderId);
                LevelUp(traderId, pmcData);
            }
        }

        var traderBase = databaseService.GetTrader(traderId)?.Base;
        if (traderBase == null)
        {
            logger.Error(serverLocalisationService.GetText("trader-unable_to_find_trader_by_id", traderId.ToString()));
        }

        return traderBase;
    }

    /// <summary>
    ///     Get all assort data for a particular trader
    /// </summary>
    /// <param name="traderId">Trader to get assorts for</param>
    /// <returns>TraderAssort</returns>
    public TraderAssort? GetTraderAssortsByTraderId(MongoId traderId)
    {
        return traderId == Traders.FENCE ? fenceService.GetRawFenceAssorts() : databaseService.GetTrader(traderId)?.Assort;
    }

    /// <summary>
    ///     Retrieve the Item from a traders assort data by its id
    /// </summary>
    /// <param name="traderId">Trader to get assorts for</param>
    /// <param name="assortId">Id of assort to find</param>
    /// <returns>Item object</returns>
    public Item? GetTraderAssortItemByAssortId(MongoId traderId, MongoId assortId)
    {
        var traderAssorts = GetTraderAssortsByTraderId(traderId);
        if (traderAssorts is null)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"No assorts on trader: {traderId} found");
            }

            return null;
        }

        // Find specific assort in traders data
        var purchasedAssort = traderAssorts.Items.FirstOrDefault(item => item.Id == assortId);
        if (purchasedAssort is null)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"No assort {assortId} on trader: {traderId} found");
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
    public void ResetTrader(MongoId sessionID, MongoId traderID)
    {
        var trader = databaseService.GetTrader(traderID);

        var fullProfile = profileHelper.GetFullProfile(sessionID);
        if (fullProfile is null)
        {
            throw new Exception(serverLocalisationService.GetText("trader-unable_to_find_profile_by_id", sessionID));
        }

        // Get matching profile 'type' e.g. 'standard'
        var pmcData = fullProfile.CharacterData.PmcData;
        var matchingSide = profileHelper.GetProfileTemplateForSide(fullProfile.ProfileInfo.Edition, pmcData.Info.Side);

        // Profiles trader settings
        var profileTemplateTraderData = matchingSide.Trader;

        var newTraderData = new TraderInfo
        {
            Disabled = false,
            LoyaltyLevel = profileTemplateTraderData.InitialLoyaltyLevel.GetValueOrDefault(traderID, 1),
            SalesSum = profileTemplateTraderData.InitialSalesSum,
            Standing = GetStartingStanding(traderID, profileTemplateTraderData),
            NextResupply = trader.Base.NextResupply,
            Unlocked = trader.Base.UnlockedByDefault,
        };

        // Add trader to profile if it doesn't already
        pmcData.TradersInfo.TryAdd(traderID, newTraderData);

        // Check if trader should be locked by default
        if (profileTemplateTraderData.LockedByDefaultOverride?.Contains(traderID) ?? false)
        {
            pmcData.TradersInfo[traderID].Unlocked = true;
        }

        if (profileTemplateTraderData.PurchaseAllClothingByDefaultForTrader?.Contains(traderID) ?? false)
        {
            // Get traders clothing
            var clothing = databaseService.GetTrader(traderID).Suits;
            if (clothing?.Count > 0)
            // Force suit ids into profile
            {
                fullProfile.AddSuitsToProfile(clothing.Select(suit => suit.SuiteId).ToList());
            }
        }

        // Template has flea block
        if ((profileTemplateTraderData.FleaBlockedDays ?? 0) > 0)
        {
            var newBanDateTime = timeUtil.GetTimeStampFromNowDays(profileTemplateTraderData.FleaBlockedDays ?? 0);
            var existingBan = pmcData.Info.Bans?.FirstOrDefault(ban => ban.BanType == BanType.RagFair);
            if (existingBan is not null)
            {
                existingBan.DateTime = newBanDateTime;
            }
            else
            {
                pmcData.Info.Bans ??= [];
                pmcData.Info.Bans = pmcData.Info.Bans.Union([new Ban { BanType = BanType.RagFair, DateTime = newBanDateTime }]);
            }
        }

        if (traderID == Traders.JAEGER)
        {
            pmcData.TradersInfo[traderID].Unlocked = profileTemplateTraderData.JaegerUnlocked;
        }
    }

    /// <summary>
    ///     Get the starting standing of a trader based on the current profiles type (e.g. EoD, Standard etc)
    /// </summary>
    /// <param name="traderId">Trader id to get standing for</param>
    /// <param name="rawProfileTemplate">Raw profile from profiles.json to look up standing from</param>
    /// <returns>Standing value</returns>
    protected double? GetStartingStanding(MongoId traderId, ProfileTraderTemplate rawProfileTemplate)
    {
        if (rawProfileTemplate.InitialStanding.TryGetValue(traderId, out var standing))
        {
            // Edge case for Lightkeeper trader, 0 standing means seeing `Make Amends - Buyout` quest
            if (traderId == Traders.LIGHTHOUSEKEEPER && standing == 0)
            {
                return 0.01;
            }

            return standing;
        }

        return rawProfileTemplate.InitialStanding["default"];
    }

    /// <summary>
    ///     Alter a traders unlocked status
    /// </summary>
    /// <param name="traderId">Trader to alter</param>
    /// <param name="status">New status to apply</param>
    /// <param name="sessionId">Session id of player</param>
    public void SetTraderUnlockedState(MongoId traderId, bool status, MongoId sessionId)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);
        if (!pmcData.TradersInfo.TryGetValue(traderId, out var profileTraderData))
        {
            logger.Error($"Unable to set trader: {traderId} unlocked state to: {status} as trader cannot be found in profile");

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
    public void AddStandingToTrader(MongoId sessionId, MongoId traderId, double standingToAdd)
    {
        var fullProfile = profileHelper.GetFullProfile(sessionId);
        var pmcTraderInfo = fullProfile.CharacterData.PmcData.TradersInfo[traderId];

        // Add standing to trader
        pmcTraderInfo.Standing = AddStandingValuesTogether(pmcTraderInfo.Standing, standingToAdd);

        if (traderId == Traders.FENCE)
        // Must add rep to scav profile to preserve consistency
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
    public void ValidateTraderStandingsAndPlayerLevelForProfile(MongoId sessionId)
    {
        var profile = profileHelper.GetPmcProfile(sessionId);
        var traders = databaseService.GetTraders();
        foreach (var (traderId, _) in traders)
        {
            LevelUp(traderId, profile);
        }
    }

    /// <summary>
    ///     Calculate trader's level based on experience amount and increments level if over threshold.
    ///     Also validates and updates player level if not correct based on XP value.
    /// </summary>
    /// <param name="traderId">Trader to check standing of.</param>
    /// <param name="pmcData">Profile to update trader in.</param>
    public void LevelUp(MongoId traderId, PmcData pmcData)
    {
        var loyaltyLevels = databaseService.GetTrader(traderId).Base.LoyaltyLevels;

        // Level up player
        pmcData.Info.Level = pmcData.CalculateLevel(databaseService.GetGlobals().Configuration.Exp.Level.ExperienceTable);

        // Level up traders
        var targetLevel = 0;

        // Round standing to 2 decimal places to address floating point inaccuracies
        pmcData.TradersInfo[traderId].Standing = Math.Round(pmcData.TradersInfo[traderId].Standing * 100 ?? 0, 2) / 100;

        foreach (var loyaltyLevel in loyaltyLevels)
        {
            if (
                loyaltyLevel.MinLevel <= pmcData.Info.Level
                && loyaltyLevel.MinSalesSum <= pmcData.TradersInfo[traderId].SalesSum
                && loyaltyLevel.MinStanding <= pmcData.TradersInfo[traderId].Standing
                && targetLevel < 4
            )
            // Level reached
            {
                targetLevel++;
            }
        }

        // set level
        pmcData.TradersInfo[traderId].LoyaltyLevel = targetLevel;
    }

    /// <summary>
    ///     Get the next update timestamp for a trader.
    /// </summary>
    /// <param name="traderId">Trader to look up update value for.</param>
    /// <returns>Future timestamp.</returns>
    public long GetNextUpdateTimestamp(MongoId traderId)
    {
        var updateSeconds = GetTraderUpdateSeconds(traderId) ?? 0;
        return timeUtil.GetTimeStamp() + updateSeconds;
    }

    /// <summary>
    ///     Get the reset time between trader assort refreshes in seconds.
    /// </summary>
    /// <param name="traderId">Trader to look up.</param>
    /// <returns>Time in seconds.</returns>
    public long? GetTraderUpdateSeconds(MongoId traderId)
    {
        var traderDetails = TraderConfig.UpdateTime.FirstOrDefault(x => x.TraderId == traderId);
        if (traderDetails?.Seconds?.Min is null || traderDetails.Seconds?.Max is null)
        {
            logger.Warning(
                serverLocalisationService.GetText(
                    "trader-missing_trader_details_using_default_refresh_time",
                    new { traderId, updateTime = TraderConfig.UpdateTimeDefault }
                )
            );

            TraderConfig.UpdateTime.Add(
                new UpdateTime
                // create temporary entry to prevent logger spam
                {
                    TraderId = traderId,
                    Seconds = new MinMax<int>(TraderConfig.UpdateTimeDefault, TraderConfig.UpdateTimeDefault),
                }
            );

            return null;
        }

        return randomUtil.GetInt(traderDetails.Seconds.Min, traderDetails.Seconds.Max);
    }

    /// <summary>
    /// Get the loyalty level object a profile has with specified trader
    /// </summary>
    /// <param name="traderId">Trader id to get loyalty level of</param>
    /// <param name="pmcData">Profile to look for loyalty data in</param>
    /// <returns>TraderLoyaltyLevel</returns>
    public TraderLoyaltyLevel GetLoyaltyLevel(MongoId traderId, PmcData pmcData)
    {
        var traderBase = databaseService.GetTrader(traderId).Base;

        int? loyaltyLevel = null;
        if (pmcData.TradersInfo.TryGetValue(traderId, out var traderInfo))
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

        return traderBase.LoyaltyLevels[loyaltyLevel.Value - 1];
    }

    /// <summary>
    ///     Store the purchase of an assort from a trader in the player profile
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="newPurchaseDetails">New item assort id + count</param>
    /// <param name="itemPurchased">Item purchased</param>
    public void AddTraderPurchasesToPlayerProfile(MongoId sessionID, PurchaseDetails newPurchaseDetails, Item itemPurchased)
    {
        var profile = profileHelper.GetFullProfile(sessionID);
        var traderId = newPurchaseDetails.TraderId;

        // Iterate over assorts bought and add to profile
        foreach (var purchasedItem in newPurchaseDetails.Items)
        {
            var currentTime = timeUtil.GetTimeStamp();

            // Nullguard traderPurchases
            profile.TraderPurchases ??= new();
            // Nullguard traderPurchases for this trader
            profile.TraderPurchases[traderId] ??= new();

            // Null guard when dict doesn't exist

            if (
                profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseCount is null
                || profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseTimestamp is null
            )
            {
                profile.TraderPurchases[traderId][purchasedItem.ItemId] = new TraderPurchaseData
                {
                    PurchaseCount = purchasedItem.Count,
                    PurchaseTimestamp = currentTime,
                };

                continue;
            }

            if (
                profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseCount + purchasedItem.Count
                > GetAccountTypeAdjustedTraderPurchaseLimit(
                    (double)itemPurchased.Upd.BuyRestrictionMax,
                    profile.CharacterData.PmcData.Info.GameVersion
                )
            )
            {
                throw new Exception(
                    serverLocalisationService.GetText(
                        "trader-unable_to_purchase_item_limit_reached",
                        new { traderId, limit = itemPurchased.Upd.BuyRestrictionMax }
                    )
                );
            }

            profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseCount += purchasedItem.Count;
            profile.TraderPurchases[traderId][purchasedItem.ItemId].PurchaseTimestamp = currentTime;
        }
    }

    /// <summary>
    ///     EoD and Unheard get a 20% bonus (1.2) to personal trader limit purchases
    /// </summary>
    /// <param name="buyRestrictionMax">Existing value from trader item</param>
    /// <param name="gameVersion">Profiles game version</param>
    /// <returns>buyRestrictionMax value</returns>
    public double GetAccountTypeAdjustedTraderPurchaseLimit(double buyRestrictionMax, string gameVersion)
    {
        if (GameVersionsWithHigherBuyRestrictions.Contains(gameVersion))
        {
            // TODO: move value into config
            return Math.Floor(buyRestrictionMax * 1.2);
        }

        return buyRestrictionMax;
    }

    /// <summary>
    ///     Get the highest price item can be sold to trader for (roubles)
    /// </summary>
    /// <param name="tpl">Item to look up best trader sell-to price</param>
    /// <returns>Rouble price</returns>
    public double GetHighestSellToTraderPrice(MongoId tpl)
    {
        lock (_highestPriceLock)
        {
            if (!HighestTraderPriceItems.TryGetValue(tpl, out var highestPrice))
            {
                highestPrice = 1d; // Default price
                var itemHandbookPrice = handbookHelper.GetTemplatePrice(tpl);
                foreach (var (_, trader) in databaseService.GetTraders())
                {
                    // Get trader and check buy category allows tpl
                    var traderBase = trader.Base;

                    if (traderBase is null)
                    {
                        continue;
                    }

                    // Get loyalty level details player has achieved with this trader
                    // Uses lowest loyalty level as this function is used before a player has logged into server
                    // We have no idea what player loyalty is with traders
                    var traderBuyBackPricePercent = 100 - traderBase.LoyaltyLevels.FirstOrDefault().BuyPriceCoefficient;

                    var priceTraderBuysItemAt = randomUtil.GetPercentOfValue(traderBuyBackPricePercent ?? 0, itemHandbookPrice, 0);

                    // Price from this trader is higher than highest found, update
                    if (priceTraderBuysItemAt > highestPrice)
                    {
                        highestPrice = priceTraderBuysItemAt;
                        HighestTraderPriceItems[tpl] = highestPrice;
                    }
                }
            }

            return highestPrice;
        }
    }

    /// <summary>
    ///     Does the provided ID exist in db as a trader id
    /// </summary>
    /// <param name="traderId">Trader id</param>
    /// <returns>True if a Trader exists with given ID</returns>
    public bool TraderExists(MongoId traderId)
    {
        return databaseService.GetTrader(traderId) != null;
    }
}
