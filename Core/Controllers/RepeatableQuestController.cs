using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Quests;
using Core.Models.Spt.Config;
using Core.Models.Spt.Repeatable;
using Core.Generators;
using Core.Helpers;
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
    protected ConfigServer _configServer;
    protected ICloner _cloner;
    private readonly QuestConfig _questConfig;

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
        throw new NotImplementedException();
    }

    private bool CanProfileAccessRepeatableQuests(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    private void ProcessExpiredQuests(PmcDataRepeatableQuest generatedRepeatables, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    private QuestTypePool GenerateQuestPool(RepeatableQuestConfig repeatableConfig, double? infoLevel)
    {
        throw new NotImplementedException();
    }

    private int GetQuestCount(RepeatableQuestConfig repeatableConfig, PmcData pmcData)
    {
        throw new NotImplementedException();
    }
}
