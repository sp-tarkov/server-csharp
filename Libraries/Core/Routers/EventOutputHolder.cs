using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Utils;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Routers;

[Injectable]
public class EventOutputHolder
{
    protected ISptLogger<EventOutputHolder> _logger;
    protected ProfileHelper _profileHelper;
    protected TimeUtil _timeUtil;
    protected ICloner _cloner;

    protected Dictionary<string, ItemEventRouterResponse> _outputStore = new();
    protected Dictionary<string, Dictionary<string, bool>> _clientActiveSessionStorage = new();

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

    public ItemEventRouterResponse GetOutput(string sessionId)
    {
        var resultFound = _outputStore.TryGetValue(sessionId, out ItemEventRouterResponse? result);
        if (resultFound)
        {
            return result;
        }

        // Nothing found, reset to default
        ResetOutput(sessionId);
        _outputStore.TryGetValue(sessionId, out result!);

        return result;
    }

    public void ResetOutput(string sessionId)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        if (_outputStore.ContainsKey(sessionId))
        {
            _outputStore.Remove(sessionId);
        }
        
        _outputStore.Add(
            sessionId,
            new ItemEventRouterResponse
            {
                ProfileChanges = new Dictionary<string, ProfileChange>()
                {
                    {
                        sessionId, new ProfileChange
                        {
                            Id = sessionId,
                            Experience = pmcProfile.Info.Experience,
                            Quests = [],
                            RagFairOffers = [],
                            WeaponBuilds = [],
                            EquipmentBuilds = [],
                            Items = new ItemChanges { NewItems = [], ChangedItems = [], DeletedItems = [] },
                            Production = new Dictionary<string, Production>(),
                            Improvements = new Dictionary<string, HideoutImprovement>(),
                            Skills = new Skills { Common = [], Mastering = [], Points = 0 },
                            Health = _cloner.Clone(pmcProfile.Health),
                            TraderRelations = new Dictionary<string, TraderData>(),
                            RecipeUnlocked = { },
                            QuestsStatus = []
                        }
                    }
                },
                Warnings = { }
            }
        );
    }

    public void UpdateOutputProperties(string sessionId)
    {
        PmcData pmcData = _profileHelper.GetPmcProfile(sessionId);
        ProfileChange profileChanges = _outputStore[sessionId].ProfileChanges[sessionId];

        profileChanges.Experience = pmcData.Info.Experience;
        profileChanges.Health = _cloner.Clone(pmcData.Health);
        profileChanges.Skills.Common = _cloner.Clone(pmcData.Skills.Common); // Always send skills for Item event route response
        profileChanges.Skills.Mastering = _cloner.Clone(pmcData.Skills.Mastering);

        // Clone productions to ensure we preseve the profile jsons data
        profileChanges.Production = GetProductionsFromProfileAndFlagComplete(
            _cloner.Clone(pmcData.Hideout.Production),
            sessionId
        );
        profileChanges.Improvements = _cloner.Clone(GetImprovementsFromProfileAndFlagComplete(pmcData));
        profileChanges.TraderRelations = ConstructTraderRelations(pmcData.TradersInfo);

        ResetMoneyTransferLimit(pmcData.MoneyTransferLimitData);
        profileChanges.MoneyTransferLimitData = pmcData.MoneyTransferLimitData;

        // Fixes container craft from water collector not resetting after collection + removed completed normal crafts
        CleanUpCompleteCraftsInProfile(pmcData.Hideout.Production);
    }

    private void CleanUpCompleteCraftsInProfile(Dictionary<string, Production>? productions)
    {
        foreach (var production in productions)
        {
            if ((production.Value.SptIsComplete ?? false) && (production.Value.SptIsContinuous ?? false))
            {
                // Water collector / Bitcoin etc
                production.Value.SptIsComplete = false;
                production.Value.Progress = 0;
                production.Value.StartTimestamp = _timeUtil.GetTimeStamp().ToString();
            }
            else if (!production.Value.InProgress ?? false)
            {
                // Normal completed craft, delete
                productions.Remove(production.Key);
            }
        }
    }

    private Dictionary<string, HideoutImprovement>? GetImprovementsFromProfileAndFlagComplete(PmcData pmcData)
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

    private Dictionary<string, Production>? GetProductionsFromProfileAndFlagComplete(Dictionary<string, Production>? productions, string sessionId)
    {
        foreach (var production in productions)
        {
            if (production.Value is null)
            {
                // Could be cancelled production, skip item to save processing
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
            Dictionary<string, bool>? storageForSessionId = null;
            if (!_clientActiveSessionStorage.TryGetValue(sessionId, out storageForSessionId))
            {
                _clientActiveSessionStorage.Add(sessionId, new Dictionary<string, bool>());
                storageForSessionId = _clientActiveSessionStorage[sessionId];
            }

            // Ensure we don't inform client of production again
            if (storageForSessionId[production.Key])
            {
                productions.Remove(production.Key);

                continue;
            }

            // Flag started craft as having been seen by client so it won't happen subsequent times
            if (production.Value.Progress > 0 && !storageForSessionId[production.Key])
            {
                storageForSessionId[production.Key] = true;
            }
        }

        // Return undefined if there's no crafts to send to client to match live behaviour
        return productions.Keys.Count > 0 ? productions : null;
    }

    private void ResetMoneyTransferLimit(MoneyTransferLimits limit)
    {
        if (limit.NextResetTime < this._timeUtil.GetTimeStamp())
        {
            limit.NextResetTime += limit.ResetInterval;
            limit.RemainingLimit = limit.TotalLimit;
        }
    }

    private Dictionary<string, TraderData> ConstructTraderRelations(Dictionary<string, TraderInfo> traderData)
    {
        return traderData.ToDictionary(
            trader => trader.Key,
            trader => new TraderData()
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
