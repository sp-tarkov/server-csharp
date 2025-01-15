using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Utils;
using Core.Utils.Cloners;
using Core.Utils.Json;

namespace Core.Routers;

[Injectable]
public class EventOutputHolder
{
    protected ProfileHelper _profileHelper;
    protected TimeUtil _timeUtil;
    protected ICloner _cloner;

    protected Dictionary<string, ItemEventRouterResponse> _outputStore = new();

    public EventOutputHolder(
        ProfileHelper profileHelper,
        TimeUtil timeUtil,
        ICloner cloner
    )
    {
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

        _outputStore.Add(sessionId, new ItemEventRouterResponse
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
                        Items = new ItemChanges(){ NewItems = [], ChangedItems = [], DeletedItems = []},
                        Production = new Dictionary<string, Productive>(),
                        Improvements = new Dictionary<string, HideoutImprovement>(),
                        Skills = new Skills{ Common = [], Mastering = [], Points = 0},
                        Health = _cloner.Clone(pmcProfile.Health),
                        TraderRelations = new Dictionary<string, TraderData>(),
                        RecipeUnlocked = {},
                        QuestsStatus = []
                    }
                }
            },
            Warnings = {}
        });
    }

    public void UpdateOutputProperties()
    {
        throw new NotImplementedException();
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
        return traderData.ToDictionary(trader => trader.Key, trader => new TraderData()
        {
            SalesSum = trader.Value.SalesSum,
            Disabled = trader.Value.Disabled,
            Loyalty = trader.Value.LoyaltyLevel,
            Standing = trader.Value.Standing,
            Unlocked = trader.Value.Unlocked,
        });
    }
}
