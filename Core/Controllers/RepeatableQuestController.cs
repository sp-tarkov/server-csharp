using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Spt.Config;
using Core.Models.Spt.Repeatable;
using Core.Generators;
using Core.Helpers;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Controllers;

[Injectable]
public class RepeatableQuestController
{
    protected ISptLogger<RepeatableQuestChangeRequest> _logger;
    protected TimeUtil _timeUtil;
    protected HashUtil _hashUtil;
    protected RandomUtil _randomUtil;
    protected HttpResponseUtil _responseUtil;
    protected ProfileHelper _profileHelper;
    protected ProfileFixerService _profileFixerService;
    protected LocalisationService _localisationService;
    protected EventOutputHolder _eventOutputHolder;
    protected PaymentService _paymentService;
    protected RepeatableQuestGenerator _repeatableQuestGenerator;
    protected RepeatableQuestHelper _repeatableQuestHelper;
    protected QuestHelper _questHelper;
    protected DatabaseService _databaseService;
    protected ConfigServer _configServer;
    protected ICloner _cloner;
    protected QuestConfig _questConfig;

    public RepeatableQuestController(
        ISptLogger<RepeatableQuestChangeRequest> logger,
        TimeUtil timeUtil,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        HttpResponseUtil responseUtil,
        ProfileHelper profileHelper,
        ProfileFixerService profileFixerService,
        LocalisationService localisationService,
        EventOutputHolder eventOutputHolder,
        PaymentService paymentService,
        RepeatableQuestGenerator repeatableQuestGenerator,
        RepeatableQuestHelper repeatableQuestHelper,
        QuestHelper questHelper,
        DatabaseService databaseService,
        ConfigServer configServer,
        ICloner cloner)
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _hashUtil = hashUtil;
        _randomUtil = randomUtil;
        _responseUtil = responseUtil;
        _profileHelper = profileHelper;
        _profileFixerService = profileFixerService;
        _localisationService = localisationService;
        _eventOutputHolder = eventOutputHolder;
        _paymentService = paymentService;
        _repeatableQuestGenerator = repeatableQuestGenerator;
        _repeatableQuestHelper = repeatableQuestHelper;
        _questHelper = questHelper;
        _databaseService = databaseService;
        _configServer = configServer;
        _cloner = cloner;

