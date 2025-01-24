using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.Prestige;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Controllers;

[Injectable]
public class PrestigeController(
    ISptLogger<PrestigeController> _logger,
    TimeUtil _timeUtil,
    InventoryHelper _inventoryHelper,
    ProfileHelper _profileHelper,
    EventOutputHolder _eventOutputHolder,
    CreateProfileService _createProfileService,
    DatabaseService _databaseService,
    SaveServer _saveServer,
    ICloner _cloner
)
{
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
        var createRequest = new ProfileCreateRequestData
        {
            Side = prePrestigePmc.Info.Side,
            Nickname = prePrestigePmc.Info.Nickname,
            HeadId = prePrestigePmc.Customization.Head,
            VoiceId = _databaseService.GetTemplates()
                .Customization.FirstOrDefault(
                    (customisation) => customisation.Value.Name == prePrestigePmc.Info.Voice
                )
                .Value.Id,
            SptForcePrestigeLevel = prePrestigeProfileClone.CharacterData.PmcData.Info.PrestigeLevel.GetValueOrDefault(0) + 1, // Current + 1
        };

        // Reset profile
        _createProfileService.CreateProfile(sessionId, createRequest);

        // Get freshly reset profile ready for editing
        var newProfile = _profileHelper.GetFullProfile(sessionId);

        // Skill copy
        // TODO - Find what skills should be prestiged over
        var commonSkillsToCopy = prePrestigePmc.Skills.Common;
        foreach (var skillToCopy in commonSkillsToCopy)
        {
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
        foreach (var skillToCopy in masteringSkillsToCopy)
        {
            // Set progress to max level 20
            skillToCopy.Progress = Math.Min(skillToCopy.Progress.Value, 2000);
            var existingSkill = newProfile.CharacterData.PmcData.Skills.Mastering.FirstOrDefault(
                (skill) => skill.Id == skillToCopy.Id
            );
            if (existingSkill is not null)
            {
                existingSkill.Progress = skillToCopy.Progress;
            }
            else
            {
                newProfile.CharacterData.PmcData.Skills.Mastering.Add(skillToCopy);
            }
        }

        // Assumes Prestige data is in descending order
        var indexOfPrestigeObtained = (int)Math.Min(createRequest.SptForcePrestigeLevel.Value - 1, 1); // Index starts at 0
        var currentPrestigeData = _databaseService.GetTemplates().Prestige.Elements[indexOfPrestigeObtained];
        var prestigeRewards = _databaseService.GetTemplates()
            .Prestige.Elements.Slice(0, indexOfPrestigeObtained + 1)
            .SelectMany((prestige) => prestige.Rewards);


        AddPrestigeRewardsToProfile(sessionId, newProfile, prestigeRewards);

        // Flag profile as having achieved this prestige level
        newProfile.CharacterData.PmcData.Prestige[currentPrestigeData.Id] = _timeUtil.GetTimeStamp();

        // Copy transferred items
        foreach (var transferRequest in request)
        {
            var item = prePrestigePmc.Inventory.Items.FirstOrDefault((item) => item.Id == transferRequest.Id);
            var addItemRequest = new AddItemDirectRequest
            {
                ItemWithModsToAdd = [item.ConvertToHideoutItem(item)],
                FoundInRaid = item.Upd?.SpawnedInSession,
                UseSortingTable = false,
                Callback = null,
            };
            _inventoryHelper.AddItemToStash(
                sessionId,
                addItemRequest,
                newProfile.CharacterData.PmcData,
                _eventOutputHolder.GetOutput(sessionId)
            );
        }

        // Add "Prestigious" achievement
        if (!newProfile.PlayerAchievements.ContainsKey("676091c0f457869a94017a23"))
        {
            newProfile.PlayerAchievements.Add("676091c0f457869a94017a23", _timeUtil.GetTimeStamp());
        }

        // Force save of above changes to disk
        _saveServer.SaveProfile(sessionId);
    }

    private void AddPrestigeRewardsToProfile(string sessionId, SptProfile newProfile, IEnumerable<Reward> rewards)
    {
        _logger.Error("NOT IMPLEMENTED AddPrestigeRewardsToProfile()");
    }
}
