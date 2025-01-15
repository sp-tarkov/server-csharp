using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class TraderPurchasePersisterService
{
    private readonly ILogger _logger;
    private readonly RandomUtil _randomUtil;
    private readonly TimeUtil _timeUtil;
    private readonly ProfileHelper _profileHelper;
    private readonly LocalisationService _localisationService;
    private readonly ConfigServer _configServer;

    private readonly TraderConfig _traderConfig;

    public TraderPurchasePersisterService(
        ILogger logger,
        RandomUtil randomUtil,
        TimeUtil timeUtil,
        ProfileHelper profileHelper,
        LocalisationService localisationService,
        ConfigServer configServer)
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _timeUtil = timeUtil;
        _profileHelper = profileHelper;
        _localisationService = localisationService;
        _configServer = configServer;

        _traderConfig = _configServer.GetConfig<TraderConfig>();
    }

    /**
     * Get the purchases made from a trader for this profile before the last trader reset
     * @param sessionId Session id
     * @param traderId Trader to loop up purchases for
     * @returns Dictionary of assort id and count purchased
     */
    public Dictionary<string, TraderPurchaseData> GetProfileTraderPurchases(
        string sessionId,
        string traderId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a purchase made from a trader for requested profile before the last trader reset
     * @param sessionId Session id
     * @param traderId Trader to loop up purchases for
     * @param assortId Id of assort to get data for
     * @returns TraderPurchaseData
     */
    public TraderPurchaseData GetProfileTraderPurchase(
        string sessionId,
        string traderId,
        string assortId)
    {
        throw new NotImplementedException();
    }

    /**
     * Remove all trader purchase records from all profiles that exist
     * @param traderId Traders id
     */
    public void ResetTraderPurchasesStoredInProfile(string traderId)
    {
        throw new NotImplementedException();
    }

    /**
     * Iterate over all server profiles and remove specific trader purchase data that has passed the trader refresh time
     * @param traderId Trader id
     */
    public void RemoveStalePurchasesFromProfiles(string traderId)
    {
        var profiles = _profileHelper.GetProfiles();
        foreach (var profileKvP in profiles) {
            var profile = profileKvP.Value;

            // Skip if no purchases or no trader-specific purchases
            var purchasesFromTrader = profile.TraderPurchases?.GetValueOrDefault(traderId, null);
            if (purchasesFromTrader is null)
            {
                continue;
            }

            foreach (var purchaseKvP in purchasesFromTrader) {
                var traderUpdateDetails = _traderConfig.UpdateTime.FirstOrDefault((x) => x.TraderId == traderId);
                if (traderUpdateDetails is null)
                {
                    _logger.Error(
                        _localisationService.GetText("trader-unable_to_delete_stale_purchases", new {
                        profileId = profile.ProfileInfo.ProfileId,
                        traderId = traderId,
                    })
                    );

                    continue;
                }

                var purchaseDetails = purchaseKvP.Value;
                var resetTimeForItem =
                purchaseDetails.PurchaseTimestamp +
                    _randomUtil.GetInt((int)traderUpdateDetails.Seconds.Min, (int)traderUpdateDetails.Seconds.Max);
                if (resetTimeForItem < _timeUtil.GetTimeStamp())
                {
                    // Item was purchased far enough in past a trader refresh would have occured, remove purchase record from profile
                    _logger.Debug($"Removed trader: { traderId} purchase: { purchaseKvP} from profile: { profile.ProfileInfo.ProfileId}");

                    profile.TraderPurchases.Remove(purchaseKvP.Key);
                }
            }
        }
    }
}
