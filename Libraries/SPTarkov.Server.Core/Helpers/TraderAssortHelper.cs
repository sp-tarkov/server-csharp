using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class TraderAssortHelper(
    ISptLogger<TraderAssortHelper> logger,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    ProfileHelper profileHelper,
    AssortHelper assortHelper,
    TraderPurchasePersisterService traderPurchasePersisterService,
    TraderHelper traderHelper,
    FenceService fenceService,
    ICloner cloner
)
{
    private Dictionary<string, Dictionary<MongoId, MongoId>>? _mergedQuestAssorts;
    protected virtual Dictionary<string, Dictionary<MongoId, MongoId>> MergedQuestAssorts
    {
        get { return _mergedQuestAssorts ??= HydrateMergedQuestAssorts(); }
    }

    /// <summary>
    ///     Get a traders assorts
    ///     Can be used for returning ragfair / fence assorts
    ///     Filter out assorts not unlocked due to level OR quest completion
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <param name="traderId">traders id</param>
    /// <param name="showLockedAssorts">Should assorts player hasn't unlocked be returned - default false</param>
    /// <returns>a traders' assorts</returns>
    public TraderAssort GetAssort(MongoId sessionId, MongoId traderId, bool showLockedAssorts = false)
    {
        var traderClone = cloner.Clone(databaseService.GetTrader(traderId));
        var fullProfile = profileHelper.GetFullProfile(sessionId);
        var pmcProfile = fullProfile?.CharacterData?.PmcData;

        if (traderId == Traders.FENCE)
        {
            return fenceService.GetFenceAssorts(pmcProfile);
        }

        // Strip assorts player should not see yet
        if (!showLockedAssorts)
        {
            traderClone.Assort = assortHelper.StripLockedLoyaltyAssort(pmcProfile, traderId, traderClone.Assort);
        }

        ResetBuyRestrictionCurrentValue(traderClone.Assort.Items);

        // Append nextResupply value to assorts so client knows when refresh is occuring
        traderClone.Assort.NextResupply = traderClone.Base.NextResupply;

        // Adjust displayed assort counts based on values stored in profile
        var assortPurchasesFromTrader = traderPurchasePersisterService.GetProfileTraderPurchases(sessionId, traderId);

        foreach (var assortId in assortPurchasesFromTrader ?? [])
        {
            // Find assort we want to update current buy count of
            var assortToAdjust = traderClone.Assort.Items.FirstOrDefault(x => x.Id == assortId.Key);
            if (assortToAdjust is null)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        $"Cannot find trader: {traderClone.Base.Nickname} assort: {assortId} to adjust BuyRestrictionCurrent value, skipping"
                    );
                }

                continue;
            }

            if (assortToAdjust.Upd is null)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        $"Unable to adjust assort: {assortToAdjust.Id} item: {assortToAdjust.Template} BuyRestrictionCurrent value, assort has a null upd object"
                    );
                }

                continue;
            }

            assortToAdjust.Upd.BuyRestrictionCurrent = (int)(assortPurchasesFromTrader[assortId.Key].PurchaseCount ?? 0);
        }

        traderClone.Assort = assortHelper.StripLockedQuestAssort(
            pmcProfile,
            traderId,
            traderClone.Assort,
            MergedQuestAssorts,
            showLockedAssorts
        );

        // Filter out root assorts that are blacklisted for this profile
        if (fullProfile.SptData.BlacklistedItemTemplates?.Count > 0)
        {
            traderClone.Assort.RemoveItemsFromAssort(fullProfile.SptData.BlacklistedItemTemplates);
        }

        return traderClone.Assort;
    }

    /// <summary>
    ///     Reset every traders root item `BuyRestrictionCurrent` property to 0
    /// </summary>
    /// <param name="assortItems">Items to adjust</param>
    protected void ResetBuyRestrictionCurrentValue(IEnumerable<Item> assortItems)
    {
        // iterate over root items
        foreach (var assort in assortItems.Where(item => item.SlotId == "hideout"))
        {
            // no value to adjust
            if (assort.Upd.BuyRestrictionCurrent is null)
            {
                continue;
            }

            assort.Upd.BuyRestrictionCurrent = 0;
        }
    }

    /// <summary>
    /// Create a dictionary keyed by quest status (started/success) with every assortId to QuestId from every trader
    /// </summary>
    /// <returns>Dictionary</returns>
    protected Dictionary<string, Dictionary<MongoId, MongoId>> HydrateMergedQuestAssorts()
    {
        var result = new Dictionary<string, Dictionary<MongoId, MongoId>>();

        // Loop every trader
        var traders = databaseService.GetTraders();
        foreach (var (_, trader) in traders)
        {
            if (trader?.QuestAssort is null)
            {
                // No assort to quest mappings, ignore
                continue;
            }

            foreach (var (unlockStatus, assortToQuestDict) in trader.QuestAssort)
            {
                if (!assortToQuestDict.Any())
                {
                    // Empty assort dict, ignore
                    continue;
                }

                // Null guard - ensure Started/Success/fail exists
                result.TryAdd(unlockStatus, new Dictionary<MongoId, MongoId>());

                foreach (var (assortId, questId) in assortToQuestDict)
                {
                    result[unlockStatus][assortId] = questId;
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Reset a traders assorts and move nextResupply value to future
    ///     Flag trader as needing a flea offer reset to be picked up by flea update() function
    /// </summary>
    /// <param name="trader">trader details to alter</param>
    public void ResetExpiredTrader(Trader trader)
    {
        trader.Assort.Items = GetPristineTraderAssorts(trader.Base.Id);

        // Update resupply value to next timestamp
        trader.Base.NextResupply = (int)traderHelper.GetNextUpdateTimestamp(trader.Base.Id);

        // Flag a refresh is needed so ragfair update() will pick it up
        trader.Base.RefreshTraderRagfairOffers = true;
    }

    /// <summary>
    ///     Does the supplied trader need its assorts refreshed
    /// </summary>
    /// <param name="traderID">Trader to check</param>
    /// <returns>true they need refreshing</returns>
    public bool TraderAssortsHaveExpired(MongoId traderID)
    {
        var time = timeUtil.GetTimeStamp();
        var trader = databaseService.GetTables().Traders[traderID];

        return trader.Base.NextResupply <= time;
    }

    /// <summary>
    ///     Get an array of pristine trader items prior to any alteration by player (as they were on server start)
    /// </summary>
    /// <param name="traderId">trader id</param>
    /// <returns>array of Items</returns>
    protected List<Item> GetPristineTraderAssorts(MongoId traderId)
    {
        return cloner.Clone(traderHelper.GetTraderAssortsByTraderId(traderId).Items);
    }
}
