using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Profile;
using Core.Models.Enums;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileFixerService
{
    /// <summary>
    /// Find issues in the pmc profile data that may cause issues and fix them
    /// </summary>
    /// <param name="pmcProfile">profile to check and fix</param>
    public void CheckForAndFixPmcProfileIssues(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Resolve any dialogue attachments that were accidentally created using the player's equipment ID as
    /// the stash root object ID
    /// </summary>
    /// <param name="fullProfile"></param>
    public void CheckForAndFixDialogueAttachments(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find issues in the scav profile data that may cause issues
    /// </summary>
    /// <param name="scavProfile">profile to check and fix</param>
    public void CheckForAndFixScavProfileIssues(PmcData scavProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Attempt to fix common item issues that corrupt profiles
    /// </summary>
    /// <param name="pmcProfile">Profile to check items of</param>
    public void FixProfileBreakingInventoryItemIssues(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// TODO - make this non-public - currently used by RepeatableQuestController
    /// Remove unused condition counters
    /// </summary>
    /// <param name="pmcProfile">profile to remove old counters from</param>
    public void RemoveDanglingConditionCounters(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Repeatable quests leave behind TaskConditionCounter objects that make the profile bloat with time, remove them
    /// </summary>
    /// <param name="pmcProfile">Player profile to check</param>
    protected void RemoveDanglingTaskConditionCounters(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    protected List<RepeatableQuest> GetActiveRepeatableQuests(List<PmcDataRepeatableQuest> repeatableQuests)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// After removing mods that add quests, the quest panel will break without removing these
    /// </summary>
    /// <param name="pmcProfile">Profile to remove dead quests from</param>
    protected void RemoveOrphanedQuests(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Verify that all quest production unlocks have been applied to the PMC Profile
    /// </summary>
    /// <param name="pmcProfile">The profile to validate quest productions for</param>
    protected void VerifyQuestProductionUnlocks(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validate that the given profile has the given quest reward production scheme unlocked, and add it if not
    /// </summary>
    /// <param name="pmcProfile">Profile to check</param>
    /// <param name="productionUnlockReward">The quest reward to validate</param>
    /// <param name="questDetails">The quest the reward belongs to</param>
    protected void VerifyQuestProductionUnlock(PmcData pmcProfile, QuestReward productionUnlockReward, Quest questDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initial release of SPT 3.10 used an incorrect favorites structure, reformat
    /// the structure to the correct MongoID array structure
    /// </summary>
    /// <param name="pmcProfile"></param>
    protected void FixFavorites(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// If the profile has elite Hideout Managment skill, add the additional slots from globals
    /// NOTE: This seems redundant, but we will leave it here just in case.
    /// </summary>
    /// <param name="pmcProfile">profile to add slots to</param>
    protected void AddHideoutEliteSlots(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// add in objects equal to the number of slots
    /// </summary>
    /// <param name="areaType">area to check</param>
    /// <param name="emptyItemCount">area to update</param>
    /// <param name="pmcProfile">profile to update</param>
    protected void AddEmptyObjectsToHideoutAreaSlots(HideoutAreas areaType, int emptyItemCount, PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    protected IList<HideoutSlot> AddObjectsToList(int count, List<HideoutSlot> slots)
    {
        throw new NotImplementedException();
    }

    /**
     * Check for and cap profile skills at 5100.
     * @param pmcProfile profile to check and fix
     */
    protected void CheckForSkillsOverMaxLevel(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * Checks profile inventory for items that do not exist inside the items db
     * @param sessionId Session id
     * @param pmcProfile Profile to check inventory of
     */
    public void CheckForOrphanedModdedItems(string sessionId, SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * @param buildType The type of build, used for logging only
     * @param build The build to check for invalid items
     * @param itemsDb The items database to use for item lookup
     * @returns True if the build should be removed from the build list, false otherwise
     */
    protected bool ShouldRemoveWeaponEquipmentBuild(
        string buildType,
        WeaponBuild equipmentBuild,
        Dictionary<string, TemplateItem> itemsDb)
    {
        throw new NotImplementedException();
    }

    /**
     * @param magazineBuild The magazine build to check for validity
     * @param itemsDb The items database to use for item lookup
     * @returns True if the build should be removed from the build list, false otherwise
     */
    protected bool ShouldRemoveMagazineBuild(
        MagazineBuild magazineBuild,
        Dictionary<string, TemplateItem> itemsDb)
    {
        throw new NotImplementedException();
    }

    /**
     * REQUIRED for dev profiles
     * Iterate over players hideout areas and find what's built, look for missing bonuses those areas give and add them if missing
     * @param pmcProfile Profile to update
     */
    public void AddMissingHideoutBonusesToProfile(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /**
     * @param profileBonuses bonuses from profile
     * @param bonus bonus to find
     * @returns matching bonus
     */
    protected Bonus GetBonusFromProfile(List<Bonus> profileBonuses, StageBonus bonus)
    {
        throw new NotImplementedException();
    }

    public void CheckForAndRemoveInvalidTraders(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }
}
