using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class TraderPurchasePersisterService(
    ISptLogger<TraderPurchasePersisterService> logger,
    RandomUtil randomUtil,
    TimeUtil timeUtil,
    ProfileHelper profileHelper,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer
)
{
    protected readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();

    /// <summary>
    ///     Get the purchases made from a trader for this profile before the last trader reset
    /// </summary>
    /// <param name="sessionId"> Session id </param>
    /// <param name="traderId"> Trader to loop up purchases for </param>
    /// <returns> Dictionary of assort id and count purchased </returns>
    public Dictionary<MongoId, TraderPurchaseData>? GetProfileTraderPurchases(MongoId sessionId, MongoId traderId)
    {
        var profile = profileHelper.GetFullProfile(sessionId);

        return profile?.TraderPurchases?.GetValueOrDefault(traderId);
    }

    /// <summary>
    ///     Get a purchase made from a trader for requested profile before the last trader reset
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="traderId"> Trader to loop up purchases for </param>
    /// <param name="assortId"> ID of assort to get data for </param>
    /// <returns> TraderPurchaseData </returns>
    public TraderPurchaseData? GetProfileTraderPurchase(MongoId sessionId, MongoId traderId, string assortId)
    {
        var profile = profileHelper.GetFullProfile(sessionId);

        if (profile.TraderPurchases is null)
        {
            return null;
        }

        if (!profile.TraderPurchases.TryGetValue(traderId, out _))
        {
            profile.TraderPurchases.TryAdd(traderId, new());
        }

        var traderPurchases = profile.TraderPurchases[traderId];

        if (!traderPurchases.TryGetValue(assortId, out _))
        {
            traderPurchases.TryAdd(assortId, new TraderPurchaseData());
        }

        return traderPurchases[assortId];
    }

    /// <summary>
    ///     Remove all trader purchase records from all profiles that exist
    /// </summary>
    /// <param name="traderId"> Traders ID </param>
    public void ResetTraderPurchasesStoredInProfile(MongoId traderId)
    {
        // Reset all profiles purchase dictionaries now a trader update has occured;
        var profiles = profileHelper.GetProfiles();
        foreach (var profile in profiles)
        {
            // Skip if no purchases
            if (profile.Value.TraderPurchases is null)
            {
                continue;
            }

            // Skip if no trader-specific purchases
            if (!profile.Value.TraderPurchases.TryGetValue(traderId, out _))
            {
                continue;
            }

            profile.Value.TraderPurchases[traderId] = new();
        }

        logger.Debug($"Reset trader: {traderId} assort buy limits");
    }

    /// <summary>
    ///     Iterate over all server profiles and remove specific trader purchase data that has passed the trader refresh time
    /// </summary>
    /// <param name="traderId"> Trader ID </param>
    public void RemoveStalePurchasesFromProfiles(MongoId traderId)
    {
        var profiles = profileHelper.GetProfiles();
        foreach (var profileKvP in profiles)
        {
            var profile = profileKvP.Value;

            // Skip if no purchases or no trader-specific purchases
            var purchasesFromTrader = profile.TraderPurchases?.GetValueOrDefault(traderId, null);
            if (purchasesFromTrader is null)
            {
                continue;
            }

            foreach (var purchaseKvP in purchasesFromTrader)
            {
                var traderUpdateDetails = _traderConfig.UpdateTime.FirstOrDefault(x => x.TraderId == traderId);
                if (traderUpdateDetails is null)
                {
                    logger.Error(
                        serverLocalisationService.GetText(
                            "trader-unable_to_delete_stale_purchases",
                            new { profileId = profile.ProfileInfo.ProfileId, traderId }
                        )
                    );

                    continue;
                }

                var purchaseDetails = purchaseKvP.Value;
                var resetTimeForItem =
                    purchaseDetails.PurchaseTimestamp
                    + randomUtil.GetDouble(traderUpdateDetails.Seconds.Min, traderUpdateDetails.Seconds.Max);
                if (resetTimeForItem < timeUtil.GetTimeStamp())
                {
                    // Item was purchased far enough in past a trader refresh would have occured, remove purchase record from profile
                    logger.Debug($"Removed trader: {traderId} purchase: {purchaseKvP} from profile: {profile.ProfileInfo.ProfileId}");

                    profile.TraderPurchases[traderId].Remove(purchaseKvP.Key);
                }
            }
        }
    }
}
