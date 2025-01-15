using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.Prestige;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Routers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Controllers;

[Injectable]
public class PrestigeController
{
    protected ISptLogger<PrestigeController> _logger;
    protected TimeUtil _timeUtil;
    protected InventoryHelper _inventoryHelper;
    protected ProfileHelper _profileHelper;
    protected EventOutputHolder _eventOutputHolder;
    protected CreateProfileService _createProfileService;
    private DatabaseService _databaseService;
    protected ICloner _cloner;

    public PrestigeController
    (
        ISptLogger<PrestigeController> logger,
        TimeUtil timeUtil,
        InventoryHelper inventoryHelper,
        ProfileHelper profileHelper,
        EventOutputHolder eventOutputHolder,
        CreateProfileService createProfileService,
        DatabaseService databaseService,
        ICloner cloner
    )
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _inventoryHelper = inventoryHelper;
        _profileHelper = profileHelper;
        _eventOutputHolder = eventOutputHolder;
        _createProfileService = createProfileService;
        _databaseService = databaseService;
        _cloner = cloner;
    }

    /// <summary>
    /// Handle /client/prestige/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public Prestige GetPrestige(
        string sessionId,
        EmptyRequestData info)
    {
        return _databaseService.GetTemplates().Prestige;
    }

    /// <summary>
    /// Handle /client/prestige/obtain
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public void ObtainPrestige(
        string sessionId,
        List<ObtainPrestigeRequest> request)
    {
        var prePrestigeProfileClone = _cloner.Clone(_profileHelper.GetFullProfile(sessionId));
        var prePrestigePmc = prePrestigeProfileClone.CharacterData.PmcData;
        var createRequest = new ProfileCreateRequestData {
            Side = prePrestigePmc.Info.Side,
            Nickname = prePrestigePmc.Info.Nickname,
            HeadId = prePrestigePmc.Customization.Head,
            VoiceId =  _databaseService.GetTemplates().Customization.FirstOrDefault(
                (customisation) => customisation.Value.Name == prePrestigePmc.Info.Voice).Value.Id,
            SptForcePrestigeLevel = prePrestigeProfileClone.CharacterData.PmcData.Info.PrestigeLevel.GetValueOrDefault(0) + 1, // Current + 1
        };

        // Reset profile
        _createProfileService.CreateProfile(sessionId, createRequest);

        // Get freshly reset profile ready for editing
        var newProfile = _profileHelper.GetFullProfile(sessionId);

        // Skill copy
        var commonSkillsToCopy = prePrestigePmc.Skills.Common;
        foreach (var skillToCopy in commonSkillsToCopy) {
            // Set progress to max level 20
            skillToCopy.Progress = Math.Min(skillToCopy.Progress.Value, 2000);
            var existingSkill = newProfile.CharacterData.PmcData.Skills.Common.FirstOrDefault((skill) => skill.Id == skillToCopy.Id);
            if (existingSkill is not null)
            {
                existingSkill.Progress = skillToCopy.Progress;
            }
            else
            {
                newProfile.CharacterData.PmcData.Skills.Common.Add(skillToCopy);
            }
        }

        var masteringSkillsToCopy = prePrestigePmc.Skills.Mastering;
        foreach (var skillToCopy in masteringSkillsToCopy) {
            // Set progress to max level 20
            skillToCopy.Progress = Math.Min(skillToCopy.Progress.Value, 2000);
            var existingSkill = newProfile.CharacterData.PmcData.Skills.Mastering.FirstOrDefault(
                (skill) => skill.Id == skillToCopy.Id);
            if (existingSkill is not null)
            {
                existingSkill.Progress = skillToCopy.Progress;
            }
            else
            {
                newProfile.CharacterData.PmcData.Skills.Mastering.Add(skillToCopy);
            }
        }

        var indexToGet = Math.Min(createRequest.SptForcePrestigeLevel.Value - 1, 1); // Index starts at 0
        var rewards = _databaseService.GetTemplates().Prestige.Elements[indexToGet].Rewards;
        AddPrestigeRewardsToProfile(sessionId, newProfile, rewards);

        // Copy transferred items
        foreach (var transferRequest in request) {
            var item = prePrestigePmc.Inventory.Items.FirstOrDefault((item) => item.Id == transferRequest.Id);
            var addItemRequest = new AddItemDirectRequest {
                ItemWithModsToAdd = [item],
                FoundInRaid = item.Upd?.SpawnedInSession,
                UseSortingTable = false,
                Callback = null,
            };
            _inventoryHelper.AddItemToStash(
                sessionId,
                addItemRequest,
                newProfile.CharacterData.PmcData,
                _eventOutputHolder.GetOutput(sessionId));
        }

        // Add "Prestigious" achievement
        if (!newProfile.PlayerAchievements.ContainsKey("676091c0f457869a94017a23"))
        {
            newProfile.PlayerAchievements.Add("676091c0f457869a94017a23", _timeUtil.GetTimeStamp());
        }
    }

    private void AddPrestigeRewardsToProfile(string sessionId, SptProfile newProfile, IEnumerable<QuestReward> rewards)
    {
        _logger.Error("NOT IMPLEMENTED AddPrestigeRewardsToProfile()");
    }

}
