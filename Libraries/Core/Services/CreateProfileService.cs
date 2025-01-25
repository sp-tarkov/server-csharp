using SptCommon.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Extensions;


namespace Core.Services;

[Injectable]
public class CreateProfileService(
    ISptLogger<CreateProfileService> _logger,
    TimeUtil _timeUtil,
    HashUtil _hashUtil,
    DatabaseService _databaseService,
    LocalisationService _localisationService,
    ProfileHelper _profileHelper,
    ItemHelper _itemHelper,
    TraderHelper _traderHelper,
    QuestHelper _questHelper,
    QuestRewardHelper _questRewardHelper,
    ProfileFixerService _profileFixerService,
    SaveServer _saveServer,
    EventOutputHolder _eventOutputHolder,
    PlayerScavGenerator _playerScavGenerator,
    ICloner _cloner
)
{
    public string CreateProfile(string sessionId, ProfileCreateRequestData request)
    {
        var account = _saveServer.GetProfile(sessionId).ProfileInfo;
        var profileTemplate = _cloner.Clone(_databaseService.GetProfiles()?.GetByJsonProp<ProfileSides>(account.Edition)?.GetByJsonProp<TemplateSide>(request.Side.ToLower()));
        var pmcData = profileTemplate.Character;

        // Delete existing profile
        DeleteProfileBySessionId(sessionId);
        // PMC
        pmcData.Id = account.ProfileId;
        pmcData.Aid = account.Aid;
        pmcData.Savage = account.ScavengerId;
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
        pmcData.Hideout.Seed = _timeUtil.GetTimeStamp() + 8 * 60 * 60 * 24 * 365; // 8 years in future why? who knows, we saw it in live
        pmcData.RepeatableQuests = [];
        pmcData.CarExtractCounts = new();
        pmcData.CoopExtractCounts = new();
        pmcData.Achievements = new();

        UpdateInventoryEquipmentId(pmcData);

        if (pmcData.UnlockedInfo == null)
        {
            pmcData.UnlockedInfo = new UnlockedInfo { UnlockedProductionRecipe = [] };
        }

        // Add required items to pmc stash
        AddMissingInternalContainersToProfile(pmcData);

        // Change item IDs to be unique
        _itemHelper.ReplaceProfileInventoryIds(pmcData.Inventory);

        // Create profile
        var profileDetails = new SptProfile
        {
            ProfileInfo = account,
            CharacterData = new Characters { PmcData = pmcData, ScavData = new() },
            Suits = profileTemplate.Suits,
            UserBuildData = profileTemplate.UserBuilds,
            DialogueRecords = profileTemplate.Dialogues,
            SptData = _profileHelper.GetDefaultSptDataObject(),
            VitalityData = new(),
            InraidData = new(),
            InsuranceList = [],
            TraderPurchases = new(),
            PlayerAchievements = new(),
            FriendProfileIds = [],
            CustomisationUnlocks = []
        };

        AddCustomisationUnlocksToProfile(profileDetails);

        _profileFixerService.CheckForAndFixPmcProfileIssues(profileDetails.CharacterData.PmcData);

        _saveServer.AddProfile(profileDetails);

        if (profileTemplate.Trader.SetQuestsAvailableForStart ?? false)
        {
            _questHelper.AddAllQuestsToProfile(profileDetails.CharacterData.PmcData, [QuestStatusEnum.AvailableForStart]);
        }

        // Profile is flagged as wanting quests set to ready to hand in and collect rewards
        if (profileTemplate.Trader.SetQuestsAvailableForFinish ?? false)
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
            ItemEventRouterResponse? response = _eventOutputHolder.GetOutput(sessionId);

            // Add rewards for starting quests to profile
            GivePlayerStartingQuestRewards(profileDetails, sessionId, response);
        }

        ResetAllTradersInProfile(sessionId);

        _saveServer.GetProfile(sessionId).CharacterData.ScavData = _playerScavGenerator.Generate(sessionId);

        // Store minimal profile and reload it
        _saveServer.SaveProfile(sessionId);
        _saveServer.LoadProfile(sessionId);

        // Completed account creation
        _saveServer.GetProfile(sessionId).ProfileInfo.IsWiped = false;
        _saveServer.SaveProfile(sessionId);

        return pmcData.Id;
    }

    /**
 * Delete a profile
 * @param sessionID Id of profile to delete
 */
    protected void DeleteProfileBySessionId(string sessionID)
    {
        if (_saveServer.GetProfiles().ContainsKey(sessionID))
        {
            _saveServer.DeleteProfileById(sessionID);
        }
        else
        {
            _logger.Warning(
                _localisationService.GetText("profile-unable_to_find_profile_by_id_cannot_delete", sessionID)
            );
        }
    }

    /**
 * make profiles pmcData.Inventory.equipment unique
 * @param pmcData Profile to update
 */
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

    /**
 * For each trader reset their state to what a level 1 player would see
 * @param sessionId Session id of profile to reset
 */
    protected void ResetAllTradersInProfile(string sessionId)
    {
        foreach (var traderId in _databaseService.GetTraders().Keys)
        {
            _traderHelper.ResetTrader(sessionId, traderId);
        }
    }

    /**
     * Ensure a profile has the necessary internal containers e.g. questRaidItems / sortingTable
     * DOES NOT check that stash exists
     * @param pmcData Profile to check
     */
    protected void AddMissingInternalContainersToProfile(PmcData pmcData)
    {
        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.HideoutCustomizationStashId))
        {
            pmcData.Inventory.Items.Add(
                new()
                {
                    Id = pmcData.Inventory.HideoutCustomizationStashId,
                    Template = ItemTpl.HIDEOUTAREACONTAINER_CUSTOMIZATION,
                }
            );
        }

        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.SortingTable))
        {
            pmcData.Inventory.Items.Add(
                new()
                {
                    Id = pmcData.Inventory.SortingTable,
                    Template = ItemTpl.SORTINGTABLE_SORTING_TABLE,
                }
            );
        }

        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.QuestStashItems))
        {
            pmcData.Inventory.Items.Add(
                new()
                {
                    Id = pmcData.Inventory.QuestStashItems,
                    Template = ItemTpl.STASH_QUESTOFFLINE,
                }
            );
        }

        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.QuestRaidItems))
        {
            pmcData.Inventory.Items.Add(
                new()
                {
                    Id = pmcData.Inventory.QuestRaidItems,
                    Template = ItemTpl.STASH_QUESTRAID,
                }
            );
        }
    }

    /// <summary>
    /// Add customisations to game profiles based on game edition
    /// </summary>
    /// <param name="fullProfile">Profile to add customisations to</param>
    public void AddCustomisationUnlocksToProfile(SptProfile fullProfile)
    {
        // Some game versions have additional customisation unlocks
        var gameEdition = GetGameEdition(fullProfile);
        if (gameEdition is null)
        {
            _logger.Error($"Unable to get Game edition of profile: {fullProfile.ProfileInfo.ProfileId}, skipping customisation unlocks");
            return;
        }

        switch (gameEdition)
        {
            case GameEditions.EDGE_OF_DARKNESS:
                // Gets EoD tags
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "6746fd09bafff85008048838",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );

                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "67471938bafff850080488b7",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );

                break;
            case GameEditions.UNHEARD:
                // Gets EoD+Unheard tags
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "6746fd09bafff85008048838",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );

                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "67471938bafff850080488b7",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );

                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "67471928d17d6431550563b5",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );

                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "6747193f170146228c0d2226",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );

                // Unheard Clothing (Cultist Hood)
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "666841a02537107dc508b704",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.SUITE,
                    }
                );

                // Unheard background
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "675850ba33627edb710b0592",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.ENVIRONMENT,
                    }
                );

                break;
        }

        var pretigeLevel = fullProfile?.CharacterData?.PmcData?.Info?.PrestigeLevel;
        if (pretigeLevel is not null)
        {
            if (pretigeLevel >= 1)
            {
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "674dbf593bee1152d407f005",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );
            }

            if (pretigeLevel >= 2)
            {
                fullProfile.CustomisationUnlocks.Add(
                    new CustomisationStorage
                    {
                        Id = "675dcfea7ae1a8792107ca99",
                        Source = CustomisationSource.DEFAULT,
                        Type = CustomisationType.DOG_TAG,
                    }
                );
            }
        }

        // Dev profile additions
        if (fullProfile.ProfileInfo.Edition.ToLower().Contains("developer"))
        {
            // CyberTark background
            fullProfile.CustomisationUnlocks.Add(
                new CustomisationStorage
                {
                    Id = "67585108def253bd97084552",
                    Source = CustomisationSource.DEFAULT,
                    Type = CustomisationType.ENVIRONMENT,
                }
            );
        }
    }

    /**
     * Get the game edition of a profile chosen on creation in Launcher
     */
    private string? GetGameEdition(SptProfile profile)
    {
        var edition = profile.CharacterData?.PmcData?.Info?.GameVersion;
        if (edition is null)
        {
            return null;
        }

        // Edge case - profile not created yet, fall back to what launcher has set
        var launcherEdition = profile.ProfileInfo.Edition;
        switch (launcherEdition.ToLower())
        {
            case "edge of darkness":
                return GameEditions.EDGE_OF_DARKNESS;
            case "unheard":
                return GameEditions.UNHEARD;
            default:
                return GameEditions.STANDARD;
        }
    }

    /**
     * Iterate over all quests in player profile, inspect rewards for the quests current state (accepted/completed)
     * and send rewards to them in mail
     * @param profileDetails Player profile
     * @param sessionID Session id
     * @param response Event router response
     */
    protected void GivePlayerStartingQuestRewards(
        SptProfile profileDetails,
        string sessionID,
        ItemEventRouterResponse response
    )
    {
        foreach (var quest in profileDetails.CharacterData.PmcData.Quests)
        {
            var questFromDb = _questHelper.GetQuestFromDb(quest.QId, profileDetails.CharacterData.PmcData);

            // Get messageId of text to send to player as text message in game
            // Copy of code from QuestController.acceptQuest()
            var messageId = _questHelper.GetMessageIdForQuestStart(
                questFromDb.StartedMessageText,
                questFromDb.Description
            );
            var itemRewards = _questRewardHelper.ApplyQuestReward(
                profileDetails.CharacterData.PmcData,
                quest.QId,
                QuestStatusEnum.Started,
                sessionID,
                response
            );

            /* TODO:
        _mailSendService.sendLocalisedNpcMessageToPlayer(
            sessionID,
            this.traderHelper.getTraderById(questFromDb.traderId),
            MessageType.QUEST_START,
            messageId,
            itemRewards,
            this.timeUtil.getHoursAsSeconds(100),
        );
        */
        }
    }
}
