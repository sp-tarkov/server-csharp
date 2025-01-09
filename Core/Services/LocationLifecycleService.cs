using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;

namespace Core.Services;

public class LocationLifecycleService
{
    /** Handle client/match/local/start */
    public void StartLocalRaid(string sessionId, StartLocalRaidRequestData request)
    {
        throw new NotImplementedException();
    }

    /**
     * Replace map exits with scav exits when player is scavving
     * @param playerSide Players side (savage/usec/bear)
     * @param location id of map being loaded
     * @param locationData Maps location base data
     */
    protected void AdjustExtracts(string playerSide, string location, LocationBase locationData)
    {
        throw new NotImplementedException();
    }

    /**
     * Adjust the bot hostility values prior to entering a raid
     * @param location map to adjust values of
     */
    protected void AdjustBotHostilitySettings(LocationBase location)
    {
        throw new NotImplementedException();
    }

    /**
     * Generate a maps base location (cloned) and loot
     * @param name Map name
     * @param generateLoot OPTIONAL - Should loot be generated for the map before being returned
     * @returns LocationBase
     */
    protected LocationBase GenerateLocationAndLoot(string name, bool generateLoot = true)
    {
        throw new NotImplementedException();
    }

    /** Handle client/match/local/end */
    public void EndLocalRaid(string sessionId, EndLocalRaidRequestData request)
    {
        throw new NotImplementedException();
    }

    /**
     * Was extract by car
     * @param extractName name of extract
     * @returns True if extract was by car
     */
    protected bool ExtractWasViaCar(string extractName)
    {
        throw new NotImplementedException();
    }

    /**
     * Handle when a player extracts using a car - Add rep to fence
     * @param extractName name of the extract used
     * @param pmcData Player profile
     * @param sessionId Session id
     */
    protected void HandleCarExtract(string extractName, PmcData pmcData, string sessionId)
    {
        throw new NotImplementedException();
    }

    /**
     * Handle when a player extracts using a coop extract - add rep to fence
     * @param sessionId Session/player id
     * @param pmcData Profile
     * @param extractName Name of extract taken
     */
    protected void HandleCoopExtract(string sessionId, PmcData pmcData, string extractName)
    {
        throw new NotImplementedException();
    }

    /**
     * Get the fence rep gain from using a car or coop extract
     * @param pmcData Profile
     * @param baseGain amount gained for the first extract
     * @param extractCount Number of times extract was taken
     * @returns Fence standing after taking extract
     */
    protected int GetFenceStandingAfterExtract(PmcData pmcData, int baseGain, int extractCount)
    {
        throw new NotImplementedException();
    }

    /**
     * Did player take a COOP extract
     * @param extractName Name of extract player took
     * @returns True if coop extract
     */
    protected bool ExtractTakenWasCoop(string extractName)
    {
        throw new NotImplementedException();
    }

    protected void HandlePostRaidPlayerScav(
        string sessionId,
        PmcData pmcProfile,
        PmcData scavProfile,
        bool isDead,
        bool isTransfer,
        EndLocalRaidRequestData request)
    {
        throw new NotImplementedException();
    }

    /**
     *
     * @param sessionId Player id
     * @param pmcProfile Pmc profile
     * @param scavProfile Scav profile
     * @param isDead Player died/got left behind in raid
     * @param isSurvived Not same as opposite of `isDead`, specific status
     * @param request
     * @param locationName
     */
    protected void HandlePostRaidPmc(
        string sessionId,
        PmcData pmcProfile,
        PmcData scavProfile,
        bool isDead,
        bool isSurvived,
        bool isTransfer,
        EndLocalRaidRequestData request,
        string locationName)
    {
        throw new NotImplementedException();
    }

    protected void CheckForAndFixPickupQuestsAfterDeath(
        string sessionId,
        List<Item> lostQuestItems,
        List<QuestStatus> profileQuests
    )
    {
        throw new NotImplementedException();
    }

/*
 * In 0.15 Lightkeeper quests do not give rewards in PvE, this issue also occurs in spt
 * We check for newly completed Lk quests and run them through the servers `CompleteQuest` process
 * This rewards players with items + craft unlocks + new trader assorts
 */
    protected void LightkeeperQuestWorkaround(
        string sessionId,
        List<QuestStatus> postRaidQuests,
        List<QuestStatus> preRaidQuests,
        PmcData pmcProfile
    )
    {
        throw new NotImplementedException();
    }

/*
 * Convert post-raid quests into correct format
 * Quest status comes back as a string version of the enum `Success`, not the expected value of 1
 */
    protected List<QuestStatus> ProcessPostRaidQuests(List<QuestStatus> questsToProcess)
    {
        throw new NotImplementedException();
    }

/*
 * Adjust server trader settings if they differ from data sent by client
 */
    protected void ApplyTraderStandingAdjustments(
        Dictionary<string, TraderInfo> tradersServerProfile,
        Dictionary<string, TraderInfo> tradersClientProfile
    )
    {
        throw new NotImplementedException();
    }

/*
 * Check if player used BTR or transit item sending service and send items to player via mail if found
 */
    protected void HandleItemTransferEvent(string sessionId, EndLocalRaidRequestData request)
    {
        throw new NotImplementedException();
    }

    protected void TransferItemDelivery(string sessionId, string traderId, List<Item> items)
    {
        throw new NotImplementedException();
    }

    protected void HandleInsuredItemLostEvent(
        string sessionId,
        PmcData preRaidPmcProfile,
        EndLocalRaidRequestData request,
        string locationName
    )
    {
        throw new NotImplementedException();
    }

/*
 * Return the equipped items from a players inventory
 */
    protected List<Item> GetEquippedGear(List<Item> items)
    {
        throw new NotImplementedException();
    }

/*
 * Checks to see if player survives. run through will return false
 */
    protected bool IsPlayerSurvived(EndRaidResult results)
    {
        throw new NotImplementedException();
    }

/*
 * Is the player dead after a raid - dead = anything other than "survived" / "runner"
 */
    protected bool IsPlayerDead(EndRaidResult results)
    {
        throw new NotImplementedException();
    }

/*
 * Has the player moved from one map to another
 */
    protected bool IsMapToMapTransfer(EndRaidResult results)
    {
        throw new NotImplementedException();
    }

/*
 * Reset the skill points earned in a raid to 0, ready for next raid
 */
    protected void ResetSkillPointsEarnedDuringRaid(List<Common> commonSkills)
    {
        throw new NotImplementedException();
    }

/*
 * merge two dictionaries together
 * Prioritise pair that has true as a value
 */
    protected void MergePmcAndScavEncyclopedias(PmcData primary, PmcData secondary)
    {
        throw new NotImplementedException();
    }
}
