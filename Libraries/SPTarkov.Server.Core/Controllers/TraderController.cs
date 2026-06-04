using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Traders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Services.Ragfair;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class TraderController(
    ISptLogger<TraderController> logger,
    TradersTable traderTable,
    TimeUtil timeUtil,
    TraderAssortHelper traderAssortHelper,
    ProfileHelper profileHelper,
    TraderHelper traderHelper,
    PaymentHelper paymentHelper,
    RagfairPriceService ragfairPriceService,
    TraderPurchasePersisterService traderPurchasePersisterService,
    FenceService fenceService,
    FenceBaseAssortGenerator fenceBaseAssortGenerator,
    TraderConfig traderConfig
)
{
    /// <summary>
    ///     Runs when onLoad event is fired
    ///     Iterate over traders, ensure a pristine copy of their assorts is stored in traderAssortService
    ///     Store timestamp of next assort refresh in nextResupply property of traders .base object
    /// </summary>
    public void Load()
    {
        var nextHourTimestamp = timeUtil.GetTimeStampOfNextHour();
        var traderResetStartsWithServer = traderConfig.TradersResetFromServerStart;

        foreach (var (traderId, trader) in traderTable)
        {
            if (traderId == Traders.LIGHTHOUSEKEEPER)
            {
                continue;
            }

            if (traderId == Traders.FENCE)
            {
                fenceBaseAssortGenerator.GenerateFenceBaseAssorts();
                fenceService.GenerateFenceAssorts();
                continue;
            }

            // Adjust price by traderPriceMultiplier config property
            if (!traderConfig.TraderPriceMultiplier.Approx(1))
            {
                AdjustTraderItemPrices(trader, traderConfig.TraderPriceMultiplier);
            }

            traderPurchasePersisterService.RemoveStalePurchasesFromProfiles(traderId);

            // Set to next hour on clock or current time + 60 minutes
            trader.Base.NextResupply = traderResetStartsWithServer
                ? (int)traderHelper.GetNextUpdateTimestamp(trader.Base.Id)
                : (int)nextHourTimestamp;
        }
    }

    /// <summary>
    ///     Adjust trader item prices based on config value multiplier
    ///     only applies to items sold for currency
    /// </summary>
    /// <param name="trader">Trader to adjust prices of</param>
    /// <param name="multiplier">Coef to apply to traders' items' prices</param>
    protected void AdjustTraderItemPrices(Trader trader, double multiplier)
    {
        foreach (var kvp in trader.Assort?.BarterScheme)
        {
            var barterSchemeItem = kvp.Value.FirstOrDefault()?.FirstOrDefault();
            if (barterSchemeItem?.Template != null && paymentHelper.IsMoneyTpl(barterSchemeItem.Template))
            {
                barterSchemeItem.Count += Math.Round(barterSchemeItem?.Count * multiplier ?? 0D, 2);
            }
        }
    }

    /// <summary>
    ///     Runs when onUpdate is fired
    ///     If current time is > nextResupply(expire) time of trader, refresh traders assorts and
    ///     Fence is handled slightly differently
    /// </summary>
    /// <returns>True if ran successfully</returns>
    public bool Update()
    {
        foreach (var (traderId, trader) in traderTable)
        {
            if (traderId == Traders.LIGHTHOUSEKEEPER)
            {
                continue;
            }

            if (traderId == Traders.FENCE)
            {
                if (fenceService.NeedsPartialRefresh())
                {
                    fenceService.GenerateFenceAssorts();
                }

                continue;
            }

            if (!traderAssortHelper.TraderAssortsHaveExpired(traderId))
            {
                // Trader is still active, nothing else to do
                continue;
            }

            // Trader needs to be refreshed
            traderAssortHelper.ResetExpiredTrader(trader);

            // Reset purchase data per trader as they have independent reset times
            traderPurchasePersisterService.ResetTraderPurchasesStoredInProfile(traderId);
        }

        return true;
    }

    /// <summary>
    ///     Handle client/items/prices/TRADERID
    /// </summary>
    /// <returns></returns>
    public GetItemPricesResponse GetItemPrices(MongoId sessionId, MongoId traderId)
    {
        var handbookPrices = ragfairPriceService.GetAllStaticPrices();

        return new GetItemPricesResponse
        {
            SupplyNextTime = traderHelper.GetNextUpdateTimestamp(traderId),
            Prices = handbookPrices,
            CurrencyCourses = new Dictionary<string, double>
            {
                { Money.ROUBLES, handbookPrices[Money.ROUBLES] },
                { Money.EUROS, handbookPrices[Money.EUROS] },
                { Money.DOLLARS, handbookPrices[Money.DOLLARS] },
            },
        };
    }
}
