using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class ProfileController(
    ISptLogger<ProfileController> _logger,
    HashUtil _hashUtil,
    ICloner _cloner,
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    DatabaseService _databaseService,
    ItemHelper _itemHelper,
    ProfileFixerService _profileFixerService,
    LocalisationService _localisationService,
    CreateProfileService _createProfileService,
    SeasonalEventService _seasonalEventService,
    PlayerScavGenerator _playerScavGenerator,
    EventOutputHolder _eventOutputHolder,
    TraderHelper _traderHelper,
    DialogueHelper _dialogueHelper,
    QuestHelper _questHelper,
    QuestRewardHelper _questRewardHelper,
    ProfileHelper _profileHelper
)
{
    /// <summary>
    ///     Handle /launcher/profiles
    /// </summary>
    /// <returns></returns>
    public virtual List<MiniProfile> GetMiniProfiles()
    {
        return _saveServer.GetProfiles().Select(kvp => GetMiniProfile(kvp.Key)).ToList();
    }

    /// <summary>
    ///     Handle launcher/profile/info
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public virtual MiniProfile GetMiniProfile(string sessionId)
    {
        var profile = _saveServer.GetProfile(sessionId);
        if (profile?.CharacterData == null)
        {
            throw new Exception($"Unable to find character data for id: {sessionId}. Profile may be corrupt");
        }

        var pmc = profile.CharacterData.PmcData;
        var maxLvl = _profileHelper.GetMaxLevel();

        // Player hasn't completed profile creation process, send defaults
        var currentLevel = pmc?.Info?.Level.GetValueOrDefault(1);
        var xpToNextLevel = _profileHelper.GetExperience((currentLevel ?? 1) + 1);
        if (pmc?.Info?.Level == null)
        {
            return new MiniProfile
            {
                Username = profile.ProfileInfo?.Username ?? "",
                Nickname = "unknown",
                HasPassword = profile.ProfileInfo?.Password != "",
                Side = "unknown",
                CurrentLevel = 0,
                CurrentExperience = 0,
                PreviousExperience = 0,
                NextLevel = xpToNextLevel,
                MaxLevel = maxLvl,
                Edition = profile.ProfileInfo?.Edition ?? "",
                ProfileId = profile.ProfileInfo?.ProfileId ?? "",
                SptData = _profileHelper.GetDefaultSptDataObject()
            };
        }

        return new MiniProfile
        {
            Username = profile.ProfileInfo?.Username,
            Nickname = pmc.Info.Nickname,
            HasPassword = profile.ProfileInfo?.Password != "",
            Side = pmc.Info.Side,
            CurrentLevel = pmc.Info.Level,
            CurrentExperience = pmc.Info.Experience ?? 0,
            PreviousExperience = currentLevel == 0 ? 0 : _profileHelper.GetExperience(currentLevel.Value),
            NextLevel = xpToNextLevel,
            MaxLevel = maxLvl,
            Edition = profile.ProfileInfo?.Edition ?? "",
            ProfileId = profile.ProfileInfo?.ProfileId ?? "",
            SptData = profile.SptData
        };
    }

    /// <summary>
    ///     Handle client/game/profile/list
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>Return a full profile, scav and pmc profiles + meta data</returns>
    public virtual List<PmcData> GetCompleteProfile(string sessionID)
    {
        return _profileHelper.GetCompleteProfile(sessionID);
    }

    /// <summary>
    ///     Handle client/game/profile/create
    /// </summary>
    /// <param name="request">Create profile request</param>
    /// <param name="sessionID">Player id</param>
    /// <returns>Player id</returns>
    public virtual string CreateProfile(ProfileCreateRequestData request, string sessionID)
    {
        return _createProfileService.CreateProfile(sessionID, request);
    }

    /// <summary>
    ///     Generate a player scav object
    ///     PMC profile MUST exist first before player-scav can be generated
    /// </summary>
    /// <param name="sessionID">Player id</param>
    /// <returns>PmcData</returns>
    public virtual PmcData GeneratePlayerScav(string sessionID)
    {
        return _playerScavGenerator.Generate(sessionID);
    }

    /// <summary>
    ///     Handle client/game/profile/nickname/validate
    /// </summary>
    /// <param name="request">Validate nickname request</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns></returns>
    public virtual string ValidateNickname(ValidateNicknameRequestData request, string sessionID)
    {
        if (request.Nickname.Length < 3)
        {
            return "tooshort";
        }

        if (_profileHelper.IsNicknameTaken(request, sessionID))
        {
            return "taken";
        }

        return "OK";
    }

    /// <summary>
    ///     Handle client/game/profile/nickname/change event
    ///     Client allows player to adjust their profile name
    /// </summary>
    /// <param name="request">Change nickname request</param>
    /// <param name="sessionID">Player id</param>
    /// <returns></returns>
    public virtual string ChangeNickname(ProfileChangeNicknameRequestData request, string sessionID)
    {
        var output = ValidateNickname(
            new ValidateNicknameRequestData
            {
                Nickname = request.Nickname
            },
            sessionID
        );

        if (output == "OK")
        {
            var pmcData = _profileHelper.GetPmcProfile(sessionID);

            pmcData.Info.Nickname = request.Nickname;
            pmcData.Info.LowerNickname = request.Nickname.ToLower();
        }

        return output;
    }

    /// <summary>
    ///     Handle client/game/profile/voice/change event
    /// </summary>
    /// <param name="request">Change voice request</param>
    /// <param name="sessionID">Player id</param>
    public virtual void ChangeVoice(ProfileChangeVoiceRequestData request, string sessionID)
    {
        var pmcData = _profileHelper.GetPmcProfile(sessionID);
        pmcData.Info.Voice = request.Voice;
    }

    /// <summary>
    ///     Handle client/game/profile/search
    /// </summary>
    /// <param name="request">Search profiles request</param>
    /// <param name="sessionID">Player id</param>
    /// <returns>Found profiles</returns>
    public virtual List<SearchFriendResponse> SearchProfiles(SearchProfilesRequestData request, string sessionID)
    {
        var result = new List<SearchFriendResponse>();

        // Find any profiles with a nickname containing the entered name
        var allProfiles = _saveServer.GetProfiles().Values;

        foreach (var profile in allProfiles)
        {
            var pmcProfile = profile?.CharacterData?.PmcData;
            if (!pmcProfile?.Info?.LowerNickname?.Contains(request.Nickname.ToLower()) ?? false)
            {
                continue;
            }

            result.Add(_profileHelper.GetChatRoomMemberFromPmcProfile(pmcProfile));
        }

        return result;
    }

    /// <summary>
    ///     Handle client/profile/status
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns></returns>
    public virtual GetProfileStatusResponseData GetProfileStatus(string sessionId)
    {
        var account = _saveServer.GetProfile(sessionId).ProfileInfo;
        var response = new GetProfileStatusResponseData
        {
            MaxPveCountExceeded = false,
            Profiles =
            [
                new ProfileStatusData
                {
                    ProfileId = account.ScavengerId,
                    ProfileToken = null,
                    Status = "Free",
                    Sid = "",
                    Ip = "",
                    Port = 0
                },
                new ProfileStatusData
                {
                    ProfileId = account.ProfileId,
                    ProfileToken = null,
                    Status = "Free",
                    Sid = "",
                    Ip = "",
                    Port = 0
                }
            ]
        };

        return response;
    }

    /// <summary>
    ///     Handle client/profile/view
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request">Get other profile request</param>
    /// <returns>GetOtherProfileResponse</returns>
    public virtual GetOtherProfileResponse GetOtherProfile(string sessionId, GetOtherProfileRequest request)
    {
        // Find the profile by the account ID, fall back to the current player if we can't find the account
        var profileToView = _profileHelper.GetFullProfileByAccountId(request.AccountId);
        if (profileToView?.CharacterData?.PmcData is null || profileToView.CharacterData.ScavData is null)
        {
            _logger.Warning($"Unable to get profile: {request.AccountId} to show, falling back to own profile");
            profileToView = _profileHelper.GetFullProfile(sessionId);
        }

        var profileToViewPmc = profileToView.CharacterData.PmcData;
        var profileToViewScav = profileToView.CharacterData.ScavData;

        // Get the keys needed to find profiles hideout-related items
        var hideoutKeys = new HashSet<string>();
        hideoutKeys.UnionWith(profileToViewPmc.Inventory.HideoutAreaStashes.Keys);
        hideoutKeys.Add(profileToViewPmc.Inventory.HideoutCustomizationStashId);

        // Find hideout items e.g. posters
        var hideoutRootItems = profileToViewPmc.Inventory.Items.Where(x => hideoutKeys.Contains(x.Id));
        var itemsToReturn = new List<Item>();
        foreach (var rootItems in hideoutRootItems)
        {
            // Check each root items for children and add
            var itemWithChildren = _itemHelper.FindAndReturnChildrenAsItems(profileToViewPmc.Inventory.Items, rootItems.Id);
            itemsToReturn.AddRange(itemWithChildren);
        }

        return new GetOtherProfileResponse
        {
            Id = profileToViewPmc.Id,
            Aid = profileToViewPmc.Aid,
            Info =
            {
                Nickname = profileToViewPmc.Info.Nickname,
                Side = profileToViewPmc.Info.Side,
                Experience = profileToViewPmc.Info.Experience,
                MemberCategory = profileToViewPmc.Info.MemberCategory as int?,
                BannedState = profileToViewPmc.Info.BannedState,
                BannedUntil = profileToViewPmc.Info.BannedUntil,
                RegistrationDate = profileToViewPmc.Info.RegistrationDate
            },
            Customization =
            {
                Head = profileToViewPmc.Customization.Head,
                Body = profileToViewPmc.Customization.Body,
                Feet = profileToViewPmc.Customization.Feet,
                Hands = profileToViewPmc.Customization.Hands,
                Dogtag = profileToViewPmc.Customization.DogTag
            },
            Skills = profileToViewPmc.Skills,
            Equipment =
            {
                Id = profileToViewPmc.Inventory.Equipment,
                Items = profileToViewPmc.Inventory.Items
            },
            Achievements = profileToViewPmc.Achievements,
            FavoriteItems = _profileHelper.GetOtherProfileFavorites(profileToViewPmc),
            PmcStats =
            {
                Eft =
                {
                    TotalInGameTime = profileToViewPmc.Stats.Eft.TotalInGameTime,
                    OverAllCounters = profileToViewPmc.Stats.Eft.OverallCounters
                }
            },
            ScavStats =
            {
                Eft =
                {
                    TotalInGameTime = profileToViewScav.Stats.Eft.TotalInGameTime,
                    OverAllCounters = profileToViewScav.Stats.Eft.OverallCounters
                }
            },
            Hideout = profileToViewPmc.Hideout,
            CustomizationStash = profileToViewPmc.Inventory.HideoutCustomizationStashId,
            HideoutAreaStashes = profileToViewPmc.Inventory.HideoutAreaStashes,
            Items = itemsToReturn
        };
    }

    /// <summary>
    ///     Handle client/profile/settings
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="request">Get profile settings request</param>
    /// <returns></returns>
    public virtual bool SetChosenProfileIcon(string sessionId, GetProfileSettingsRequest request)
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
