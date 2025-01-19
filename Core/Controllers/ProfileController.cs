using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Launcher;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Controllers;

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
        var maxLvl = _profileHelper.GetMaxLevel();

        // Player hasn't completed profile creation process, send defaults
        var currlvl = pmc?.Info?.Level.GetValueOrDefault(1);
        var xpToNextLevel = _profileHelper.GetExperience((int)(currlvl ?? 1 + 1));
        if (pmc?.Info?.Level == null)
        {
            return new MiniProfile
            {
                Username = profile.ProfileInfo?.Username ?? "",
                Nickname = "unknown",
                Side = "unknown",
                CurrentLevel = 0,
                CurrentExperience = 0,
                PreviousExperience = 0,
                NextLevel = xpToNextLevel,
                MaxLevel = maxLvl,
                Edition = profile.ProfileInfo?.Edition ?? "",
                ProfileId = profile.ProfileInfo?.ProfileId ?? "",
                SptData = _profileHelper.GetDefaultSptDataObject(),
            };
        }

        return new MiniProfile
        {
            Username = profile.ProfileInfo.Username,
            Nickname = pmc.Info.Nickname,
            Side = pmc.Info.Side,
            CurrentLevel = (int)(pmc.Info.Level),
            CurrentExperience = (int)(pmc.Info.Experience ?? 0),
            PreviousExperience = currlvl == 0 ? 0 : _profileHelper.GetExperience((int)currlvl),
            NextLevel = xpToNextLevel,
            MaxLevel = maxLvl,
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
    public string CreateProfile(ProfileCreateRequestData request, string sessionID)
    {
        return _createProfileService.CreateProfile(sessionID, request);
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
        var profileToView = _profileHelper.GetFullProfileByAccountId(request.AccountId);
        if (profileToView?.CharacterData?.PmcData is null || profileToView.CharacterData.ScavData is null)
        {
            _logger.Warning($"Unable to get profile: ${request.AccountId} to show, falling back to own profile");
            profileToView = _profileHelper.GetFullProfile(sessionId);
        }

        var profileToViewPmc = profileToView.CharacterData.PmcData;
        var profileToViewScav = profileToView.CharacterData.ScavData;

        // Get the keys needed to find profiles hideout-related items
        var hideoutKeys = new List<string>();
        hideoutKeys.AddRange(profileToViewPmc.Inventory.HideoutAreaStashes.Keys);
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
                Experience = profileToViewPmc.Info.Experience as int?,
                MemberCategory = profileToViewPmc.Info.MemberCategory as int?,
                BannedState = profileToViewPmc.Info.BannedState,
                BannedUntil = profileToViewPmc.Info.BannedUntil,
                RegistrationDate = profileToViewPmc.Info.RegistrationDate,
            },
            Customization =
            {
                Head = profileToViewPmc.Customization.Head,
                Body = profileToViewPmc.Customization.Body,
                Feet = profileToViewPmc.Customization.Feet,
                Hands = profileToViewPmc.Customization.Hands,
                Dogtag = profileToViewPmc.Customization.DogTag,
            },
            Skills = profileToViewPmc.Skills,
            Equipment =
            {
                Id = profileToViewPmc.Inventory.Equipment,
                Items = profileToViewPmc.Inventory.Items,
            },
            Achievements = profileToViewPmc.Achievements,
            FavoriteItems = _profileHelper.GetOtherProfileFavorites(profileToViewPmc),
            PmcStats =
            {
                Eft =
                {
                    TotalInGameTime = profileToViewPmc.Stats.Eft.TotalInGameTime,
                    OverAllCounters = profileToViewPmc.Stats.Eft.OverallCounters,
                },
            },
            ScavStats =
            {
                Eft =
                {
                    TotalInGameTime = profileToViewScav.Stats.Eft.TotalInGameTime,
                    OverAllCounters = profileToViewScav.Stats.Eft.OverallCounters,
                }
            },
            Hideout = profileToViewPmc.Hideout,
            CustomizationStash = profileToViewPmc.Inventory.HideoutCustomizationStashId,
            HideoutAreaStashes = profileToViewPmc.Inventory.HideoutAreaStashes,
            Items = itemsToReturn
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
