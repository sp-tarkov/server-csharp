using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class EventOutputHolder(ISptLogger<EventOutputHolder> logger, ProfileHelper profileHelper, TimeUtil timeUtil, ICloner cloner)
{
    protected readonly Dictionary<MongoId, Dictionary<string, bool>> _clientActiveSessionStorage = new();
    protected readonly Dictionary<MongoId, ItemEventRouterResponse> _outputStore = new();

    /// <summary>
    /// Get a fresh/empty response to send to the client
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse GetOutput(MongoId sessionId)
    {
        if (_outputStore.TryGetValue(sessionId, out var result))
        {
            return result;
        }

        // Nothing found, Create new empty output response
        ResetOutput(sessionId);
        _outputStore.TryGetValue(sessionId, out result!);

        return result;
    }

    public void ResetOutput(MongoId sessionId)
    {
        var pmcProfile = profileHelper.GetPmcProfile(sessionId);

        _outputStore.Remove(sessionId);

        // Create fresh output object
        _outputStore.Add(
            sessionId,
            new ItemEventRouterResponse
            {
                ProfileChanges = new Dictionary<MongoId, ProfileChange>
                {
                    {
                        sessionId,
                        new ProfileChange
                        {
                            Id = sessionId,
                            Experience = pmcProfile.Info.Experience,
                            Quests = [],
                            RagFairOffers = [],
                            WeaponBuilds = [],
                            EquipmentBuilds = [],
                            Items = new ItemChanges
                            {
                                NewItems = [],
                                ChangedItems = [],
                                DeletedItems = [],
                            },
                            Production = [],
                            Improvements = [],
                            Skills = new Skills
                            {
                                Common = [],
                                Mastering = [],
                                Points = 0,
                            },
                            Health = cloner.Clone(pmcProfile.Health),
                            TraderRelations = [],
                            QuestsStatus = [],
                        }
                    },
                },
                Warnings = [],
            }
        );
    }

    /// <summary>
    ///     Update output object with most recent values from player profile
    /// </summary>
    /// <param name="sessionId"> Session id </param>
    public void UpdateOutputProperties(MongoId sessionId)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);
        var profileChanges = _outputStore[sessionId].ProfileChanges[sessionId];

        profileChanges.Experience = pmcData.Info.Experience;
        profileChanges.Health = cloner.Clone(pmcData.Health);
        profileChanges.Skills.Common = cloner.Clone(pmcData.Skills.Common); // Always send skills for Item event route response
        profileChanges.Skills.Mastering = cloner.Clone(pmcData.Skills.Mastering);

        // Clone productions to ensure we preserve the profile jsons data
        var hideoutProductionNull = pmcData.Hideout.Production == null;
        if (!hideoutProductionNull)
        {
            profileChanges.Production = GetProductionsFromProfileAndFlagComplete(cloner.Clone(pmcData.Hideout.Production), sessionId);
        }

        if (pmcData.Hideout.Improvements != null)
        {
            profileChanges.Improvements = cloner.Clone(GetImprovementsFromProfileAndFlagComplete(pmcData));
        }

        profileChanges.TraderRelations = ConstructTraderRelations(pmcData.TradersInfo);

        ResetMoneyTransferLimit(pmcData.MoneyTransferLimitData);
        profileChanges.MoneyTransferLimitData = pmcData.MoneyTransferLimitData;

        // Fixes container craft from water collector not resetting after collection + removed completed normal crafts
        if (!hideoutProductionNull && pmcData.Hideout.Production.Any())
        {
            CleanUpCompleteCraftsInProfile(pmcData.Hideout.Production);
        }
    }

    /// <summary>
    ///     Required as continuous productions don't reset and stay at 100% completion but client thinks it hasn't started
    /// </summary>
    /// <param name="productions"> Productions in a profile </param>
    protected void CleanUpCompleteCraftsInProfile(Dictionary<MongoId, Production>? productions)
    {
        foreach (var production in productions)
        {
            if (production.Value == null)
            {
                // cultist circle
                // remove production in case client already issued a HideoutDeleteProductionCommand and the item is moved to stash
                productions.Remove(production.Key);
            }
            else if ((production.Value.SptIsComplete ?? false) && (production.Value.SptIsContinuous ?? false))
            {
                // Water collector / Bitcoin etc
                production.Value.SptIsComplete = false;
                production.Value.Progress = 0;
                production.Value.StartTimestamp = timeUtil.GetTimeStamp();
            }
            else if (!production.Value.InProgress ?? false)
            {
                // Normal completed craft, delete
                productions.Remove(production.Key);
            }
        }
    }

    /// <summary>
    ///     Return all hideout Improvements from player profile, adjust completed Improvements' completed property to be true
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <returns> Dictionary of hideout improvements </returns>
    protected Dictionary<MongoId, HideoutImprovement> GetImprovementsFromProfileAndFlagComplete(PmcData pmcData)
    {
        foreach (var (key, improvement) in pmcData.Hideout.Improvements)
        {
            // Skip completed
            if (improvement.Completed ?? false)
            {
                continue;
            }

            if (improvement.ImproveCompleteTimestamp < timeUtil.GetTimeStamp())
            {
                improvement.Completed = true;
            }
        }

        return pmcData.Hideout.Improvements;
    }

    /// <summary>
    ///     Return productions from player profile except those completed crafts the client has already seen
    /// </summary>
    /// <param name="productions"> Productions from player profile </param>
    /// <param name="sessionId"> Player session ID</param>
    /// <returns> Dictionary of hideout productions </returns>
    protected Dictionary<MongoId, Production> GetProductionsFromProfileAndFlagComplete(
        Dictionary<MongoId, Production>? productions,
        MongoId sessionId
    )
    {
        foreach (var production in productions)
        {
            if (production.Value is null)
            // Could be cancelled production, skip item to save processing
            {
                continue;
            }

            // Complete and is Continuous e.g. water collector
            if ((production.Value.SptIsComplete ?? false) && (production.Value.SptIsContinuous ?? false))
            {
                continue;
            }

            // Skip completed
            if (!production.Value.InProgress ?? false)
            {
                continue;
            }

            // Client informed of craft, remove from data returned
            if (!_clientActiveSessionStorage.TryGetValue(sessionId, out var storageForSessionId))
            {
                _clientActiveSessionStorage.Add(sessionId, []);
                storageForSessionId = _clientActiveSessionStorage[sessionId];
            }

            // Ensure we don't inform client of production again
            if (storageForSessionId.ContainsKey(production.Key))
            {
                productions.Remove(production.Key);

                continue;
            }

            // Flag started craft as having been seen by client so it won't happen subsequent times
            if (production.Value.Progress > 0 && !storageForSessionId.ContainsKey(production.Key))
            {
                storageForSessionId.TryAdd(production.Key, true);
            }
        }

        // Return undefined if there's no crafts to send to client to match live behaviour
        return productions.Keys.Count > 0 ? productions : null;
    }

    protected void ResetMoneyTransferLimit(MoneyTransferLimits limit)
    {
        if (limit.NextResetTime < timeUtil.GetTimeStamp())
        {
            limit.NextResetTime += limit.ResetInterval;
            limit.RemainingLimit = limit.TotalLimit;
        }
    }

    /// <summary>
    ///     Convert the internal trader data object into an object we can send to the client
    /// </summary>
    /// <param name="traderData"> Server data for traders </param>
    /// <returns> Dict of trader id + TraderData </returns>
    protected Dictionary<MongoId, TraderData> ConstructTraderRelations(Dictionary<MongoId, TraderInfo> traderData)
    {
        return traderData.ToDictionary(
            trader => trader.Key,
            trader => new TraderData
            {
                SalesSum = trader.Value.SalesSum,
                Disabled = trader.Value.Disabled,
                Loyalty = trader.Value.LoyaltyLevel,
                Standing = trader.Value.Standing,
                Unlocked = trader.Value.Unlocked,
            }
        );
    }
}