        _questConfig = _configServer.GetConfig<QuestConfig>();
    }

    public ItemEventRouterResponse ChangeRepeatableQuest(PmcData pmcData, RepeatableQuestChangeRequest info, string sessionId)
    {
        throw new NotImplementedException();
    }

    public List<PmcDataRepeatableQuest> GetClientRepeatableQuests(string sessionID)
    {
        var returnData = new List<PmcDataRepeatableQuest>();
        var fullProfile = _profileHelper.GetFullProfile(sessionID);
        var pmcData = fullProfile.CharacterData.PmcData;
        var currentTime = _timeUtil.GetTimeStamp();

        // Daily / weekly / Daily_Savage
        foreach (var repeatableConfig in _questConfig.RepeatableQuests) {
            // Get daily/weekly data from profile, add empty object if missing
            var generatedRepeatables = GetRepeatableQuestSubTypeFromProfile(repeatableConfig, pmcData);
            var repeatableTypeLower = repeatableConfig.Name.ToLower();

            var canAccessRepeatables = CanProfileAccessRepeatableQuests(repeatableConfig, pmcData);
            if (!canAccessRepeatables)
            {
                // Don't send any repeatables, even existing ones
                continue;
            }

            // Existing repeatables are still valid, add to return data and move to next sub-type
            if (currentTime < generatedRepeatables.EndTime - 1)
            {
                returnData.Add(generatedRepeatables);

                _logger.Debug($"[Quest Check] { repeatableTypeLower} quests are still valid.");

                continue;
            }

            // Current time is past expiry time

            // Set endtime to be now + new duration
            generatedRepeatables.EndTime = currentTime + repeatableConfig.ResetTime;
            generatedRepeatables.InactiveQuests = [];
            _logger.Debug($"Generating new { repeatableTypeLower}");

            // Put old quests to inactive (this is required since only then the client makes them fail due to non-completion)
            // Also need to push them to the "inactiveQuests" list since we need to remove them from offraidData.profile.Quests
            // after a raid (the client seems to keep quests internally and we want to get rid of old repeatable quests)
            // and remove them from the PMC's Quests and RepeatableQuests[i].activeQuests
            ProcessExpiredQuests(generatedRepeatables, pmcData);

            // Create dynamic quest pool to avoid generating duplicates
            var questTypePool = GenerateQuestPool(repeatableConfig, pmcData.Info.Level);

            // Add repeatable quests of this loops sub-type (daily/weekly)
            for (var i = 0; i < GetQuestCount(repeatableConfig, pmcData); i++)
            {
                RepeatableQuest quest = new RepeatableQuest();
                var lifeline = 0;
                while (quest.Id is null && questTypePool.Types.Count > 0)
                {
                    quest = _repeatableQuestGenerator.GenerateRepeatableQuest(
                    sessionID,
                        pmcData.Info.Level,
                        pmcData.TradersInfo,
                        questTypePool,
                        repeatableConfig);
                    lifeline++;
                    if (lifeline > 10)
                    {
                        _logger.Debug("We were stuck in repeatable quest generation. This should never happen. Please report");

                        break;
                    }
                }

                // check if there are no more quest types available
                if (questTypePool.Types.Count == 0)
                {
                    break;
                }
                quest.Side = repeatableConfig.Side;
                generatedRepeatables.ActiveQuests.Add(quest);
            }

            // Nullguard
            fullProfile.SptData.FreeRepeatableRefreshUsedCount ??= new Dictionary<string, int>();


            // Reset players free quest count for this repeatable sub-type as we're generating new repeatables for this group (daily/weekly)
            fullProfile.SptData.FreeRepeatableRefreshUsedCount[repeatableTypeLower] = 0;

            // Create stupid redundant change requirements from quest data
            foreach (var quest in generatedRepeatables.ActiveQuests) {
                generatedRepeatables.ChangeRequirement[quest.Id] = new ChangeRequirement{
                    ChangeCost = quest.ChangeCost,
                    ChangeStandingCost = _randomUtil.GetArrayValue([0, 0.01]), // Randomise standing cost to replace
                };
            }

            // Reset free repeatable values in player profile to defaults
            generatedRepeatables.FreeChanges = repeatableConfig.FreeChanges;
            generatedRepeatables.FreeChangesAvailable = repeatableConfig.FreeChanges;

            returnData.Add( new PmcDataRepeatableQuest{
                Id = repeatableConfig.Id,
                Name = generatedRepeatables.Name,
                EndTime = generatedRepeatables.EndTime,
                ActiveQuests = generatedRepeatables.ActiveQuests,
                InactiveQuests = generatedRepeatables.InactiveQuests,
                ChangeRequirement = generatedRepeatables.ChangeRequirement,
                FreeChanges = generatedRepeatables.FreeChanges,
                FreeChangesAvailable = generatedRepeatables.FreeChanges,
            });
        }

        return returnData;
    }

    private PmcDataRepeatableQuest GetRepeatableQuestSubTypeFromProfile(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        // Get from profile, add if missing
        var repeatableQuestDetails = pmcData.RepeatableQuests.FirstOrDefault(
            (repeatable) => repeatable.Name == repeatableConfig.Name);
        if (repeatableQuestDetails is not null)
        {
            // Not in profile, generate
            var hasAccess = _profileHelper.HasAccessToRepeatableFreeRefreshSystem(pmcData);
            repeatableQuestDetails = new PmcDataRepeatableQuest(){
                Id = repeatableConfig.Id,
                Name= repeatableConfig.Name,
                ActiveQuests= [],
                InactiveQuests= [],
                EndTime= 0,
                ChangeRequirement= { },
                FreeChanges= hasAccess? repeatableConfig.FreeChanges: 0,
                FreeChangesAvailable= hasAccess? repeatableConfig.FreeChangesAvailable: 0,
            };

            // Add base object that holds repeatable data to profile
            pmcData.RepeatableQuests.Add(repeatableQuestDetails);
        }

        return repeatableQuestDetails;
    }

    private bool CanProfileAccessRepeatableQuests(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        // PMC and daily quests not unlocked yet
        if (repeatableConfig.Side == "Pmc" && !PlayerHasDailyPmcQuestsUnlocked(pmcData, repeatableConfig))
        {
            return false;
        }

        // Scav and daily quests not unlocked yet
        if (repeatableConfig.Side == "Scav" && !PlayerHasDailyScavQuestsUnlocked(pmcData))
        {
            _logger.Debug("Daily scav quests still locked, Intel center not built");

            return false;
        }

        return true;
    }

    /**
     * Does player have daily pmc quests unlocked
     * @param pmcData Player profile to check
     * @param repeatableConfig Config of daily type to check
     * @returns True if unlocked
     */
    private bool PlayerHasDailyPmcQuestsUnlocked(PmcData pmcData, RepeatableQuestConfig repeatableConfig)
    {
        return pmcData.Info.Level >= repeatableConfig.MinPlayerLevel;
    }

    /**
     * Does player have daily scav quests unlocked
     * @param pmcData Player profile to check
     * @returns True if unlocked
     */
    private bool PlayerHasDailyScavQuestsUnlocked(PmcData pmcData)
    {
        return (
            pmcData?.Hideout?.Areas?.FirstOrDefault((hideoutArea) => hideoutArea.Type == HideoutAreas.INTEL_CENTER)?.Level >= 1
        );
    }

    private void ProcessExpiredQuests(PmcDataRepeatableQuest generatedRepeatables, PmcData pmcData)
    {
        var questsToKeep = new List<RepeatableQuest>();
        foreach (var activeQuest in generatedRepeatables.ActiveQuests) {
            var questStatusInProfile = pmcData.Quests.FirstOrDefault((quest) => quest.QId == activeQuest.Id);
            if (questStatusInProfile is null)
            {
                continue;
            }

            // Keep finished quests in list so player can hand in
            if (questStatusInProfile.Status == QuestStatusEnum.AvailableForFinish)
            {
                questsToKeep.Add(activeQuest);
                _logger.Debug($"Keeping repeatable quest: ${ activeQuest.Id} in activeQuests since it is available to hand in");

                continue;
            }

            // Clean up quest-related counters being left in profile
            _profileFixerService.RemoveDanglingConditionCounters(pmcData);

            // Remove expired quest from pmc.quest array
            pmcData.Quests = pmcData.Quests.Where((quest) => quest.QId != activeQuest.Id).ToList();

            // Store in inactive array
            generatedRepeatables.InactiveQuests.Add(activeQuest);
        }

        generatedRepeatables.ActiveQuests = questsToKeep;
    }

    private QuestTypePool GenerateQuestPool(RepeatableQuestConfig repeatableConfig, int? pmcLevel)
    {
        var questPool = CreateBaseQuestPool(repeatableConfig);

        // Get the allowed locations based on the PMC's level
        var locations = GetAllowedLocationsForPmcLevel(repeatableConfig.Locations, pmcLevel.Value);

        // Populate Exploration and Pickup quest locations
        foreach (var location in locations) {
            if (location.Key != ELocationName.any)
            {
                questPool.Pool.Exploration.Locations[location.Key] = location.Value;
                questPool.Pool.Pickup.Locations[location.Key] = location.Value;
            }
        }

        // Add "any" to pickup quest pool
        questPool.Pool.Pickup.Locations[ELocationName.any] = ["any"];

        var eliminationConfig = _repeatableQuestHelper.GetEliminationConfigByPmcLevel(pmcLevel.Value, repeatableConfig);
        var targetsConfig = _repeatableQuestHelper.ProbabilityObjectArray<string, BossInfo>(eliminationConfig.Targets);

        // Populate Elimination quest targets and their locations
        foreach (var target in targetsConfig) {
            // Target is boss
            //if (target.isBoss)
            //{
            //    questPool.Pool.Elimination.Targets[targetKey] = new { locations: ["any"] };
            //}
            //else
            //{
            //    // Non-boss targets
            //    var possibleLocations = locations;

            //    var allowedLocations =
            //    targetKey == "Savage"
            //        ? possibleLocations.filter((location) => location != "laboratory") // Exclude labs for Savage targets.
            //        : possibleLocations;

            //    questPool.Pool.Elimination.Targets[targetKey] = new { Locations = allowedLocations };
            //}
            _logger.Error("NOT IMPLEMENTED - GenerateQuestPool");
        }

        return questPool;
    }

    private QuestTypePool CreateBaseQuestPool(RepeatableQuestConfig repeatableConfig)
    {
        return new QuestTypePool
        {
            Types = _cloner.Clone(repeatableConfig.Types),
            Pool = new QuestPool
            {
                Exploration = new ExplorationPool
                {
                    Locations = new Dictionary<ELocationName, List<string>>()
                },
                Elimination = new EliminationPool
                {
                    Targets = new EliminationTargetPool()
                },
                Pickup = new ExplorationPool
                {
                    Locations = new Dictionary<ELocationName, List<string>>()
                }
            },
        };
    }

    private Dictionary<ELocationName, List<string>> GetAllowedLocationsForPmcLevel(Dictionary<ELocationName, List<string>> locations, int pmcLevel)
    {
        var allowedLocation = new Dictionary<ELocationName, List<string>>();

        foreach (var locationKvP in locations) {
            var locationNames = new List<string>();
            foreach (var locationName in locationKvP.Value) {
                if (IsPmcLevelAllowedOnLocation(locationName, pmcLevel))
                {
                    locationNames.Add(locationName);
                }
            }

            if (locationNames.Count > 0)
            {
                allowedLocation[locationKvP.Key] = locationNames;
            }
        }

        return allowedLocation;
    }

        /**
     * Return true if the given pmcLevel is allowed on the given location
     * @param location The location name to check
     * @param pmcLevel The level of the pmc
     * @returns True if the given pmc level is allowed to access the given location
     */
    protected bool IsPmcLevelAllowedOnLocation(string location, int pmcLevel) {
        // All PMC levels are allowed for 'any' location requirement
        if (location == ELocationName.any.ToString()) {
            return true;
        }

        var locationBase = _databaseService.GetLocation(location.ToLower())?.Base;
        if (locationBase is not null) {
            return true;
        }

        return pmcLevel <= locationBase.RequiredPlayerLevelMax && pmcLevel >= locationBase.RequiredPlayerLevelMin;
    }

    private int GetQuestCount(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        if (repeatableConfig.Name.ToLower() == "daily"
            && _profileHelper.HasEliteSkillLevel(SkillTypes.Charisma, pmcData)
        )
        {
            // Elite charisma skill gives extra daily quest(s)
            return (repeatableConfig.NumQuests +
                    _databaseService.GetGlobals().Configuration.SkillsSettings.Charisma.BonusSettings.EliteBonusSettings
                        .RepeatableQuestExtraCount.GetValueOrDefault(0)
            );
        }

        return repeatableConfig.NumQuests;
    }
}
