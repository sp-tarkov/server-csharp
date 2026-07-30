using System.Globalization;
using Microsoft.Extensions.Logging;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Helper;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers.Profile;

[Injectable]
public class ProfileHelper(
    ISptLogger<ProfileHelper> logger,
    TemplateTable templateTable,
    GlobalTable globalTable,
    ICloner cloner,
    SaveServer saveServer,
    Watermark watermark,
    TimeUtil timeUtil,
    ServerLocalisationService serverLocalisationService,
    InventoryConfig inventoryConfig
)
{
    protected static readonly HashSet<string> GameEditionsWithFreeRefresh = ["edge_of_darkness", "unheard_edition"];

    /// <summary>
    ///     Remove/reset a completed quest condition from players profile quest data
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="questConditionId">Quest with condition to remove</param>
    public void RemoveQuestConditionFromProfile(PmcData pmcData, Dictionary<string, string> questConditionId)
    {
        foreach (var questId in questConditionId)
        {
            var conditionId = questId.Value;
            var profileQuest = pmcData.Quests?.FirstOrDefault(q => q.QId == conditionId);

            // Remove condition
            profileQuest?.CompletedConditions?.Remove(conditionId);
        }
    }

    /// <summary>
    ///     Removes all trader/pmc/etc dialogues from a profile.
    ///     Useful in scenarios where the profile has too many dialogues to properly handle saving/loading due to size
    /// </summary>
    /// <param name="sessionId">Player profile</param>
    /// <returns>Whether anything was cleared and the number of dialogues removed</returns>
    public (bool Cleared, int Count) RemoveAllDialoguesFromProfile(MongoId sessionId)
    {
        var profile = GetFullProfile(sessionId);
        var dialogues = profile.DialogueRecords ?? (profile.DialogueRecords = new Dictionary<MongoId, Models.Eft.Profile.Dialogue>());

        var count = dialogues.Count;
        if (count == 0)
        {
            return (false, 0);
        }

        dialogues.Clear();
        return (true, count);
    }

    /// <summary>
    ///     Get all profiles from server
    /// </summary>
    /// <returns>Dictionary of profiles</returns>
    public Dictionary<MongoId, SptProfile> GetProfiles()
    {
        return saveServer.GetProfiles();
    }

    /// <summary>
    ///     Get the pmc and scav profiles as an array by profile id
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Array of PmcData objects</returns>
    public List<PmcData> GetCompleteProfile(MongoId sessionId)
    {
        var output = new List<PmcData>();

        if (IsWiped(sessionId))
        {
            return output;
        }

        var fullProfileClone = cloner.Clone(GetFullProfile(sessionId))!;

        // Sanitize any data the client can not receive
        SanitizeProfileForClient(fullProfileClone);

        // PMC must be at array index 0, scav at 1
        output.Add(fullProfileClone.CharacterData!.PmcData!);
        output.Add(fullProfileClone.CharacterData!.ScavData!);

        return output;
    }

    /// <summary>
    ///     Sanitize any information from the profile that the client does not expect to receive
    /// </summary>
    /// <param name="clonedProfile">A clone of the full player profile</param>
    protected void SanitizeProfileForClient(SptProfile clonedProfile)
    {
        if (clonedProfile.CharacterData?.PmcData?.TradersInfo?.Values is null)
        {
            return;
        }

        // Remove `loyaltyLevel` from `TradersInfo`, as otherwise it causes the client to not
        // properly calculate the player's `loyaltyLevel`
        foreach (var trader in clonedProfile.CharacterData.PmcData.TradersInfo.Values)
        {
            trader.LoyaltyLevel = null;
        }
    }

    /// <summary>
    ///     Check if a nickname is used by another profile loaded by the server
    /// </summary>
    /// <param name="nicknameRequest">nickname request object</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>True if already in use</returns>
    public bool IsNicknameTaken(ValidateNicknameRequestData nicknameRequest, MongoId sessionId)
    {
        var allProfiles = saveServer.GetProfiles().Values;

        // Find a profile that doesn't have same session id but has same name
        return allProfiles.Any(profile =>
            // Valid profile
            ProfileHasInfoProperty(profile)
            && profile.ProfileInfo?.ProfileId != sessionId
            // SessionIds dont match
            && StringsMatch(
                profile.CharacterData?.PmcData?.Info?.LowerNickname?.ToLowerInvariant()!,
                nicknameRequest.Nickname?.ToLowerInvariant()!
            )
        ); // Nicknames do
    }

    protected bool ProfileHasInfoProperty(SptProfile profile)
    {
        return profile.CharacterData?.PmcData?.Info != null;
    }

    protected bool StringsMatch(string stringA, string stringB)
    {
        return stringA == stringB;
    }

    /// <summary>
    ///     Add experience to a PMC inside the players profile
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="experienceToAdd">Experience to add to PMC character</param>
    public void AddExperienceToPmc(MongoId sessionId, int experienceToAdd)
    {
        var pmcData = GetPmcProfile(sessionId);
        if (pmcData?.Info != null)
        {
            pmcData.Info.Experience += experienceToAdd;
        }
        else
        {
            logger.Error($"Profile {sessionId} does not exist");
        }
    }

    /// <summary>
    ///     Iterate all profiles and find matching pmc profile by provided id
    /// </summary>
    /// <param name="pmcId">Profile id to find</param>
    /// <returns>PmcData</returns>
    public PmcData? GetProfileByPmcId(MongoId pmcId)
    {
        return saveServer.GetProfiles().Values.First(p => p.CharacterData?.PmcData?.Id == pmcId).CharacterData?.PmcData;
    }

    /// <summary>
    ///     Get experience value for given level
    /// </summary>
    /// <param name="level">Level to get xp for</param>
    /// <returns>Number of xp points for level</returns>
    public int? GetExperience(int level)
    {
        var playerLevel = level;
        var expTable = globalTable.Configuration.Exp.Level.ExperienceTable;
        int? exp = 0;

        if (playerLevel > expTable.Length)
        {
            playerLevel = expTable.Length;
        }

        for (var i = 0; i < playerLevel; i++)
        {
            exp += expTable[i].Experience;
        }

        return exp;
    }

    /// <summary>
    ///     Get level from an expected experience value
    /// </summary>
    /// <param name="experience">Experience number to get level for</param>
    /// <returns>Level expected</returns>
    public int GetLevelFromExperience(int experience)
    {
        var expTable = globalTable.Configuration.Exp.Level.ExperienceTable;

        var cumulative = 0;

        for (var level = 1; level <= expTable.Length; level++)
        {
            if (level > 1)
            {
                cumulative += expTable[level - 1].Experience;
            }

            if (experience < cumulative)
            {
                return level - 1;
            }
        }

        return expTable.Length;
    }

    /// <summary>
    ///     Get the max level a player can be
    /// </summary>
    /// <returns>Max level</returns>
    public int GetMaxLevel()
    {
        return globalTable.Configuration.Exp.Level.ExperienceTable.Length - 1;
    }

    /// <summary>
    ///     Get default Spt data object
    /// </summary>
    /// <returns>Spt</returns>
    public Spt GetDefaultSptDataObject()
    {
        return new Spt
        {
            Version = watermark.GetVersionTag(true),
            Mods = [],
            ReceivedGifts = [],
            BlacklistedItemTemplates = [],
            FreeRepeatableRefreshUsedCount = [],
            Migrations = [],
            CultistRewards = [],
            PendingPrestige = null,
            ExtraRepeatableQuests = [],
        };
    }

    /// <summary>
    ///     Get full representation of a players profile json
    /// </summary>
    /// <param name="sessionId">Profile id to get</param>
    /// <returns>SptProfile object</returns>
    public SptProfile GetFullProfile(MongoId sessionId)
    {
        return saveServer.GetProfile(sessionId);
    }

    /// <summary>
    ///     Get full representation of a players profile JSON by the account ID, or undefined if not found
    /// </summary>
    /// <param name="accountId">Account ID to find</param>
    /// <returns></returns>
    public SptProfile? GetFullProfileByAccountId(string accountId)
    {
        var check = int.TryParse(accountId, out var aid);
        if (!check)
        {
            logger.Error($"Account {accountId} does not exist");
        }

        return saveServer.GetProfiles().FirstOrDefault(p => p.Value.ProfileInfo?.Aid == aid).Value;
    }

    /// <summary>
    ///     Retrieve a ChatRoomMember formatted profile for the given session ID
    /// </summary>
    /// <param name="sessionId">The session ID to return the profile for</param>
    /// <returns></returns>
    public SearchFriendResponse? GetChatRoomMemberFromSessionId(MongoId sessionId)
    {
        var pmcProfile = GetFullProfile(sessionId).CharacterData?.PmcData;
        return pmcProfile is null ? null : GetChatRoomMemberFromPmcProfile(pmcProfile);
    }

    /// <summary>
    ///     Retrieve a ChatRoomMember formatted profile for the given PMC profile data
    /// </summary>
    /// <param name="pmcProfile">The PMC profile data to format into a ChatRoomMember structure</param>
    /// <returns></returns>
    public SearchFriendResponse GetChatRoomMemberFromPmcProfile(PmcData pmcProfile)
    {
        return new SearchFriendResponse
        {
            Id = pmcProfile.Id!.Value,
            Aid = pmcProfile.Aid,
            Info = new UserDialogDetails
            {
                Nickname = pmcProfile.Info!.Nickname,
                Side = pmcProfile.Info.Side,
                Level = pmcProfile.Info.Level,
                MemberCategory = pmcProfile.Info.MemberCategory,
                SelectedMemberCategory = pmcProfile.Info.SelectedMemberCategory,
            },
        };
    }

    /// <summary>
    ///     Get a PMC profile by its session id
    /// </summary>
    /// <param name="sessionId">Profile id to return</param>
    /// <returns>PmcData object</returns>
    public PmcData? GetPmcProfile(MongoId sessionId)
    {
        return GetFullProfile(sessionId).CharacterData?.PmcData;
    }

    /// <summary>
    ///     Is given user id a player
    /// </summary>
    /// <param name="userId">Id to validate</param>
    /// <returns>True is a player</returns>
    /// UNUSED?
    public bool IsPlayer(MongoId userId)
    {
        return saveServer.ProfileExists(userId);
    }

    /// <summary>
    ///     Get a full profiles scav-specific sub-profile
    /// </summary>
    /// <param name="sessionId">Profiles id</param>
    /// <returns>IPmcData object</returns>
    public PmcData? GetScavProfile(MongoId sessionId)
    {
        return saveServer.GetProfile(sessionId).CharacterData?.ScavData;
    }

    /// <summary>
    ///     Get baseline counter values for a fresh profile
    /// </summary>
    /// <returns>Default profile Stats object</returns>
    public Stats GetDefaultCounters()
    {
        return new Stats
        {
            Eft = new EftStats
            {
                CarriedQuestItems = [],
                DamageHistory = new DamageHistory
                {
                    LethalDamagePart = "Head",
                    LethalDamage = null,
                    BodyParts = new BodyPartsDamageHistory(),
                },
                DroppedItems = [],
                ExperienceBonusMult = 0,
                FoundInRaidItems = [],
                LastPlayerState = null,
                LastSessionDate = 0,
                OverallCounters = new OverallCounters { Items = [] },
                SessionCounters = new SessionCounters { Items = [] },
                SessionExperienceMult = 0,
                SurvivorClass = "Unknown",
                TotalInGameTime = 0,
                TotalSessionExperience = 0,
                Victims = [],
            },
        };
    }

    /// <summary>
    ///     is this profile flagged for data removal
    /// </summary>
    /// <param name="sessionId">Profile id</param>
    /// <returns>True if profile is to be wiped of data/progress</returns>
    /// TODO: logic doesn't feel right to have IsWiped being nullable
    protected bool IsWiped(MongoId sessionId)
    {
        return saveServer.GetProfile(sessionId).ProfileInfo?.IsWiped ?? false;
    }

    /// <summary>
    ///     Iterate over player profile inventory items and find the secure container and remove it
    /// </summary>
    /// <param name="profile">Profile to remove secure container from</param>
    /// <returns>profile without secure container</returns>
    public PmcData RemoveSecureContainer(PmcData profile)
    {
        var items = profile.Inventory?.Items;
        var secureContainer = items?.FirstOrDefault(i => i.SlotId == "SecuredContainer");
        if (secureContainer is not null)
        {
            // Find secure container + children
            var secureContainerAndChildrenIds = items?.GetItemWithChildrenTpls(secureContainer.Id).ToHashSet();

            // Remove secure container + its children
            items?.RemoveAll(x => (secureContainerAndChildrenIds?.Contains(x.Id) ?? false));
        }

        return profile;
    }

    /// <summary>
    ///     Flag a profile as having received a gift
    ///     Store giftId in profile spt object
    /// </summary>
    /// <param name="playerId">Player to add gift flag to</param>
    /// <param name="giftId">Gift player received</param>
    /// <param name="maxCount">Limit of how many of this gift a player can have</param>
    public void FlagGiftReceivedInProfile(MongoId playerId, string giftId, int maxCount)
    {
        var profileToUpdate = GetFullProfile(playerId);
        profileToUpdate.SptData!.ReceivedGifts ??= [];

        var giftData = profileToUpdate.SptData.ReceivedGifts.FirstOrDefault(g => g.GiftId == giftId);
        if (giftData != null)
        {
            // Increment counter
            giftData.Current++;
            return;
        }

        // Player has never received gift, make a new object
        profileToUpdate.SptData.ReceivedGifts.Add(
            new ReceivedGift
            {
                GiftId = giftId,
                TimestampLastAccepted = timeUtil.GetTimeStamp(),
                Current = 1,
            }
        );
    }

    /// <summary>
    ///     Check if profile has received a gift by id
    /// </summary>
    /// <param name="playerId">Player profile to check for gift</param>
    /// <param name="giftId">Gift to check for</param>
    /// <param name="maxGiftCount">Max times gift can be given to player</param>
    /// <returns>True if player has received gift previously</returns>
    public bool PlayerHasReceivedMaxNumberOfGift(MongoId playerId, string giftId, int maxGiftCount)
    {
        var profile = GetFullProfile(playerId);
        var giftDataFromProfile = profile.SptData?.ReceivedGifts?.FirstOrDefault(g => g.GiftId == giftId);
        if (giftDataFromProfile == null)
        {
            return false;
        }

        return giftDataFromProfile.Current >= maxGiftCount;
    }

    /// <summary>
    ///     Find Stat in profile counters and increment by one.
    /// </summary>
    /// <param name="counters">Counters to search for key</param>
    /// <param name="keyToIncrement">Key</param>
    /// Was Includes in Node so might not be exact?
    public void IncrementStatCounter(CounterKeyValue[] counters, string keyToIncrement)
    {
        var stat = counters.FirstOrDefault(c => c.Key != null && c.Key.Contains(keyToIncrement));
        if (stat != null)
        {
            stat.Value++;
        }
    }

    /// <summary>
    ///     Check if player has a skill at elite level
    /// </summary>
    /// <param name="skill">Skill to check</param>
    /// <param name="pmcProfile">Profile to find skill in</param>
    /// <returns>True if player has skill at elite level</returns>
    public bool HasEliteSkillLevel(SkillTypes skill, PmcData pmcProfile)
    {
        var profileSkills = pmcProfile.Skills?.Common;
        if (profileSkills == null)
        {
            return false;
        }

        var profileSkill = profileSkills.FirstOrDefault(s => s.Id == skill);
        if (profileSkill == null)
        {
            logger.Error(serverLocalisationService.GetText("quest-no_skill_found", skill));
            return false;
        }

        return profileSkill.Progress >= 5100; // 51
    }

    /// <summary>
    ///     Add points to a specific skill in player profile, adjusted for low levels by default
    /// </summary>
    /// <param name="pmcProfile">Player profile with skill</param>
    /// <param name="skill">Skill to add points to</param>
    /// <param name="pointsToAddToSkill">Points to add</param>
    /// <param name="useSkillProgressRateMultiplier">Skills are multiplied by a value in globals, default is off to maintain compatibility with legacy code</param>
    public void AddSkillPointsToPlayer(
        PmcData pmcProfile,
        SkillTypes skill,
        double pointsToAddToSkill,
        bool useSkillProgressRateMultiplier = false
    )
    {
        AddSkillPointsToPlayer(pmcProfile, skill, pointsToAddToSkill, useSkillProgressRateMultiplier, true);
    }

    /// <summary>
    ///     Add points to a specific skill in player profile
    /// </summary>
    /// <param name="pmcProfile">Player profile with skill</param>
    /// <param name="skill">Skill to add points to</param>
    /// <param name="pointsToAddToSkill">Points to add</param>
    /// <param name="useSkillProgressRateMultiplier">Skills are multiplied by a value in globals, default is off to maintain compatibility with legacy code</param>
    /// <param name="adjustSkillExpForLowLevels">Skills are multiplied by a multiplier for lower levels; if false, treats every level as requiring 100 points</param>
    public void AddSkillPointsToPlayer(
        PmcData pmcProfile,
        SkillTypes skill,
        double pointsToAddToSkill,
        bool useSkillProgressRateMultiplier = false,
        bool adjustSkillExpForLowLevels = true
    )
    {
        if (pointsToAddToSkill < 0D)
        {
            logger.Warning(serverLocalisationService.GetText("player-attempt_to_increment_skill_with_negative_value", skill));
            return;
        }

        var profileSkills = pmcProfile.Skills?.Common;
        if (profileSkills == null)
        {
            logger.Warning($"Unable to add: {pointsToAddToSkill} points to {skill}, Profile has no skills");
            return;
        }

        var profileSkill = profileSkills.FirstOrDefault(s => s.Id == skill);
        if (profileSkill == null)
        {
            logger.Error(serverLocalisationService.GetText("quest-no_skill_found", skill));
            return;
        }

        // already max level, no need to do any further calculations
        if (profileSkill.Progress >= 5100)
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Player already has max level in skill: {skill}, not adding points");
            }

            profileSkill.LastAccess = timeUtil.GetTimeStamp();
            return;
        }

        if (useSkillProgressRateMultiplier)
        {
            var skillProgressRate = globalTable.Configuration.SkillsSettings.SkillProgressRate;
            pointsToAddToSkill *= skillProgressRate;
        }

        if (inventoryConfig.SkillGainMultipliers.TryGetValue(skill.ToString(), out var multiplier))
        {
            pointsToAddToSkill *= multiplier;
        }

        var adjustedSkillProgress = adjustSkillExpForLowLevels
            ? AdjustSkillExpForLowLevels(profileSkill.Progress, pointsToAddToSkill)
            : pointsToAddToSkill;
        profileSkill.Progress += adjustedSkillProgress;
        profileSkill.Progress = Math.Min(profileSkill.Progress, 5100); // Prevent skill from ever going above level 51 (5100)

        profileSkill.PointsEarnedDuringSession += adjustedSkillProgress;

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Added: {adjustedSkillProgress} points to skill: {skill}, new progress value is: {profileSkill.Progress}");
        }

        profileSkill.LastAccess = timeUtil.GetTimeStamp();
    }

    /// <summary>
    ///     This method calculates the adjusted skill progression for lower levels.
    /// </summary>
    /// <param name="currentProgress">Current internal progress value of the skill, used to determine current level</param>
    /// <param name="visualProgressAmount">The amount of visual progress to add</param>
    /// <returns>Scaled skill progress according to level</returns>
    /// <remarks>
    ///     It expects to be passed on a value as expected per the visual progress on the UI.
    ///     It will return scaled internal progress according to the current skill level, to match Tarkovs skill progression curve.
    ///     So passing on "0.4" will always yield +0.4 progress on the UI for the player.
    /// </remarks>
    public double AdjustSkillExpForLowLevels(double currentProgress, double visualProgressAmount)
    {
        var level = Math.Floor(currentProgress / 100d);

        if (level >= 9)
        {
            return visualProgressAmount;
        }

        double internalAdded = 0;

        // See "CalculateExpOnFirstLevels" in client for original logic
        // loop until all visual progress has been used up
        while (visualProgressAmount > 0)
        {
            // scale to apply for levels 1-10, decreasing as level goes higher
            var uiMax = 10d * (level + 1d);
            var factor = 100d / uiMax;

            // remaining internal points in this level
            var inLevel = currentProgress % 100d;
            var internalRemaining = 100d - inLevel;

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"currentLevelRemainingProgress: {internalRemaining}");
            }

            // visual needed to fill the rest of this internal level
            var visualToLevelUp = internalRemaining / factor;

            var spendVisual = Math.Min(visualProgressAmount, visualToLevelUp);
            var addInternal = spendVisual * factor;

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Progress To Add Adjusted For Level: {addInternal}");
            }

            internalAdded += addInternal;
            currentProgress += addInternal;
            visualProgressAmount -= spendVisual;

            level = Math.Floor(currentProgress / 100d);
        }

        return internalAdded;
    }

    /// <summary>
    ///     Is the provided session id for a developer account
    /// </summary>
    /// <param name="sessionId">Profile id to check</param>
    /// <returns>True if account is developer</returns>
    public bool IsDeveloperAccount(MongoId sessionId)
    {
        return GetFullProfile(sessionId).ProfileInfo?.Edition?.ToLowerInvariant().StartsWith("spt developer") ?? false;
    }

    /// <summary>
    ///     Add stash row bonus to profile or increments rows given count if it already exists
    /// </summary>
    /// <param name="sessionId">Profile id to give rows to</param>
    /// <param name="rowsToAdd">How many rows to give profile</param>
    /// <returns>The stash rows bonus id, this is needed for ws notification if we send one</returns>
    public MongoId? AddStashRowsBonusToProfile(MongoId sessionId, int rowsToAdd)
    {
        var profile = GetPmcProfile(sessionId);
        if (profile?.Bonuses is null)
        {
            // Something is very wrong with profile to lack bonuses array, likely broken profile, exit early
            return null;
        }

        var existingBonus = profile.Bonuses.FirstOrDefault(b => b.Type == BonusType.StashRows);

        var bonusId = existingBonus?.Id;
        if (existingBonus is null)
        {
            bonusId = new MongoId();
            profile.Bonuses.Add(
                new Bonus
                {
                    Id = bonusId.Value,
                    Value = rowsToAdd,
                    Type = BonusType.StashRows,
                    IsPassive = true,
                    IsVisible = true,
                    IsProduction = false,
                }
            );
        }
        else
        {
            existingBonus.Value += rowsToAdd;
        }

        return bonusId!.Value;
    }

    public bool HasAccessToRepeatableFreeRefreshSystem(PmcData pmcProfile)
    {
        return GameEditionsWithFreeRefresh.Contains(pmcProfile.Info?.GameVersion ?? string.Empty);
    }

    /// <summary>
    ///     Find a profiles "Pockets" item and replace its tpl with passed in value
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="newPocketTpl">New tpl to set profiles Pockets to</param>
    public void ReplaceProfilePocketTpl(PmcData pmcProfile, string newPocketTpl)
    {
        // Find all pockets in profile, may be multiple as they could have equipment stand
        // (1 pocket for each upgrade level of equipment stand)
        var pockets = pmcProfile.Inventory?.Items?.Where(i => i.SlotId == "Pockets");
        if (pockets is null || !pockets.Any())
        {
            logger.Error($"Unable to replace profile: {pmcProfile.Id} pocket tpl with: {newPocketTpl} as Pocket item could not be found.");
            return;
        }

        foreach (var pocket in pockets)
        {
            pocket.Template = newPocketTpl;
        }
    }

    /// <summary>
    ///     Return a favorites list in the format expected by the GetOtherProfile call
    /// </summary>
    /// <param name="profile"></param>
    /// <returns>A list of Item objects representing the favorited data</returns>
    public List<Item> GetOtherProfileFavorites(PmcData profile)
    {
        var fullFavorites = new List<Item>();

        foreach (var itemId in profile.Inventory?.FavoriteItems ?? [])
        {
            // When viewing another users profile, the client expects a full item with children, so get that
            var itemAndChildren = profile.Inventory?.Items?.GetItemWithChildren(itemId);
            if (itemAndChildren?.Count > 0)
            {
                // To get the client to actually see the items, we set the main item's parent to null, so it's treated as a root item
                var clonedItems = cloner.Clone(itemAndChildren)!;
                clonedItems.First().ParentId = null;

                fullFavorites.AddRange(clonedItems);
            }
        }

        return fullFavorites;
    }

    public void AddHideoutCustomisationUnlock(SptProfile fullProfile, Reward reward, string source)
    {
        if (reward.Target is null)
        {
            logger.Error("Unable to add hideout customisation unlock, reward.Target is null.");
            return;
        }

        fullProfile.CustomisationUnlocks ??= [];

        if (fullProfile.CustomisationUnlocks?.Any(u => u.Id == reward.Target) ?? false)
        {
            logger.Warning(
                $"Profile: {fullProfile.ProfileInfo?.ProfileId ?? "`ProfileId is null`"} already has hideout customisation reward: {reward.Target}, skipping"
            );
            return;
        }

        var customisationTemplateDb = templateTable.Customization;

        if (!customisationTemplateDb.TryGetValue(reward.Target, out var template))
        {
            logger.Error("Unable to find customisation reward template");
            return;
        }

        var rewardToStore = new CustomisationStorage
        {
            Id = new MongoId(reward.Target),
            Source = source,
            Type = null,
        };

        switch (template.Parent)
        {
            case CustomisationTypeId.MANNEQUIN_POSE:
                rewardToStore.Type = CustomisationType.MANNEQUIN_POSE;
                break;
            case CustomisationTypeId.GESTURES:
                rewardToStore.Type = CustomisationType.GESTURE;
                break;
            case CustomisationTypeId.FLOOR:
                rewardToStore.Type = CustomisationType.FLOOR;
                break;
            case CustomisationTypeId.DOG_TAGS:
                rewardToStore.Type = CustomisationType.DOG_TAG;
                break;
            case CustomisationTypeId.CEILING:
                rewardToStore.Type = CustomisationType.CEILING;
                break;
            case CustomisationTypeId.WALL:
                rewardToStore.Type = CustomisationType.WALL;
                break;
            case CustomisationTypeId.ENVIRONMENT_UI:
                rewardToStore.Type = CustomisationType.ENVIRONMENT;
                break;
            case CustomisationTypeId.SHOOTING_RANGE_MARK:
                rewardToStore.Type = CustomisationType.SHOOTING_RANGE_MARK;
                break;
            case CustomisationTypeId.VOICE:
                rewardToStore.Type = CustomisationType.VOICE;
                break;
            case CustomisationTypeId.LIGHT:
                rewardToStore.Type = CustomisationType.LIGHT;
                break;
            case CustomisationTypeId.UPPER:
                rewardToStore.Type = CustomisationType.UPPER;
                break;
            case CustomisationTypeId.HEAD:
                rewardToStore.Type = CustomisationType.HEAD;
                break;
            default:
                logger.Error($"Unhandled customisation unlock type: {template.Parent} not added to profile");
                return;
        }

        fullProfile.CustomisationUnlocks?.Add(rewardToStore);
    }

    /// <summary>
    /// Get a profile template by the account and side
    /// </summary>
    /// <param name="accountEdition">Edition of profile desired, e.g. "Standard"</param>
    /// <param name="side">Side of profile desired, e.g. "Bear"</param>
    /// <returns></returns>
    public TemplateSide? GetProfileTemplateForSide(string accountEdition, string side)
    {
        var profileTemplates = templateTable.Profiles;

        // Get matching profile 'type' e.g. 'standard'
        if (!profileTemplates.TryGetValue(accountEdition, out var matchingProfileTemplate))
        {
            logger.Error($"Unable to find profile template for account edition: {accountEdition} and side: {side}");
            return null;
        }

        // Get matching profile by 'side' e.g. USEC
        return string.Equals(side, "bear", StringComparison.OrdinalIgnoreCase)
            ? matchingProfileTemplate.Bear
            : matchingProfileTemplate.Usec;
    }

    /// <summary>
    /// Look up a key inside the `CustomFlags` property from a profile template
    /// </summary>
    /// <param name="accountEdition">Edition of profile desired, e.g. "Standard"</param>
    /// <param name="flagKey">key stored in CustomFlags dictionary</param>
    /// <returns></returns>
    public bool GetProfileTemplateFlagValue(string accountEdition, string flagKey)
    {
        var profileTemplates = templateTable.Profiles;

        // Get matching profile 'type' e.g. 'standard'
        if (!profileTemplates.TryGetValue(accountEdition, out var matchingProfileTemplate))
        {
            logger.Error($"Unable to find profile template for account edition: {accountEdition}");
            return false;
        }

        return matchingProfileTemplate.CustomFlags.GetValueOrDefault(flagKey, false);
    }

    /// <summary>
    /// Gets the currencies in the profiles inventory
    /// </summary>
    /// <param name="profile">Full profile</param>
    /// <returns></returns>
    public ProfileCurrency GetAccountCurrency(SptProfile profile)
    {
        var returnObject = new ProfileCurrency
        {
            Roubles = 0,
            Euros = 0,
            Dollars = 0,
            GP = 0,
        };

        foreach (var item in profile.CharacterData?.PmcData?.Inventory?.Items ?? [])
        {
            var stackSize = item.GetItemStackSize();

            if (item.Template == Money.ROUBLES)
            {
                returnObject.Roubles += stackSize;
            }

            if (item.Template == Money.DOLLARS)
            {
                returnObject.Dollars += stackSize;
            }

            if (item.Template == Money.EUROS)
            {
                returnObject.Euros += stackSize;
            }

            if (item.Template == Money.GP)
            {
                returnObject.GP += stackSize;
            }
        }

        return returnObject;
    }

    /// <summary>
    /// Currently returns a template of data for the profiles stats
    /// </summary>
    /// <param name="profile">Full profile</param>
    /// <returns></returns>
    public ProfileStats GetProfileStats(SptProfile profile)
    {
        var pmcStats = ReadStats(profile.CharacterData?.PmcData);
        var scavStats = ReadStats(profile.CharacterData?.ScavData);
        var overallStats = CombineStats(pmcStats, scavStats);
        var accountLifetime = FormatOverallLifetime(profile.CharacterData?.PmcData?.Info?.RegistrationDate ?? 0);

        return new ProfileStats
        {
            Overall = FormatStats(overallStats, accountLifetime),
            Pmc = FormatStats(pmcStats, accountLifetime),
            Scav = FormatStats(scavStats, accountLifetime),
        };
    }

    private static CharacterStats ReadStats(PmcData? character)
    {
        var eft = character?.Stats?.Eft;
        var counters = eft?.OverallCounters?.Items;

        var transitCounter = GetStat(counters, "ExitStatus", "Transit");

        return new CharacterStats
        {
            Raids = GetStat(counters, "Sessions"),
            Survived = GetStat(counters, "ExitStatus", "Survived") + transitCounter,
            Killed = GetStat(counters, "ExitStatus", "Killed"),
            Deaths = GetStat(counters, "Deaths"),
            Kills = GetStat(counters, "Kills"),
            RunThrough = GetStat(counters, "ExitStatus", "Runner"),
            Awols = GetStat(counters, "ExitStatus", "Left"),
            Mias = GetStat(counters, "ExitStatus", "MissingInAction"),
            LongestWinStreak = GetStat(counters, "LongestWinStreak"),
            TotalInGameTime = eft?.TotalInGameTime ?? 0,
        };
    }

    private static CharacterStats CombineStats(CharacterStats pmcStats, CharacterStats scavStats)
    {
        return new CharacterStats
        {
            Raids = pmcStats.Raids + scavStats.Raids,
            Survived = pmcStats.Survived + scavStats.Survived,
            Killed = pmcStats.Killed + scavStats.Killed,
            Deaths = pmcStats.Deaths + scavStats.Deaths,
            Kills = pmcStats.Kills + scavStats.Kills,
            RunThrough = pmcStats.RunThrough + scavStats.RunThrough,
            Awols = pmcStats.Awols + scavStats.Awols,
            Mias = pmcStats.Mias + scavStats.Mias,
            LongestWinStreak = pmcStats.LongestWinStreak + scavStats.LongestWinStreak,
            TotalInGameTime = pmcStats.TotalInGameTime + scavStats.TotalInGameTime,
        };
    }

    private static Dictionary<string, string> FormatStats(CharacterStats stats, string accountLifetime)
    {
        var killDeathRatio =
            stats.Deaths > 0
                ? Math.Round(stats.Kills / (double)stats.Deaths, 2).ToString(CultureInfo.InvariantCulture)
                : stats.Kills.ToString(CultureInfo.InvariantCulture);
        var averageLifeSpan = stats.Raids > 0 ? FormatMinSeconds(stats.TotalInGameTime / stats.Raids) : "-";

        return new Dictionary<string, string>
        {
            { "Raids", stats.Raids.ToString(CultureInfo.InvariantCulture) },
            { "Survived", stats.Survived.ToString(CultureInfo.InvariantCulture) },
            { "KIA", stats.Killed.ToString(CultureInfo.InvariantCulture) },
            { "Kills", stats.Kills.ToString(CultureInfo.InvariantCulture) },
            { "RunThrough", stats.RunThrough.ToString(CultureInfo.InvariantCulture) },
            { "AWOL", stats.Awols.ToString(CultureInfo.InvariantCulture) },
            { "MIA", stats.Mias.ToString(CultureInfo.InvariantCulture) },
            { "KD", killDeathRatio },
            { "SurvivalRate", FormatPercentage(stats.Survived, stats.Raids) },
            { "AverageLifeSpan", averageLifeSpan },
            { "Online", FormatHoursMins(stats.TotalInGameTime) },
            { "LeaveRate", FormatPercentage(stats.Awols, stats.Raids) },
            { "SurvivalsInARow", stats.LongestWinStreak.ToString(CultureInfo.InvariantCulture) },
            { "AccountLifetime", accountLifetime },
        };
    }

    private static string FormatPercentage(long value, long total)
    {
        if (total <= 0)
        {
            return "-";
        }

        var percentage = (long)(value / (double)total * 100);
        return $"{percentage.ToString(CultureInfo.InvariantCulture)}%";
    }

    private static long GetStat(IEnumerable<CounterKeyValue>? counters, params string[] requiredTags)
    {
        var value = counters?.FirstOrDefault(counter => counter.Key?.IsSupersetOf(requiredTags) == true)?.Value;

        return value ?? 0;
    }

    /// <summary>
    /// Taken from the client with interpolation instead, returns a format of MM:SS
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private static string FormatMinSeconds(long seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
    }

    /// <summary>
    /// Taken from the client with interpolation instead, returns a format of HH:MM
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private static string FormatHoursMins(long seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}";
    }

    /// <summary>
    /// Taken from the client with interpolation instead, returns a format of - or HH:MM or DD or YYDD
    /// </summary>
    /// <param name="regDate"></param>
    /// <returns></returns>
    private static string FormatOverallLifetime(int regDate)
    {
        var localDateTimeFromUnixTime = DateTime.UnixEpoch.AddMilliseconds(regDate * 1000.0).ToUniversalTime();
        var ts = DateTime.UtcNow - localDateTimeFromUnixTime;

        if (ts <= TimeSpan.FromHours(48.0))
        {
            return $"{(int)ts.TotalHours}H{ts.Minutes:00}M";
        }

        if (ts <= TimeSpan.FromDays(365))
        {
            return $"{(int)ts.TotalDays}D";
        }

        var num = (int)(ts.TotalDays / 365);
        var num2 = (int)(ts.TotalDays - 365 * num);
        return $"{num}Y{num2}D";
    }
}

public record CharacterStats
{
    public long Raids { get; init; }
    public long Survived { get; init; }
    public long Killed { get; init; }
    public long Deaths { get; init; }
    public long Kills { get; init; }
    public long RunThrough { get; init; }
    public long Awols { get; init; }
    public long Mias { get; init; }
    public long LongestWinStreak { get; init; }
    public long TotalInGameTime { get; init; }
}
