using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Services.Ragfair;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Servers;

[Injectable]
public sealed class RagfairServer(
    ISptLogger<RagfairServer> logger,
    TimeUtil timeUtil,
    RagfairOfferService ragfairOfferService,
    RagfairRequiredItemsService ragfairRequiredItemsService,
    ServerLocalisationService serverLocalisationService,
    RagfairOfferGenerator ragfairOfferGenerator,
    RagfairOfferHolder ragfairOfferHolder,
    RagfairConfig ragfairConfig,
    ICloner cloner
)
{
    public void Load()
    {
        logger.Info(serverLocalisationService.GetText("ragfair-generating_offers"));
        ragfairOfferGenerator.GenerateDynamicOffers();
        Update();
    }

    public void Update()
    {
        RefreshTraderOffers();
        ProcessExpiredFleaOffers();

        // Flag data as stale and in need of regeneration
        ragfairRequiredItemsService.InvalidateCache();
    }

    private void RefreshTraderOffers()
    {
        // Generate/refresh trader offers - skip fence as his offers are separately handled
        var tradersToProcess = ragfairConfig.Traders.Keys.ToList().Where(trader => trader != Traders.FENCE);
        foreach (var traderId in tradersToProcess)
        {
            // Each trader has its own expiry time
            if (ragfairOfferService.TraderOffersNeedRefreshing(traderId))
            {
                // Trader has passed its offer expiry time, update stock and reset offer times
                ragfairOfferGenerator.GenerateFleaOffersForTrader(traderId);
            }
        }
    }

    private void ProcessExpiredFleaOffers()
    {
        // Regenerate expired offers when over timestamp threshold
        ragfairOfferHolder.FlagExpiredOffersAfterDate(timeUtil.GetTimeStamp());

        if (!ragfairOfferService.EnoughExpiredOffersExistToProcess())
        {
            // Not enough expired offers to process, exit
            return;
        }

        // Must occur BEFORE "RemoveExpiredOffers" + clone items as they'll be purged by `RemoveExpiredOffers()`
        var expiredOfferItemsClone = cloner.Clone(ragfairOfferHolder.GetExpiredOfferItems());

        ragfairOfferService.RemoveExpiredOffers();

        // Force a cleanup+compact now all the expired offers are gone
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, true, true);

        if (expiredOfferItemsClone is not null)
        {
            // Replace the expired offers with new ones
            ragfairOfferGenerator.GenerateDynamicOffers(expiredOfferItemsClone);
        }
    }
}
