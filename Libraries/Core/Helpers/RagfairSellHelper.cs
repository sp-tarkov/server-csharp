using SptCommon.Annotations;
using Core.Models.Eft.Ragfair;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Models.Utils;
using Core.Services;
using Core.Utils;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Helpers;

[Injectable]
public class RagfairSellHelper(
    ISptLogger<RagfairSellHelper> _logger,
    TimeUtil _timeUtil,
    RandomUtil _randomUtil,
    DatabaseService _databaseService,
    ConfigServer _configServer)
{
    protected RagfairConfig _ragfairConfig = _configServer.GetConfig<RagfairConfig>();

    /// <summary>
    /// Get the percent chance to sell an item based on its average listed price vs player chosen listing price
    /// </summary>
    /// <param name="averageOfferPriceRub">Price of average offer in roubles</param>
    /// <param name="playerListedPriceRub">Price player listed item for in roubles</param>
    /// <param name="qualityMultiplier">Quality multiplier of item being sold</param>
    /// <returns>percent value</returns>
    public double CalculateSellChance(
        double averageOfferPriceRub,
        double playerListedPriceRub,
        double qualityMultiplier)
    {
        var sellConfig = _ragfairConfig.Sell.Chance;

        // Base sell chance modified by items quality
        var baseSellChancePercent = sellConfig.Base * qualityMultiplier;

        // Modifier gets applied twice to either penalize or incentivize over/under pricing (Probably a cleaner way to do this)
        var sellModifier = averageOfferPriceRub / playerListedPriceRub * sellConfig.SellMultiplier;
        var sellChance = Math.Round(baseSellChancePercent * sellModifier * Math.Pow(sellModifier, 3) + 10); // Power of 3

        // Adjust sell chance if below config value
        if (sellChance < sellConfig.MinSellChancePercent) sellChance = sellConfig.MinSellChancePercent;

        // Adjust sell chance if above config value
        if (sellChance > sellConfig.MaxSellChancePercent) sellChance = sellConfig.MaxSellChancePercent;

        return sellChance;
    }

    /// <summary>
    /// Get list of item count and sell time (empty list = no sell)
    /// </summary>
    /// <param name="sellChancePercent">chance item will sell</param>
    /// <param name="itemSellCount">count of items to sell</param>
    /// <param name="sellInOneGo">All items listed get sold at once</param>
    /// <returns>List of purchases of item(s) listed</returns>
    public List<SellResult> RollForSale(double? sellChancePercent, int itemSellCount, bool sellInOneGo = false)
    {
        var startTimestamp = _timeUtil.GetTimeStamp();

        // Get a time in future to stop simulating sell chances at
        var endTime =
            startTimestamp +
            _timeUtil.GetHoursAsSeconds((int)_databaseService.GetGlobals().Configuration.RagFair.OfferDurationTimeInHour.Value);

        var sellTimestamp = startTimestamp;
        var remainingCount = itemSellCount;
        var result = new List<SellResult>();

        var effectiveSellChance = sellChancePercent;

        if (sellChancePercent is null)
        {
            effectiveSellChance = _ragfairConfig.Sell.Chance.Base;
            _logger.Warning($"Sell chance was not a number: {sellChancePercent}, defaulting to {_ragfairConfig.Sell.Chance.Base}%");
        }

        if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Rolling to sell: {itemSellCount}items(chance: {effectiveSellChance}%)");

        // No point rolling for a sale on a 0% chance item, exit early
        if (effectiveSellChance == 0) return result;

        while (remainingCount > 0 && sellTimestamp < endTime)
        {
            var boughtAmount = sellInOneGo ? remainingCount : _randomUtil.GetInt(1, remainingCount);
            if (_randomUtil.GetChance100(effectiveSellChance))
            {
                // Passed roll check, item will be sold
                // Weight time to sell towards selling faster based on how cheap the item sold
                var weighting = (100 - effectiveSellChance) / 100;
                var maximumTime = weighting * _ragfairConfig.Sell.Time.Max * 60;
                var minimumTime = _ragfairConfig.Sell.Time.Min * 60;
                if (maximumTime < minimumTime) maximumTime = minimumTime + 5;
                // Sell time will be random between min/max
                var random = new Random();
                var newSellTime = Math.Floor(random.NextDouble() * (maximumTime.Value - minimumTime.Value) + minimumTime.Value);
                if (newSellTime == 0)
                    // Ensure all sales don't occur the same exact time
                    newSellTime += 1;
                sellTimestamp += (long)newSellTime;
                result.Add(new SellResult { SellTime = sellTimestamp, Amount = boughtAmount });

                if (_logger.IsLogEnabled(LogLevel.Debug))
                    _logger.Debug($"Offer will sell at: {_timeUtil.GetDateTimeFromTimeStamp(sellTimestamp).ToLocalTime().ToString()}, bought: {boughtAmount}");
            }
            else
            {
                if (_logger.IsLogEnabled(LogLevel.Debug)) _logger.Debug($"Offer rolled not to sell, item count: {boughtAmount}");
            }

            remainingCount -= boughtAmount;
        }

        return result;
    }
}
