using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Controllers;

[Injectable]
public class ProfileController
{
    protected Models.Utils.ILogger _logger;

    protected HashUtil _hashUtil;
    protected ICloner _cloner;
    protected TimeUtil _timeUtil;
    protected SaveServer _saveServer;
    protected DatabaseService _databaseService;
    protected ItemHelper _itemHelper;
    protected ProfileFixerService _profileFixerService;
    protected LocalisationService _localisationService;

    protected SeasonalEventService _seasonalEventService;

    // TODO: MailSendService mailSendService: MailSendService
    protected PlayerScavGenerator _playerScavGenerator;
    private readonly EventOutputHolder _eventOutputHolder;

    protected TraderHelper _traderHelper;
    protected DialogueHelper _dialogueHelper;
    protected QuestHelper _questHelper;
    private readonly QuestRewardHelper _questRewardHelper;
    protected ProfileHelper _profileHelper;

    public ProfileController(
        Models.Utils.ILogger logger,
        HashUtil hashUtil,
        ICloner cloner,
        TimeUtil timeUtil,
        SaveServer saveServer,
        DatabaseService databaseService,
        ItemHelper itemHelper,
        ProfileFixerService profileFixerService,
        LocalisationService localisationService,
        SeasonalEventService seasonalEventService,
        // TODO: MailSendService mailSendService,
        PlayerScavGenerator playerScavGenerator,
        EventOutputHolder eventOutputHolder,
        TraderHelper traderHelper,
        DialogueHelper dialogueHelper,
        QuestHelper questHelper,
        QuestRewardHelper questRewardHelper,
        ProfileHelper profileHelper
    )
    {
        _logger = logger;
        _cloner = cloner;
        _hashUtil = hashUtil;
        _timeUtil = timeUtil;
        _saveServer = saveServer;
        _databaseService = databaseService;
        _itemHelper = itemHelper;
        _profileFixerService = profileFixerService;
        _localisationService = localisationService;
        _seasonalEventService = seasonalEventService;
        _playerScavGenerator = playerScavGenerator;
        _eventOutputHolder = eventOutputHolder;
        _traderHelper = traderHelper;
        _dialogueHelper = dialogueHelper;
        _questHelper = questHelper;
        _questRewardHelper = questRewardHelper;
        _profileHelper = profileHelper;
    }

    /**
     * Handle /launcher/profiles
     */
    public List<MiniProfile> GetMiniProfiles()
    {
        return _saveServer.GetProfiles().Select(kv => GetMiniProfile(kv.Key)).ToList();
    }

    /**
     * Handle launcher/profile/info
     */
    public MiniProfile GetMiniProfile(string sessionID)
    {
        var profile = _saveServer.GetProfile(sessionID);
        if (profile?.CharacterData == null)
        {
            throw new Exception($"Unable to find character data for id: {sessionID}. Profile may be corrupt");
        }

        var pmc = profile.CharacterData.PmcData;
        var maxlvl = _profileHelper.GetMaxLevel();

        // Player hasn't completed profile creation process, send defaults
        if (pmc?.Info?.Level == null)
        {
            return new MiniProfile()
            {
                Username = profile.ProfileInfo?.Username ?? "",
                Nickname = "unknown",
                Side = "unknown",
                CurrentLevel = 0,
                CurrentExperience = 0,
                PreviousExperience = 0,
                NextLevel = 0,
                MaxLevel = maxlvl,
                Edition = profile.ProfileInfo?.Edition ?? "",
                ProfileId = profile.ProfileInfo?.ProfileId ?? "",
                SptData = _profileHelper.GetDefaultSptDataObject(),
            };
        }

        var currlvl = pmc.Info.Level;
        var nextlvl = _profileHelper.GetExperience((int)(currlvl + 1));
        return new MiniProfile()
        {
            Username = profile.ProfileInfo.Username,
            Nickname = pmc.Info.Nickname,
            Side = pmc.Info.Side,
            CurrentLevel = (int)(pmc.Info.Level),
            CurrentExperience = (int)(pmc.Info.Experience ?? 0),
            PreviousExperience = currlvl == 0 ? 0 : _profileHelper.GetExperience((int)currlvl),
            NextLevel = nextlvl,
            MaxLevel = maxlvl,
            Edition = profile.ProfileInfo?.Edition ?? "",
            ProfileId = profile.ProfileInfo?.ProfileId ?? "",
            SptData = profile.SptData,
        };
    }

    /**
     * Handle client/game/profile/list
     */
    public List<PmcData> GetCompleteProfile(string sessionID)
    {
        return _profileHelper.GetCompleteProfile(sessionID);
    }

