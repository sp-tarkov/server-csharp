using Core.Models.Eft.Common;
using Core.Models.Eft.Game;
using Core.Models.Spt.Config;
using Core.Models.Spt.Location;

namespace Core.Services;

public class RaidTimeAdjustmentService
{
    /// <summary>
    /// Make alterations to the base map data passed in
    /// Loot multipliers/waves/wave start times
    /// </summary>
    /// <param name="raidAdjustments">Changes to process on map</param>
    /// <param name="mapBase">Map to adjust</param>
    public void MakeAdjustmentsToMap(RaidChanges raidAdjustments, LocationBase mapBase)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust the loot multiplier values passed in to be a % of their original value
    /// </summary>
    /// <param name="mapLootMultipliers">Multipliers to adjust</param>
    /// <param name="loosePercent">Percent to change values to</param>
    protected void AdjustLootMultipliers(LootMultiplier mapLootMultipliers, float loosePercent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust bot waves to act as if player spawned later
    /// </summary>
    /// <param name="mapBase">Map to adjust</param>
    /// <param name="raidAdjustments">Map adjustments</param>
    protected void AdjustWaves(LocationBase mapBase, RaidChanges raidAdjustments)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a randomised adjustment to the raid based on map data in location.json
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">Raid adjustment request</param>
    /// <returns>Response to send to client</returns>
    public GetRaidTimeResponse GetRaidAdjustments(string sessionId, GetRaidTimeRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get raid start time settings for specific map
    /// </summary>
    /// <param name="location">Map Location e.g. bigmap</param>
    /// <returns>ScavRaidTimeLocationSettings</returns>
    protected ScavRaidTimeLocationSettings GetMapSettings(string location)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust exit times to handle scavs entering raids part-way through
    /// </summary>
    /// <param name="mapBase">Map base file player is on</param>
    /// <param name="newRaidTimeMinutes">How long raid is in minutes</param>
    /// <returns>List of exit changes to send to client</returns>
    protected List<ExtractChange> GetExitAdjustments(LocationBase mapBase, int newRaidTimeMinutes)
    {
        throw new NotImplementedException();
    }
}
