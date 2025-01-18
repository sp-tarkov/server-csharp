using Core.Annotations;
using Core.Models.Common;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;


namespace Core.Helpers;

[Injectable]
public class TraderHelper(
    ISptLogger<TraderHelper> _logger,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    LocalisationService _localisationService,
    ConfigServer _configServer,
    ProfileHelper _profileHelper,
    DatabaseService _databaseService
)
{
    protected TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();
    private Dictionary<string, int> _highestTraderPriceItems = new();

    /// <summary>
    /// Get a trader base object, update profile to reflect players current standing in profile
    /// when trader not found in profile
    /// </summary>
    /// <param name="traderID">Traders Id to get</param>
    /// <param name="sessionID">Players id</param>
    /// <returns>Trader base</returns>
    public TraderBase GetTrader(string traderID, string sessionID)
    {
        if (traderID == "ragfair")
        {
            return new()
            {
                Currency = "RUB"
            };
        }

        var pmcData = _profileHelper.GetPmcProfile(sessionID);
        if (pmcData == null)
            throw new Exception(_localisationService.GetText("trader-unable_to_find_profile_with_id", sessionID));

        // Profile has traderInfo dict (profile beyond creation stage) but no requested trader in profile
        if (pmcData?.TradersInfo != null && (pmcData?.TradersInfo?.ContainsKey(traderID) ?? false))
        {
            // Add trader values to profile
            ResetTrader(sessionID, traderID);
            LevelUp(traderID, pmcData);
        }

        var traderBase = _databaseService.GetTrader(traderID).Base;
        if (traderBase == null)
            _logger.Error(_localisationService.GetText("trader-unable_to_find_trader_by_id", traderID));

        return traderBase;
    }

    /// <summary>
    /// Get all assort data for a particular trader
    /// </summary>
    /// <param name="traderId">Trader to get assorts for</param>
    /// <returns>TraderAssort</returns>
    public TraderAssort GetTraderAssortsByTraderId(string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve the Item from a traders assort data by its id
    /// </summary>
    /// <param name="traderId">Trader to get assorts for</param>
    /// <param name="assortId">Id of assort to find</param>
    /// <returns>Item object</returns>
    public Item GetTraderAssortItemByAssortId(string traderId, string assortId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Reset a profiles trader data back to its initial state as seen by a level 1 player
    /// Does NOT take into account different profile levels
    /// </summary>
    /// <param name="sessionID">session id of player</param>
    /// <param name="traderID">trader id to reset</param>
    public void ResetTrader(string sessionID, string traderID)
    {
        // TODO: implement actually
        return;
    }

    /// <summary>
    /// Get the starting standing of a trader based on the current profiles type (e.g. EoD, Standard etc)
    /// </summary>
    /// <param name="traderId">Trader id to get standing for</param>
    /// <param name="rawProfileTemplate">Raw profile from profiles.json to look up standing from</param>
    /// <returns>Standing value</returns>
    protected double GetStartingStanding(string traderId, ProfileTraderTemplate rawProfileTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a list of suit ids to a profiles suit list, no duplicates
    /// </summary>
    /// <param name="fullProfile">Profile to add to</param>
    /// <param name="suitIds">Suit Ids to add</param>
    protected void AddSuitsToProfile(SptProfile fullProfile, List<string> suitIds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Alter a traders unlocked status
    /// </summary>
    /// <param name="traderId">Trader to alter</param>
    /// <param name="status">New status to use</param>
    /// <param name="sessionId">Session id of player</param>
    public void SetTraderUnlockedState(string traderId, bool status, string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add standing to a trader and level them up if exp goes over level threshold
    /// </summary>
    /// <param name="sessionId">Session id of player</param>
    /// <param name="traderId">Traders id to add standing to</param>
    /// <param name="standingToAdd">Standing value to add to trader</param>
    public void AddStandingToTrader(string sessionId, string traderId, double standingToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add standing to current standing and clamp value if it goes too low
    /// </summary>
    /// <param name="currentStanding">current trader standing</param>
    /// <param name="standingToAdd">standing to add to trader standing</param>
    /// <returns>current standing + added standing (clamped if needed)</returns>
    protected double AddStandingValuesTogether(double currentStanding, double standingToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over a profile's traders and ensure they have the correct loyalty level for the player.
    /// </summary>
    /// <param name="sessionId">Profile to check.</param>
    public void ValidateTraderStandingsAndPlayerLevelForProfile(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate trader's level based on experience amount and increments level if over threshold.
    /// Also validates and updates player level if not correct based on XP value.
    /// </summary>
    /// <param name="traderID">Trader to check standing of.</param>
    /// <param name="pmcData">Profile to update trader in.</param>
    public void LevelUp(string traderID, PmcData pmcData)
    {
        // TODO: implement actually
        return;
    }

    /// <summary>
    /// Get the next update timestamp for a trader.
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
    /// Get the reset time between trader assort refreshes in seconds.
    /// </summary>
    /// <param name="traderId">Trader to look up.</param>
    /// <returns>Time in seconds.</returns>
    public long? GetTraderUpdateSeconds(string traderId)
    {
        var traderDetails = _traderConfig.UpdateTime.FirstOrDefault((x) => x.TraderId == traderId);
        if (traderDetails is null || traderDetails.Seconds?.Min is null || traderDetails.Seconds.Max is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "trader-missing_trader_details_using_default_refresh_time",
                    new
                    {
                        traderId = traderId,
                        updateTime = _traderConfig.UpdateTimeDefault,
                    }
                )
            );

            _traderConfig.UpdateTime.Add(
                new UpdateTime
                    // create temporary entry to prevent logger spam
                    {
                        TraderId = traderId,
                        Seconds = new MinMax { Min = _traderConfig.UpdateTimeDefault, Max = _traderConfig.UpdateTimeDefault }
                    }
            );

            return null;
        }

        return _randomUtil.GetInt((int)traderDetails.Seconds.Min, (int)traderDetails.Seconds.Max);
    }

    public TraderLoyaltyLevel GetLoyaltyLevel(string traderID, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Store the purchase of an assort from a trader in the player profile
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="newPurchaseDetails">New item assort id + count</param>
    public void AddTraderPurchasesToPlayerProfile(
        string sessionID,
        object newPurchaseDetails, // TODO: TYPE FUCKEY { items: { itemId: string; count: number }[]; traderId: string }
        Item itemPurchased)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// EoD and Unheard get a 20% bonus to personal trader limit purchases
    /// </summary>
    /// <param name="buyRestrictionMax">Existing value from trader item</param>
    /// <param name="gameVersion">Profiles game version</param>
    /// <returns>buyRestrictionMax value</returns>
    public double GetAccountTypeAdjustedTraderPurchaseLimit(double buyRestrictionMax, string gameVersion)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the highest rouble price for an item from traders
    /// UNUSED
    /// </summary>
    /// <param name="tpl">Item to look up highest price for</param>
    /// <returns>highest rouble cost for item</returns>
    public double GetHighestTraderPriceRouble(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the highest price item can be sold to trader for (roubles)
    /// </summary>
    /// <param name="tpl">Item to look up best trader sell-to price</param>
    /// <returns>Rouble price</returns>
    public double GetHighestSellToTraderPrice(string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a trader enum key by its value
    /// </summary>
    /// <param name="traderId">Traders id</param>
    /// <returns>Traders key</returns>
    public Trader GetTraderById(string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates that the provided traderEnumValue exists in the Traders enum. If the value is valid, it returns the
    /// same enum value, effectively serving as a trader ID; otherwise, it logs an error and returns an empty string.
    /// This method provides a runtime check to prevent undefined behavior when using the enum as a dictionary key.
    /// 
    /// For example, instead of this:
    /// const traderId = Traders[Traders.PRAPOR];
    /// 
    /// You can use safely use this:
    /// const traderId = this.traderHelper.getValidTraderIdByEnumValue(Traders.PRAPOR);
    /// 
    /// </summary>
    /// <param name="traderEnumValue">The trader enum value to validate</param>
    /// <returns>The validated trader enum value as a string, or an empty string if invalid</returns>
    public string GetValidTraderIdByEnumValue(string traderEnumValue) // TODO: param was Traders
    {
        var traderId = _databaseService.GetTraders();
        var id = traderId.FirstOrDefault(x => x.Value.Base.Nickname.ToLower() == traderEnumValue.ToLower()).Key;
        return id;
    }

    /// <summary>
    /// Does the 'Traders' enum has a value that matches the passed in parameter
    /// </summary>
    /// <param name="key">Value to check for</param>
    /// <returns>True, values exists in Traders enum as a value</returns>
    public bool TraderEnumHasKey(string key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Accepts a trader id
    /// </summary>
    /// <param name="traderId">Trader id</param>
    /// <returns>True if Traders enum has the param as a value</returns>
    public bool TraderEnumHasValue(string traderId)
    {
        _logger.Error("HACK TraderEnumHasValue");
        return Traders.TradersDictionary.ContainsValue(traderId);
    }
}
