using Core.Annotations;
using Core.Models.Eft.Profile;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class TraderPurchasePersisterService
{
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
        throw new NotImplementedException();
    }
}
