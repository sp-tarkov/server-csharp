using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class PrestigeHelper(
    ISptLogger<PrestigeHelper> logger,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    MailSendService mailSendService,
    ProfileHelper profileHelper,
    RewardHelper rewardHelper
)
{
    public void ProcessPendingPrestige(
        SptProfile oldProfile,
        SptProfile newProfile,
        PendingPrestige prestige
    )
    {
        var prePrestigePmc = oldProfile.CharacterData.PmcData;
        var sessionId = newProfile.ProfileInfo.ProfileId;

        // Skill copy

        if (prePrestigePmc.Skills.Common is not null)
        {
            var commonSKillsToCopy = prePrestigePmc.Skills.Common;
            foreach (var skillToCopy in commonSKillsToCopy)
            {
                // Set progress 5% of what it was
                skillToCopy.Progress = skillToCopy.Progress * 0.05;
                var existingSkill = newProfile.CharacterData.PmcData.Skills.Common.FirstOrDefault(
                    skill => skill.Id == skillToCopy.Id
                );
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
                // Set progress 5% of what it was
                skillToCopy.Progress = skillToCopy.Progress * 0.05;
                var existingSkill =
                    newProfile.CharacterData.PmcData.Skills.Mastering.FirstOrDefault(skill =>
                        skill.Id == skillToCopy.Id
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
        }

        var indexOfPrestigeObtained = Math.Min(prestige.PrestigeLevel.Value - 1, 1); // Index starts at 0

        // Add "Prestigious" achievement
        if (!newProfile.CharacterData.PmcData.Achievements.ContainsKey("676091c0f457869a94017a23"))
        {
            rewardHelper.AddAchievementToProfile(newProfile, "676091c0f457869a94017a23");
        }

        // Assumes Prestige data is in descending order
        var currentPrestigeData = databaseService.GetTemplates().Prestige.Elements[
            indexOfPrestigeObtained
        ];
        var prestigeRewards = databaseService
            .GetTemplates()
            .Prestige.Elements.Slice(0, indexOfPrestigeObtained + 1)
            .SelectMany(prestige => prestige.Rewards);

        AddPrestigeRewardsToProfile(sessionId.Value, newProfile, prestigeRewards);

        // Flag profile as having achieved this prestige level
        newProfile.CharacterData.PmcData.Prestige[currentPrestigeData.Id] = timeUtil.GetTimeStamp();

        var itemsToTransfer = new List<Item>();

        // Copy transferred items
        foreach (var transferRequest in prestige.Items ?? [])
        {
            var item = prePrestigePmc.Inventory.Items.FirstOrDefault(item =>
                item.Id == transferRequest.Id
            );
            if (item is null)
            {
                logger.Error(
                    $"Unable to find item with id: {transferRequest.Id} in profile: {sessionId}, skipping"
                );
                continue;
            }

            itemsToTransfer.Add(item);
        }

        mailSendService.SendSystemMessageToPlayer(sessionId.Value, "", itemsToTransfer, 31536000);

        newProfile.CharacterData.PmcData.Info.PrestigeLevel = prestige.PrestigeLevel;
    }

    private void AddPrestigeRewardsToProfile(
        MongoId sessionId,
        SptProfile newProfile,
        IEnumerable<Reward> rewards
    )
    {
        var itemsToSend = new List<Item>();

        foreach (var reward in rewards)
        {
            switch (reward.Type)
            {
                case RewardType.CustomizationDirect:
                {
                    profileHelper.AddHideoutCustomisationUnlock(
                        newProfile,
                        reward,
                        CustomisationSource.PRESTIGE
                    );
                    break;
                }
                case RewardType.Skill:
                    if (Enum.TryParse(reward.Target, out SkillTypes result))
                    {
                        profileHelper.AddSkillPointsToPlayer(
                            newProfile.CharacterData.PmcData,
                            result,
                            reward.Value
                        );
                    }
                    else
                    {
                        logger.Error($"Unable to parse reward Target to Enum: {reward.Target}");
                    }

                    break;
                case RewardType.Item:
                {
                    itemsToSend.AddRange(reward.Items);
                    break;
                }
                case RewardType.ExtraDailyQuest:
                {
                    newProfile.AddExtraRepeatableQuest(reward.Target, (double)reward.Value);
                    break;
                }
                default:
                    logger.Error($"Unhandled prestige reward type: {reward.Type}");
                    break;
            }
        }

        if (itemsToSend.Count > 0)
        {
            mailSendService.SendSystemMessageToPlayer(sessionId, "", itemsToSend, 31536000);
        }
    }
}
