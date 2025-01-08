using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;
using Core.Models.Enums;

namespace Core.Helpers;

public class ProfileHelper
{
    /// <summary>
    /// Remove/reset a completed quest condtion from players profile quest data
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="questConditionId">Quest with condition to remove</param>
    public void RemoveQuestConditionFromProfile(PmcData pmcData, Dictionary<string, string> questConditionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all profiles from server
    /// </summary>
    /// <returns>Dictionary of profiles</returns>
    public Dictionary<string, SptProfile> GetProfiles()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the pmc and scav profiles as an array by profile id
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns>Array of PmcData objects</returns>
    public List<PmcData> GetCompleteProfile(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sanitize any information from the profile that the client does not expect to receive
    /// </summary>
    /// <param name="clonedProfile">A clone of the full player profile</param>
    protected void SanitizeProfileForClient(SptProfile clonedProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if a nickname is used by another profile loaded by the server
    /// </summary>
    /// <param name="nicknameRequest">nickname request object</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>True if already in use</returns>
    public bool IsNicknameTaken(ValidateNicknameRequestData nicknameRequest, string sessionID)
    {
        throw new NotImplementedException();
    }

    protected bool ProfileHasInfoProperty(SptProfile profile)
    {
        throw new NotImplementedException();
    }

    protected bool StringsMatch(string stringA, string stringB)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add experience to a PMC inside the players profile
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="experienceToAdd">Experience to add to PMC character</param>
    public void AddExperienceToPmc(string sessionID, int experienceToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate all profiles and find matching pmc profile by provided id
    /// </summary>
    /// <param name="pmcId">Profile id to find</param>
    /// <returns>PmcData</returns>
    public PmcData? GetProfileByPmcId(string pmcId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get experience value for given level
    /// </summary>
    /// <param name="level">Level to get xp for</param>
    /// <returns>Number of xp points for level</returns>
    public int GetExperience(int level)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the max level a player can be
    /// </summary>
    /// <returns>Max level</returns>
    public int GetMaxLevel()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get default Spt data object
    /// </summary>
    /// <returns>Spt</returns>
    public Spt GetDefaultSptDataObject()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get full representation of a players profile json
    /// </summary>
    /// <param name="sessionID">Profile id to get</param>
    /// <returns>SptProfile object</returns>
    public SptProfile? GetFullProfile(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get full representation of a players profile JSON by the account ID, or undefined if not found
    /// </summary>
    /// <param name="accountId">Account ID to find</param>
    /// <returns></returns>
    public SptProfile? GetFullProfileByAccountId(string accountID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve a ChatRoomMember formatted profile for the given session ID
    /// </summary>
    /// <param name="sessionID">The session ID to return the profile for</param>
    /// <returns></returns>
    public SearchFriendResponse? GetChatRoomMemberFromSessionId(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve a ChatRoomMember formatted profile for the given PMC profile data
    /// </summary>
    /// <param name="pmcProfile">The PMC profile data to format into a ChatRoomMember structure</param>
    /// <returns></returns>
    public SearchFriendResponse GetChatRoomMemberFromPmcProfile(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a PMC profile by its session id
    /// </summary>
    /// <param name="sessionID">Profile id to return</param>
    /// <returns>PmcData object</returns>
    public PmcData? GetPmcProfile(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is given user id a player
    /// </summary>
    /// <param name="userId">Id to validate</param>
    /// <returns>True is a player</returns>
    public bool IsPlayer(string userId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a full profiles scav-specific sub-profile
    /// </summary>
    /// <param name="sessionID">Profiles id</param>
    /// <returns>IPmcData object</returns>
    public PmcData GetScavProfile(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get baseline counter values for a fresh profile
    /// </summary>
    /// <returns>Default profile Stats object</returns>
    public Stats GetDefaultCounters()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// is this profile flagged for data removal
    /// </summary>
    /// <param name="sessionID">Profile id</param>
    /// <returns>True if profile is to be wiped of data/progress</returns>
    protected bool IsWiped(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over player profile inventory items and find the secure container and remove it
    /// </summary>
    /// <param name="profile">Profile to remove secure container from</param>
    /// <returns>profile without secure container</returns>
    public PmcData RemoveSecureContainer(PmcData profile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Flag a profile as having received a gift
    /// Store giftid in profile spt object
    /// </summary>
    /// <param name="playerId">Player to add gift flag to</param>
    /// <param name="giftId">Gift player received</param>
    /// <param name="maxCount">Limit of how many of this gift a player can have</param>
    public void FlagGiftReceivedInProfile(string playerId, string giftId, int maxCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if profile has recieved a gift by id
    /// </summary>
    /// <param name="playerId">Player profile to check for gift</param>
    /// <param name="giftId">Gift to check for</param>
    /// <param name="maxGiftCount">Max times gift can be given to player</param>
    /// <returns>True if player has recieved gift previously</returns>
    public bool PlayerHasRecievedMaxNumberOfGift(string playerId, string giftId, int maxGiftCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find Stat in profile counters and increment by one
    /// </summary>
    /// <param name="counters">Counters to search for key</param>
    /// <param name="keyToIncrement">Key</param>
    public void IncrementStatCounter(CounterKeyValue[] counters, string keyToIncrement)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if player has a skill at elite level
    /// </summary>
    /// <param name="skillType">Skill to check</param>
    /// <param name="pmcProfile">Profile to find skill in</param>
    /// <returns>True if player has skill at elite level</returns>
    public bool HasEliteSkillLevel(SkillTypes skillType, PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add points to a specific skill in player profile
    /// </summary>
    /// <param name="pmcProfile">Player profile with skill</param>
    /// <param name="skill">Skill to add points to</param>
    /// <param name="pointsToAdd">Points to add</param>
    /// <param name="useSkillProgressRateMultipler">Skills are multiplied by a value in globals, default is off to maintain compatibility with legacy code</param>
    public void AddSkillPointsToPlayer(PmcData pmcProfile, SkillTypes skill, int pointsToAdd, bool useSkillProgressRateMultipler = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a specific common skill from supplied profile
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="skill">Skill to look up and return value from</param>
    /// <returns>Common skill object from desired profile</returns>
    public Common GetSkillFromProfile(PmcData pmcData, SkillTypes skill)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided session id for a developer account
    /// </summary>
    /// <param name="sessionID">Profile id to check</param>
    /// <returns>True if account is developer</returns>
    public bool IsDeveloperAccount(string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add stash row bonus to profile or increments rows given count if it already exists
    /// </summary>
    /// <param name="sessionId">Profile id to give rows to</param>
    /// <param name="rowsToAdd">How many rows to give profile</param>
    public void AddStashRowsBonusToProfile(string sessionId, int rowsToAdd)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over all bonuses and sum up all bonuses of desired type in provided profile
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="desiredBonus">Bonus to sum up</param>
    /// <returns>Summed bonus value or 0 if no bonus found</returns>
    public int GetBonusValueFromProfile(PmcData pmcProfile, BonusType desiredBonus)
    {
        throw new NotImplementedException();
    }

    public bool PlayerIsFleaBanned(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add an achievement to player profile
    /// </summary>
    /// <param name="pmcProfile">Profile to add achievement to</param>
    /// <param name="achievementId">Id of achievement to add</param>
    public void AddAchievementToProfile(PmcData pmcProfile, string achievementId)
    {
        throw new NotImplementedException();
    }

    public bool HasAccessToRepeatableFreeRefreshSystem(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find a profiles "Pockets" item and replace its tpl with passed in value
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    /// <param name="newPocketTpl">New tpl to set profiles Pockets to</param>
    public void ReplaceProfilePocketTpl(PmcData pmcProfile, string newPocketTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return all quest items current in the supplied profile
    /// </summary>
    /// <param name="profile">Profile to get quest items from</param>
    /// <returns>List of item objects</returns>
    public List<Item> GetQuestItemsInProfile(PmcData profile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return a favorites list in the format expected by the GetOtherProfile call
    /// </summary>
    /// <param name="profile"></param>
    /// <returns>A list of Item objects representing the favorited data</returns>
    public List<Item> GetOtherProfileFavorites(PmcData profile)
    {
        throw new NotImplementedException();
    }
}
