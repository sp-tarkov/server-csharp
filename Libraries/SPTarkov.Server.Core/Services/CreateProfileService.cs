using System.Security.Cryptography;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Vitality = SPTarkov.Server.Core.Models.Eft.Profile.Vitality;

namespace SPTarkov.Server.Core.Services;

[Injectable]
public class CreateProfileService(
    ISptLogger<CreateProfileService> _logger,
    TimeUtil _timeUtil,
    HashUtil _hashUtil,
    DatabaseService _databaseService,
    ServerLocalisationService _serverLocalisationService,
    ProfileHelper _profileHelper,
    ItemHelper _itemHelper,
    TraderHelper _traderHelper,
    QuestHelper _questHelper,
    QuestRewardHelper _questRewardHelper,
    PrestigeHelper _prestigeHelper,
    RewardHelper _rewardHelper,
    ProfileFixerService _profileFixerService,
    SaveServer _saveServer,
    EventOutputHolder _eventOutputHolder,
    PlayerScavGenerator _playerScavGenerator,
    ICloner _cloner,
    MailSendService _mailSendService
)
{
    public async ValueTask<string> CreateProfile(string sessionId, ProfileCreateRequestData request)
    {
        var account = _cloner.Clone(_saveServer.GetProfile(sessionId));
        var profileTemplateClone = _cloner.Clone(
            _profileHelper.GetProfileTemplateForSide(account.ProfileInfo.Edition, request.Side)
        );

        var pmcData = profileTemplateClone.Character;

        // Delete existing profile
        DeleteProfileBySessionId(sessionId);
        // PMC
        pmcData.Id = account.ProfileInfo.ProfileId;
        pmcData.Aid = account.ProfileInfo.Aid;
        pmcData.Savage = account.ProfileInfo.ScavengerId;
        pmcData.SessionId = sessionId;
        pmcData.Info.Nickname = request.Nickname;
        pmcData.Info.LowerNickname = request.Nickname.ToLower();
        pmcData.Info.RegistrationDate = (int)_timeUtil.GetTimeStamp();
        pmcData.Info.Voice = _databaseService.GetCustomization()[request.VoiceId].Name;
        pmcData.Stats = _profileHelper.GetDefaultCounters();
        pmcData.Info.NeedWipeOptions = [];
        pmcData.Customization.Head = request.HeadId;
        pmcData.Health.UpdateTime = _timeUtil.GetTimeStamp();
        pmcData.Quests = [];
        pmcData.Hideout.Seed = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
        pmcData.RepeatableQuests = [];
        pmcData.CarExtractCounts = new Dictionary<string, int>();
        pmcData.CoopExtractCounts = new Dictionary<string, int>();
        pmcData.Achievements = new Dictionary<string, long>();

        // Process handling if the account has been forced to wipe
        // BSG keeps both the achievements, prestige level and the total in-game time in a wipe
        if (account.CharacterData.PmcData.Achievements is not null)
        {
            pmcData.Achievements = account.CharacterData.PmcData.Achievements;
        }

        if (account.CharacterData.PmcData.Prestige is not null)
        {
            pmcData.Prestige = account.CharacterData.PmcData.Prestige;
            pmcData.Info.PrestigeLevel = account.CharacterData.PmcData.Info.PrestigeLevel;
        }

        if (account.CharacterData?.PmcData?.Stats?.Eft is not null)
        {
            if (pmcData.Stats.Eft is not null)
            {
                pmcData.Stats.Eft.TotalInGameTime = account
                    .CharacterData
                    .PmcData
                    .Stats
                    .Eft
                    .TotalInGameTime;
            }
        }

        UpdateInventoryEquipmentId(pmcData);

        pmcData.UnlockedInfo ??= new UnlockedInfo { UnlockedProductionRecipe = [] };

        // Add required items to pmc stash
        AddMissingInternalContainersToProfile(pmcData);

        // Change item IDs to be unique
        _itemHelper.ReplaceProfileInventoryIds(pmcData.Inventory);

        // Create profile
        var profileDetails = new SptProfile
        {
            ProfileInfo = account.ProfileInfo,
            CharacterData = new Characters { PmcData = pmcData, ScavData = new PmcData() },
            UserBuildData = profileTemplateClone.UserBuilds,
            DialogueRecords = profileTemplateClone.Dialogues,
            SptData = _profileHelper.GetDefaultSptDataObject(),
            VitalityData = new Vitality(),
            InraidData = new Inraid(),
            InsuranceList = [],
            BtrDeliveryList = [],
            TraderPurchases = new Dictionary<string, Dictionary<string, TraderPurchaseData>?>(),
            FriendProfileIds = [],
            CustomisationUnlocks = [],
        };

        profileDetails.AddCustomisationUnlocksToProfile();

        profileDetails.AddSuitsToProfile(profileTemplateClone.Suits);

        _profileFixerService.CheckForAndFixPmcProfileIssues(profileDetails.CharacterData.PmcData);

        if (profileDetails.CharacterData.PmcData.Achievements.Count > 0)
        {
            var achievementsDb = _databaseService.GetTemplates().Achievements;
            var achievementRewardItemsToSend = new List<Item>();

            foreach (var (achievementId, _) in profileDetails.CharacterData.PmcData.Achievements)
            {
                var rewards = achievementsDb
                    .FirstOrDefault(achievementDb => achievementDb.Id == achievementId)
                    ?.Rewards;

                if (rewards is null)
                {
                    continue;
                }

                achievementRewardItemsToSend.AddRange(
                    _rewardHelper.ApplyRewards(
                        rewards,
                        CustomisationSource.ACHIEVEMENT,
                        profileDetails,
                        profileDetails.CharacterData.PmcData,
                        achievementId
                    )
                );
            }

            if (achievementRewardItemsToSend.Count > 0)
            {
                _mailSendService.SendLocalisedSystemMessageToPlayer(
                    profileDetails.ProfileInfo.ProfileId,
                    "670547bb5fa0b1a7c30d5836 0",
                    achievementRewardItemsToSend,
                    [],
                    31536000
                );
            }
        }

        // Process handling if the account is forced to prestige, or if the account currently has any pending prestiges
        if (
            request.SptForcePrestigeLevel is not null
            || account.SptData?.PendingPrestige is not null
        )
        {
            var pendingPrestige = account.SptData.PendingPrestige is not null
                ? account.SptData.PendingPrestige
                : new PendingPrestige { PrestigeLevel = request.SptForcePrestigeLevel };

            _prestigeHelper.ProcessPendingPrestige(account, profileDetails, pendingPrestige);
        }

        _saveServer.AddProfile(profileDetails);

        if (profileTemplateClone.Trader.SetQuestsAvailableForStart ?? false)
        {
            _questHelper.AddAllQuestsToProfile(
                profileDetails.CharacterData.PmcData,
                [QuestStatusEnum.AvailableForStart]
            );
        }

        // Profile is flagged as wanting quests set to ready to hand in and collect rewards
        if (profileTemplateClone.Trader.SetQuestsAvailableForFinish ?? false)
        {
            _questHelper.AddAllQuestsToProfile(
                profileDetails.CharacterData.PmcData,
                [
                    QuestStatusEnum.AvailableForStart,
                    QuestStatusEnum.Started,
                    QuestStatusEnum.AvailableForFinish,
                ]
            );

            // Make unused response so applyQuestReward works
            var response = _eventOutputHolder.GetOutput(sessionId);

            // Add rewards for starting quests to profile
            GivePlayerStartingQuestRewards(profileDetails, sessionId, response);
        }

        ResetAllTradersInProfile(sessionId);

        _saveServer.GetProfile(sessionId).CharacterData.ScavData = _playerScavGenerator.Generate(
            sessionId
        );

        // Store minimal profile and reload it
        await _saveServer.SaveProfileAsync(sessionId);
        await _saveServer.LoadProfileAsync(sessionId);

        // Completed account creation
        _saveServer.GetProfile(sessionId).ProfileInfo.IsWiped = false;
        await _saveServer.SaveProfileAsync(sessionId);

        return pmcData.Id;
    }

    /// <summary>
    ///     Delete a profile
    /// </summary>
    /// <param name="sessionID"> ID of profile to delete </param>
    protected void DeleteProfileBySessionId(string sessionID)
    {
        if (_saveServer.GetProfiles().ContainsKey(sessionID))
        {
            _saveServer.DeleteProfileById(sessionID);
        }
        else
        {
            _logger.Warning(
                _serverLocalisationService.GetText(
                    "profile-unable_to_find_profile_by_id_cannot_delete",
                    sessionID
                )
            );
        }
    }

    /// <summary>
    ///     Make profiles pmcData.Inventory.equipment unique
    /// </summary>
    /// <param name="pmcData"> Profile to update </param>
    protected void UpdateInventoryEquipmentId(PmcData pmcData)
    {
        var oldEquipmentId = pmcData.Inventory.Equipment;
        pmcData.Inventory.Equipment = _hashUtil.Generate();

        foreach (var item in pmcData.Inventory.Items)
        {
            if (item.ParentId == oldEquipmentId)
            {
                item.ParentId = pmcData.Inventory.Equipment;
                continue;
            }

            if (item.Id == oldEquipmentId)
            {
                item.Id = pmcData.Inventory.Equipment;
            }
        }
    }

    /// <summary>
    ///     For each trader reset their state to what a level 1 player would see
    /// </summary>
    /// <param name="sessionId"> Session ID of profile to reset </param>
    protected void ResetAllTradersInProfile(string sessionId)
    {
        foreach (var traderId in _databaseService.GetTraders().Keys)
        {
            _traderHelper.ResetTrader(sessionId, traderId);
        }
    }

    /// <summary>
    ///     Ensure a profile has the necessary internal containers e.g. questRaidItems / sortingTable <br />
    ///     DOES NOT check that stash exists
    /// </summary>
    /// <param name="pmcData"> Profile to check </param>
    protected void AddMissingInternalContainersToProfile(PmcData pmcData)
    {
        if (
            !pmcData.Inventory.Items.Any(item =>
                item.Id == pmcData.Inventory.HideoutCustomizationStashId
            )
        )
        {
            pmcData.Inventory.Items.Add(
                new Item
                {
                    Id = pmcData.Inventory.HideoutCustomizationStashId,
                    Template = ItemTpl.HIDEOUTAREACONTAINER_CUSTOMIZATION,
                }
            );
        }

        if (!pmcData.Inventory.Items.Any(item => item.Id == pmcData.Inventory.SortingTable))
        {
            pmcData.Inventory.Items.Add(
                new Item
                {
                    Id = pmcData.Inventory.SortingTable,
                    Template = ItemTpl.SORTINGTABLE_SORTING_TABLE,
                }
            );
        }

        if (!pmcData.Inventory.Items.Any(item => item.Id == pmcData.Inventory.QuestStashItems))
        {
            pmcData.Inventory.Items.Add(
                new Item
                {
                    Id = pmcData.Inventory.QuestStashItems,
                    Template = ItemTpl.STASH_QUESTOFFLINE,
                }
            );
        }

        if (!pmcData.Inventory.Items.Any(item => item.Id == pmcData.Inventory.QuestRaidItems))
        {
            pmcData.Inventory.Items.Add(
                new Item
                {
                    Id = pmcData.Inventory.QuestRaidItems,
                    Template = ItemTpl.STASH_QUESTRAID,
                }
            );
        }
    }

    /// <summary>
    ///     Iterate over all quests in player profile, inspect rewards for the quests current state (accepted/completed)
    ///     and send rewards to them in mail
    /// </summary>
    /// <param name="profileDetails"> Player profile </param>
    /// <param name="sessionID"> Session ID </param>
    /// <param name="response"> Event router response </param>
    protected void GivePlayerStartingQuestRewards(
        SptProfile profileDetails,
        string sessionID,
        ItemEventRouterResponse response
    )
    {
        foreach (var quest in profileDetails.CharacterData.PmcData.Quests)
        {
            var questFromDb = _questHelper.GetQuestFromDb(
                quest.QId,
                profileDetails.CharacterData.PmcData
            );

            // Get messageId of text to send to player as text message in game
            // Copy of code from QuestController.acceptQuest()
            var messageId = _questHelper.GetMessageIdForQuestStart(
                questFromDb.StartedMessageText,
                questFromDb.Description
            );
            var itemRewards = _questRewardHelper
                .ApplyQuestReward(
                    profileDetails.CharacterData.PmcData,
                    quest.QId,
                    QuestStatusEnum.Started,
                    sessionID,
                    response
                )
                .ToList();

            _mailSendService.SendLocalisedNpcMessageToPlayer(
                sessionID,
                questFromDb.TraderId,
                MessageType.QuestStart,
                messageId,
                itemRewards,
                _timeUtil.GetHoursAsSeconds(100)
            );
        }
    }
}