    /**
     * Handle client/game/profile/create
     * @param info Client reqeust object
     * @param sessionID Player id
     * @returns Profiles _id value
     */
    public string CreateProfile(ProfileCreateRequestData info, string sessionID)
    {
        var account = _saveServer.GetProfile(sessionID).ProfileInfo;
        var profileTemplate = _cloner.Clone(_databaseService.GetProfiles()?[account.Edition]?[info.Side.ToLower()]);
        var pmcData = profileTemplate.Character;

        // Delete existing profile
        DeleteProfileBySessionId(sessionID);
        // PMC
        pmcData.Id = account.ProfileId;
        pmcData.Aid = account.Aid;
        pmcData.Savage = account.ScavengerId;
        pmcData.SessionId = sessionID;
        pmcData.Info.Nickname = info.Nickname;
        pmcData.Info.LowerNickname = account.Username.ToLower();
        pmcData.Info.RegistrationDate = _timeUtil.GetTimeStamp();
        pmcData.Info.Voice = _databaseService.GetCustomization()[info.VoiceId].Name;
        pmcData.Stats = _profileHelper.GetDefaultCounters();
        pmcData.Info.NeedWipeOptions = [];
        pmcData.Customization.Head = info.HeadId;
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
        pmcData.Inventory.Items = _itemHelper.ReplaceIDs(
            pmcData.Inventory.Items,
            pmcData,
            null,
            pmcData.Inventory.FastPanel
        );

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
        };

        _profileFixerService.CheckForAndFixPmcProfileIssues(profileDetails.CharacterData.PmcData);

        _saveServer.AddProfile(profileDetails);

        if (profileTemplate.Trader.SetQuestsAvailableForStart ?? false)
        {
            _questHelper.AddAllQuestsToProfile(profileDetails.CharacterData.PmcData, [QuestStatusEnum.AvailableForStart]);
        }

        // Profile is flagged as wanting quests set to ready to hand in and collect rewards
        if (profileTemplate.Trader.SetQuestsAvailableForFinish ?? false)
        {
            _questHelper.AddAllQuestsToProfile(profileDetails.CharacterData.PmcData, [
                QuestStatusEnum.AvailableForStart,
                QuestStatusEnum.Started,
                QuestStatusEnum.AvailableForFinish,
            ]);

            // Make unused response so applyQuestReward works
            ItemEventRouterResponse? response = _eventOutputHolder.GetOutput(sessionID);

            // Add rewards for starting quests to profile
            GivePlayerStartingQuestRewards(profileDetails, sessionID, response);
        }

        ResetAllTradersInProfile(sessionID);

        _saveServer.GetProfile(sessionID).CharacterData.ScavData = GeneratePlayerScav(sessionID);

        // Store minimal profile and reload it
        _saveServer.SaveProfile(sessionID);
        _saveServer.LoadProfile(sessionID);

        // Completed account creation
        _saveServer.GetProfile(sessionID).ProfileInfo.IsWiped = false;
        _saveServer.SaveProfile(sessionID);

