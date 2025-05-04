using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Routers;

[Injectable]
public class EventOutputHolder
{
    protected Dictionary<string, Dictionary<string, bool>> _clientActiveSessionStorage = new();
    protected ICloner _cloner;
    protected ISptLogger<EventOutputHolder> _logger;

    protected Dictionary<string, ItemEventRouterResponse> _outputStore = new();
    protected ProfileHelper _profileHelper;
    protected TimeUtil _timeUtil;

    public EventOutputHolder(
        ISptLogger<EventOutputHolder> logger,
        ProfileHelper profileHelper,
        TimeUtil timeUtil,
        ICloner cloner
    )
    {
        _logger = logger;
        _profileHelper = profileHelper;
        _timeUtil = timeUtil;
        _cloner = cloner;
    }

    /// <summary>
    /// Get a fresh/empty response to send to the client
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse GetOutput(string sessionId)
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

    public void ResetOutput(string sessionId)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        if (_outputStore.ContainsKey(sessionId))
        {
            // Dict contains existing output object, purge it
            _outputStore.Remove(sessionId);
        }

        // Create fresh output object
        _outputStore.Add(
            sessionId,
            new ItemEventRouterResponse
            {
                ProfileChanges = new Dictionary<string, ProfileChange>
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
                            Production = new Dictionary<string, Production>(),
                            Improvements = new Dictionary<string, HideoutImprovement>(),
                            Skills = new Skills
                            {
                                Common = [],
                                Mastering = [],
                                Points = 0,
                            },
                            Health = _cloner.Clone(pmcProfile.Health),
                            TraderRelations = new Dictionary<string, TraderData>(),
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
    public void UpdateOutputProperties(string sessionId)
    {
        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var profileChanges = _outputStore[sessionId].ProfileChanges[sessionId];

        profileChanges.Experience = pmcData.Info.Experience;
        profileChanges.Health = _cloner.Clone(pmcData.Health);
        profileChanges.Skills.Common = _cloner.Clone(pmcData.Skills.Common); // Always send skills for Item event route response
        profileChanges.Skills.Mastering = _cloner.Clone(pmcData.Skills.Mastering);

        // Clone productions to ensure we preseve the profile jsons data
        profileChanges.Production = GetProductionsFromProfileAndFlagComplete(
            _cloner.Clone(pmcData.Hideout.Production),
            sessionId
        );
        profileChanges.Improvements = _cloner.Clone(
            GetImprovementsFromProfileAndFlagComplete(pmcData)
        );
        profileChanges.TraderRelations = ConstructTraderRelations(pmcData.TradersInfo);

        ResetMoneyTransferLimit(pmcData.MoneyTransferLimitData);
        profileChanges.MoneyTransferLimitData = pmcData.MoneyTransferLimitData;

        // Fixes container craft from water collector not resetting after collection + removed completed normal crafts
        CleanUpCompleteCraftsInProfile(pmcData.Hideout.Production);
    }

    /// <summary>
    ///     Required as continuous productions don't reset and stay at 100% completion but client thinks it hasn't started
    /// </summary>
    /// <param name="productions"> Productions in a profile </param>
    private void CleanUpCompleteCraftsInProfile(Dictionary<string, Production>? productions)
    {
        foreach (var production in productions)
        {
            if (production.Value == null)
            {
                // cultist circle
                // remove production in case client already issued a HideoutDeleteProductionCommand and the item is moved to stash
                productions.Remove(production.Key);
            }
            else if (
                (production.Value.SptIsComplete ?? false)
                && (production.Value.SptIsContinuous ?? false)
            )
            {
                // Water collector / Bitcoin etc
                production.Value.SptIsComplete = false;
                production.Value.Progress = 0;
                production.Value.StartTimestamp = _timeUtil.GetTimeStamp();
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
    private Dictionary<string, HideoutImprovement>? GetImprovementsFromProfileAndFlagComplete(
        PmcData pmcData
    )
    {
        foreach (var improvementKey in pmcData.Hideout.Improvements)
        {
            var improvement = pmcData.Hideout.Improvements[improvementKey.Key];

            // Skip completed
            if (improvement.Completed ?? false)
            {
                continue;
            }

            if (improvement.ImproveCompleteTimestamp < _timeUtil.GetTimeStamp())
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
    private Dictionary<string, Production>? GetProductionsFromProfileAndFlagComplete(
        Dictionary<string, Production>? productions,
        string sessionId
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
            if (
                (production.Value.SptIsComplete ?? false)
                && (production.Value.SptIsContinuous ?? false)
            )
            {
                continue;
            }

            // Skip completed
            if (!production.Value.InProgress ?? false)
            {
                continue;
            }

            // Client informed of craft, remove from data returned
            Dictionary<string, bool>? storageForSessionId = null;
            if (!_clientActiveSessionStorage.TryGetValue(sessionId, out storageForSessionId))
            {
                _clientActiveSessionStorage.Add(sessionId, new Dictionary<string, bool>());
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

    private void ResetMoneyTransferLimit(MoneyTransferLimits limit)
    {
        if (limit.NextResetTime < _timeUtil.GetTimeStamp())
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
    private Dictionary<string, TraderData> ConstructTraderRelations(
        Dictionary<string, TraderInfo> traderData
    )
    {
        return traderData.ToDictionary(
            trader =>
            {
                return trader.Key;
            },
            trader =>
            {
                return new TraderData
                {
                    SalesSum = trader.Value.SalesSum,
                    Disabled = trader.Value.Disabled,
                    Loyalty = trader.Value.LoyaltyLevel,
                    Standing = trader.Value.Standing,
                    Unlocked = trader.Value.Unlocked,
                };
            }
        );
    }
}
