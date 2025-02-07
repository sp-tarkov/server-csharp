using System.Text.Json;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.Prestige;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using SptCommon.Extensions;

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
    protected double _prestigePercentage = 0.05;

    /// <summary>
    ///     Handle /client/prestige/list
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
    ///     <para>Handle /client/prestige/obtain</para>
    ///     Going to Prestige 1 grants the below
    ///     <list type="bullet">
    ///         <item>5% of skills should be transfered over</item>
    ///         <item>5% of mastering should be transfered over</item>
    ///         <item>Earned achievements should be transfered over</item>
    ///         <item>Profile stats should be transfered over</item>
    ///         <item>Prestige progress should be transfered over</item>
    ///         <item>Items and rewards for Prestige 1</item>
    ///     </list>
    ///     Going to Prestige 2 grants the below
    ///     <list type="bullet">
    ///         <item>10% of skills should be transfered over</item>
    ///         <item>10% of mastering should be transfered over</item>
    ///         <item>Earned achievements should be transfered over</item>
    ///         <item>Profile stats should be transfered over</item>
    ///         <item>Prestige progress should be transfered over</item>
    ///         <item>Items and rewards for Prestige 2</item>
    ///     </list>
    ///     Each time reseting the below
    ///     <list type="bullet">
    ///         <item>Trader standing</item>
    ///         <item>Task progress</item>
    ///         <item>Character level</item>
    ///         <item>Stash</item>
    ///         <item>Hideout progress</item>
    ///     </list>
    /// </summary>
    /// <returns></returns>
    public void ObtainPrestige(
        string sessionId,
        ObtainPrestigeRequestList request)
    {
        // Clone existing profile, create a new one
        var prePrestigeProfileClone = _cloner.Clone(_profileHelper.GetFullProfile(sessionId));
        var prePrestigePmc = prePrestigeProfileClone.CharacterData.PmcData;
        var createRequest = new ProfileCreateRequestData
        {
            Side = prePrestigePmc.Info.Side,
            Nickname = prePrestigePmc.Info.Nickname,
            HeadId = prePrestigePmc.Customization.Head,
            VoiceId = _databaseService.GetTemplates()
                .Customization.FirstOrDefault(
                    customisation => customisation.Value.Name == prePrestigePmc.Info.Voice
                )
                .Value.Id
        };

        // Reset profile
        _createProfileService.CreateProfile(sessionId, createRequest);

        // Get freshly reset profile ready for editing
        var newProfile = _profileHelper.GetFullProfile(sessionId);

        // set this here so we can use the prestigeLevel for further calcs
        newProfile.CharacterData.PmcData.Info.PrestigeLevel = prePrestigePmc.Info.PrestigeLevel ?? 0;
        newProfile.CharacterData.PmcData.Info.PrestigeLevel++;

        // Copy skills to new profile
        var commonSkillsToCopy = prePrestigePmc.Skills.Common;
        foreach (var skillToCopy in commonSkillsToCopy)
        {
            // Set progress 5% of what it was * prestige level to get 5% or 10% for prestige 1 or 2 respectivly
            skillToCopy.Progress = skillToCopy.Progress.Value * _prestigePercentage * newProfile.CharacterData.PmcData.Info.PrestigeLevel;
            var existingSkill = newProfile.CharacterData.PmcData.Skills.Common.FirstOrDefault(skill => skill.Id == skillToCopy.Id);
            if (existingSkill is not null)
            {
                existingSkill.Progress = skillToCopy.Progress;
            }
            else
            {
                newProfile.CharacterData.PmcData.Skills.Common.Add(skillToCopy);
            }
        }

        // Copy mastering to new profile
        var masteringSkillsToCopy = prePrestigePmc.Skills.Mastering;
        foreach (var skillToCopy in masteringSkillsToCopy)
        {
            // Set progress 5% of what it was * prestige level to get 5% or 10% for prestige 1 or 2 respectivly
            skillToCopy.Progress = skillToCopy.Progress.Value * _prestigePercentage * newProfile.CharacterData.PmcData.Info.PrestigeLevel;
            var existingSkill = newProfile.CharacterData.PmcData.Skills.Mastering.FirstOrDefault(
                skill => skill.Id == skillToCopy.Id
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

        // Add existing completed achievements and new one for prestige
        newProfile.CharacterData.PmcData.Achievements = prePrestigeProfileClone.CharacterData.PmcData.Achievements; // this *should* only contain completed ones

        // Add "Prestigious" achievement
        if (!newProfile.CharacterData.PmcData.Achievements.ContainsKey("676091c0f457869a94017a23"))
        {
            newProfile.CharacterData.PmcData.Achievements.Add("676091c0f457869a94017a23", _timeUtil.GetTimeStamp());
        }
        // TODO: is there one for second prestige

        // Add existing Stats to profile
        newProfile.CharacterData.PmcData.Stats = prePrestigePmc.Stats;

        // Assumes Prestige data is in descending order
        var indexOfPrestigeObtained = newProfile.CharacterData.PmcData.Info.PrestigeLevel ?? 0; // Index starts at 0
        var currentPrestigeData = _databaseService.GetTemplates().Prestige.Elements[indexOfPrestigeObtained];
        var prestigeRewards = currentPrestigeData.Rewards;

        AddPrestigeRewardsToProfile(sessionId, newProfile, prestigeRewards);

        // Flag profile as having achieved this prestige level
        newProfile.CharacterData.PmcData.Prestige[currentPrestigeData.Id] = _timeUtil.GetTimeStamp();

        if (request is not null)
            // Copy transferred items
        {
            foreach (var transferRequest in request)
            {
                var item = prePrestigePmc.Inventory.Items.FirstOrDefault(item => item.Id == transferRequest.Id);
                var addItemRequest = new AddItemDirectRequest
                {
                    ItemWithModsToAdd = [item],
                    FoundInRaid = item.Upd?.SpawnedInSession,
                    UseSortingTable = false,
                    Callback = null
                };
                _inventoryHelper.AddItemToStash(
                    sessionId,
                    addItemRequest,
                    newProfile.CharacterData.PmcData,
                    _eventOutputHolder.GetOutput(sessionId)
                );
            }
        }

        // Force save of above changes to disk
        _saveServer.SaveProfile(sessionId);
    }

    private void AddPrestigeRewardsToProfile(string sessionId, SptProfile newProfile, IEnumerable<Reward> rewards)
    {
        foreach (var reward in rewards)
        {
            switch (reward.Type)
            {
                case RewardType.CustomizationDirect:
                    {
                        _profileHelper.AddHideoutCustomisationUnlock(newProfile, reward, CustomisationSource.PRESTIGE);
                        break;
                    }
                case RewardType.Skill:
                    if (Enum.TryParse(reward.Target, out SkillTypes result))
                    {
                        _profileHelper.AddSkillPointsToPlayer(
                            newProfile.CharacterData.PmcData,
                            result,
                            ((JsonElement) reward.Value).ToObject<double>()
                        );
                    }
                    else
                    {
                        _logger.Error($"Unable to parse reward Target to Enum: {reward.Target}");
                    }

                    break;
                case RewardType.Item:
                    {
                        var addItemRequest = new AddItemDirectRequest
                        {
                            ItemWithModsToAdd = reward.Items,
                            FoundInRaid = reward.Items.FirstOrDefault()?.Upd?.SpawnedInSession,
                            UseSortingTable = false,
                            Callback = null
                        };
                        _inventoryHelper.AddItemToStash(
                            sessionId,
                            addItemRequest,
                            newProfile.CharacterData.PmcData,
                            _eventOutputHolder.GetOutput(sessionId)
                        );
                        break;
                    }
                case RewardType.ExtraDailyQuest:
                    {
                        _logger.Info("additional quests will be added when generating repeatables");
                        break;
                    }
                default:
                    _logger.Error($"Unhandled prestige reward type: {reward.Type}");
                    break;
            }
        }
    }
}