        return pmcData.Id;
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
     * Ensure a profile has the necessary internal containers e.g. questRaidItems / sortingTable
     * DOES NOT check that stash exists
     * @param pmcData Profile to check
     */
    protected void AddMissingInternalContainersToProfile(PmcData pmcData)
    {
        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.HideoutCustomizationStashId))
        {
            pmcData.Inventory.Items.Add(new()
            {
                Id = pmcData.Inventory.HideoutCustomizationStashId,
                Template = ItemTpl.HIDEOUTAREACONTAINER_CUSTOMIZATION,
            });
        }

        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.SortingTable))
        {
            pmcData.Inventory.Items.Add(new()
            {
                Id = pmcData.Inventory.SortingTable,
                Template = ItemTpl.SORTINGTABLE_SORTING_TABLE,
            });
        }

        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.QuestStashItems))
        {
            pmcData.Inventory.Items.Add(new()
            {
                Id = pmcData.Inventory.QuestStashItems,
                Template = ItemTpl.STASH_QUESTOFFLINE,
            });
        }

        if (!pmcData.Inventory.Items.Any((item) => item.Id == pmcData.Inventory.QuestRaidItems))
        {
            pmcData.Inventory.Items.Add(new()
            {
                Id = pmcData.Inventory.QuestRaidItems,
                Template = ItemTpl.STASH_QUESTRAID,
            });
        }
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
     * Generate a player scav object
     * PMC profile MUST exist first before pscav can be generated
     * @param sessionID
     * @returns IPmcData object
     */
    public PmcData GeneratePlayerScav(string sessionID)
    {
        return _playerScavGenerator.Generate(sessionID);
    }

    /**
     * Handle client/game/profile/nickname/validate
     */
    public string ValidateNickname(ValidateNicknameRequestData info, string sessionID)
    {
        if (info.Nickname.Length < 3)
        {
            return "tooshort";
        }

        if (_profileHelper.IsNicknameTaken(info, sessionID))
        {
            return "taken";
        }

        return "OK";
    }

    /**
     * Handle client/game/profile/nickname/change event
     * Client allows player to adjust their profile name
     */
    public string ChangeNickname(ProfileChangeNicknameRequestData info, string sessionID)
    {
        var output = ValidateNickname(new ValidateNicknameRequestData() { Nickname = info.Nickname }, sessionID);

        if (output == "OK")
        {
            var pmcData = _profileHelper.GetPmcProfile(sessionID);

            pmcData.Info.Nickname = info.Nickname;
            pmcData.Info.LowerNickname = info.Nickname.ToLower();
        }

        return output;
    }

    /**
     * Handle client/game/profile/voice/change event
     */
    public void ChangeVoice(ProfileChangeVoiceRequestData info, string sessionID)
    {
        var pmcData = _profileHelper.GetPmcProfile(sessionID);
        pmcData.Info.Voice = info.Voice;
    }

    /**
     * Handle client/game/profile/search
     */
    public List<SearchFriendResponse> GetFriends(SearchFriendRequestData info, string sessionID)
    {
        // TODO: We should probably rename this method in the next client update
        var result = new List<SearchFriendResponse>();

        // Find any profiles with a nickname containing the entered name
        var allProfiles = _saveServer.GetProfiles().Values;

        foreach (var profile in allProfiles)
        {
            var pmcProfile = profile?.CharacterData?.PmcData;

            if (!pmcProfile?.Info?.LowerNickname?.Contains(info.Nickname.ToLower()) ?? false)
            {
                continue;
            }

            result.Add(_profileHelper.GetChatRoomMemberFromPmcProfile(pmcProfile));
        }

        return result;
    }

    /**
     * Handle client/profile/status
     */
    public GetProfileStatusResponseData GetProfileStatus(string sessionId)
    {
        var account = _saveServer.GetProfile(sessionId).ProfileInfo;
        var response = new GetProfileStatusResponseData()
        {
            MaxPveCountExceeded = false,
            Profiles =
            [
                new() { ProfileId = account.ScavengerId, ProfileToken = null, Status = "Free", Sid = "", Ip = "", Port = 0 },
                new() { ProfileId = account.ProfileId, ProfileToken = null, Status = "Free", Sid = "", Ip = "", Port = 0 },
            ]
        };

        return response;
    }

    /**
     * Handle client/profile/view
     */
    public GetOtherProfileResponse GetOtherProfile(string sessionId, GetOtherProfileRequest request)
    {
        // Find the profile by the account ID, fall back to the current player if we can't find the account
        var profile = _profileHelper.GetFullProfileByAccountId(request.AccountId);
        if (profile?.CharacterData?.PmcData == null || profile?.CharacterData?.ScavData == null)
        {
            profile = _profileHelper.GetFullProfile(sessionId);
        }

        var playerPmc = profile.CharacterData.PmcData;
        var playerScav = profile.CharacterData.ScavData;

        return new GetOtherProfileResponse()
        {
            Id = playerPmc.Id,
            Aid = playerPmc.Aid as int?,
            Info =
            {
                Nickname = playerPmc.Info.Nickname,
                Side = playerPmc.Info.Side,
                Experience = playerPmc.Info.Experience as int?,
                MemberCategory = playerPmc.Info.MemberCategory as int?,
                BannedState = playerPmc.Info.BannedState,
                BannedUntil = playerPmc.Info.BannedUntil,
                RegistrationDate = playerPmc.Info.RegistrationDate,
            },
            Customization =
            {
                Head = playerPmc.Customization.Head,
                Body = playerPmc.Customization.Body,
                Feet = playerPmc.Customization.Feet,
                Hands = playerPmc.Customization.Hands,
                Dogtag = playerPmc.Customization.DogTag,
            },
            Skills = playerPmc.Skills,
            Equipment =
            {
                Id = playerPmc.Inventory.Equipment,
                Items = playerPmc.Inventory.Items,
            },
            Achievements = playerPmc.Achievements,
            FavoriteItems = _profileHelper.GetOtherProfileFavorites(playerPmc),
            PmcStats =
            {
                Eft =
                {
                    TotalInGameTime = playerPmc.Stats.Eft.TotalInGameTime as int?,
                    OverAllCounters = playerPmc.Stats.Eft.OverallCounters,
                },
            },
            ScavStats =
            {
                Eft =
                {
                    TotalInGameTime = playerScav.Stats.Eft.TotalInGameTime as int?,
                    OverAllCounters = playerScav.Stats.Eft.OverallCounters,
                }
            }
        };
    }

    /**
     * Handle client/profile/settings
     */
    public bool SetChosenProfileIcon(string sessionId, GetProfileSettingsRequest request)
    {
        var profileToUpdate = _profileHelper.GetPmcProfile(sessionId);
        if (profileToUpdate == null)
        {
            return false;
        }

        if (request.MemberCategory != null)
        {
            profileToUpdate.Info.SelectedMemberCategory = request.MemberCategory as MemberCategory?;
        }

        if (request.SquadInviteRestriction != null)
        {
            profileToUpdate.Info.SquadInviteRestriction = request.SquadInviteRestriction;
        }

        return true;
    }
}
