using Core.Context;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Game;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Location;
using Core.Models.Utils;
using Core.Servers;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RaidTimeAdjustmentService(
    ISptLogger<RaidTimeAdjustmentService> _logger,
    DatabaseService _databaseService,
    RandomUtil _randomUtil,
    WeightedRandomHelper _weightedRandomHelper,
    ApplicationContext _applicationContext,
    ConfigServer _configServer
)
{
    protected LocationConfig _locationConfig = _configServer.GetConfig<LocationConfig>();

    /// <summary>
    ///     Make alterations to the base map data passed in
    ///     Loot multipliers/waves/wave start times
    /// </summary>
    /// <param name="raidAdjustments">Changes to process on map</param>
    /// <param name="mapBase">Map to adjust</param>
    public void MakeAdjustmentsToMap(RaidChanges raidAdjustments, LocationBase mapBase)
    {
        _logger.Debug(
            $"Adjusting dynamic loot multipliers to {raidAdjustments.DynamicLootPercent}% and static loot multipliers to {raidAdjustments.StaticLootPercent}% of original"
        );

        // Change loot multiplier values before they're used below
        AdjustLootMultipliers(_locationConfig.LooseLootMultiplier, raidAdjustments.DynamicLootPercent);
        AdjustLootMultipliers(_locationConfig.StaticLootMultiplier, raidAdjustments.StaticLootPercent);

        var mapSettings = GetMapSettings(mapBase.Id);
        if (mapSettings.AdjustWaves)
            // Make alterations to bot spawn waves now player is simulated spawning later
        {
            AdjustWaves(mapBase, raidAdjustments);
        }
    }

    /// <summary>
    ///     Adjust the loot multiplier values passed in to be a % of their original value
    /// </summary>
    /// <param name="mapLootMultiplers">Multipliers to adjust</param>
    /// <param name="loosePercent">Percent to change values to</param>
    protected void AdjustLootMultipliers(Dictionary<string, double> mapLootMultiplers, double? loosePercent)
    {
        foreach (var location in mapLootMultiplers)
        {
            mapLootMultiplers[location.Key] = _randomUtil.GetPercentOfValue(mapLootMultiplers[location.Key], loosePercent ?? 1);
        }
    }

    /// <summary>
    ///     Adjust bot waves to act as if player spawned later
    /// </summary>
    /// <param name="mapBase">Map to adjust</param>
    /// <param name="raidAdjustments">Map adjustments</param>
    protected void AdjustWaves(LocationBase mapBase, RaidChanges raidAdjustments)
    {
        // Remove waves that spawned before the player joined
        var originalWaveCount = mapBase.Waves.Count;
        mapBase.Waves = mapBase.Waves.Where(x => x.TimeMax > raidAdjustments.SimulatedRaidStartSeconds).ToList();

        // Adjust wave min/max times to match new simulated start
        var startSeconds = raidAdjustments.SimulatedRaidStartSeconds.GetValueOrDefault(1);
        foreach (var wave in mapBase.Waves)
        {
            // Dont let time fall below 0
            wave.TimeMin -= (int) Math.Max(startSeconds, 0);
            wave.TimeMax -= (int) Math.Max(startSeconds, 0);
        }

        _logger.Debug(
            $"Removed {originalWaveCount - mapBase.Waves.Count} wave from map due to simulated raid start time of {raidAdjustments.SimulatedRaidStartSeconds / 60} minutes"
        );
    }

    /// <summary>
    ///     Create a randomised adjustment to the raid based on map data in location.json
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">Raid adjustment request</param>
    /// <returns>Response to send to client</returns>
    public GetRaidTimeResponse GetRaidAdjustments(string sessionId, GetRaidTimeRequest request)
    {
        var globals = _databaseService.GetGlobals();
        var mapBase = _databaseService.GetLocation(request.Location.ToLower()).Base;
        var baseEscapeTimeMinutes = mapBase.EscapeTimeLimit;

        // Prep result object to return
        var result = new GetRaidTimeResponse
        {
            RaidTimeMinutes = baseEscapeTimeMinutes,
            ExitChanges = [],
            NewSurviveTimeSeconds = null,
            OriginalSurvivalTimeSeconds = globals.Configuration.Exp.MatchEnd.SurvivedSecondsRequirement
        };

        // Pmc raid, send default
        if (request.Side.ToLower() == "pmc")
        {
            return result;
        }

        // We're scav adjust values
        var mapSettings = GetMapSettings(request.Location);

        // Chance of reducing raid time for scav, not guaranteed
        if (!_randomUtil.GetChance100(mapSettings.ReducedChancePercent))
            // Send default
        {
            return result;
        }

        // Get the weighted percent to reduce the raid time by
        var chosenRaidReductionPercent = int.Parse(
            _weightedRandomHelper.GetWeightedValue(mapSettings.ReductionPercentWeights)
        );
        var raidTimeRemainingPercent = 100 - chosenRaidReductionPercent;

        // How many minutes raid will last
        var newRaidTimeMinutes = Math.Floor(
            _randomUtil.ReduceValueByPercent(baseEscapeTimeMinutes ?? 1, chosenRaidReductionPercent)
        );

        // Time player spawns into the raid if it was online
        var simulatedRaidStartTimeMinutes = baseEscapeTimeMinutes - newRaidTimeMinutes;

        if (mapSettings.ReduceLootByPercent)
            // Store time reduction percent in app context so loot gen can pick it up later
        {
            _applicationContext.AddValue(
                ContextVariableType.RAID_ADJUSTMENTS,
                new RaidChanges
                {
                    DynamicLootPercent = Math.Max(raidTimeRemainingPercent, mapSettings.MinDynamicLootPercent),
                    StaticLootPercent = Math.Max(raidTimeRemainingPercent, mapSettings.MinStaticLootPercent),
                    SimulatedRaidStartSeconds = simulatedRaidStartTimeMinutes * 60
                }
            );
        }

        // Update result object with new time
        result.RaidTimeMinutes = newRaidTimeMinutes;

        _logger.Debug($"Reduced: {request.Location} raid time by: {chosenRaidReductionPercent}% to {newRaidTimeMinutes} minutes");

        // Calculate how long player needs to be in raid to get a `survived` extract status
        result.NewSurviveTimeSeconds = Math.Max(
            result.OriginalSurvivalTimeSeconds - (baseEscapeTimeMinutes - newRaidTimeMinutes) * 60 ?? 0,
            0D
        );

        var exitAdjustments = GetExitAdjustments(mapBase, newRaidTimeMinutes);
        if (exitAdjustments is not null)
        {
            result.ExitChanges.AddRange(exitAdjustments);
        }

        return result;
    }

    /// <summary>
    ///     Get raid start time settings for specific map
    /// </summary>
    /// <param name="location">Map Location e.g. bigmap</param>
    /// <returns>ScavRaidTimeLocationSettings</returns>
    protected ScavRaidTimeLocationSettings GetMapSettings(string location)
    {
        var mapSettings = _locationConfig.ScavRaidTimeSettings.Maps?[location.ToLower()];
        if (mapSettings is null)
        {
            _logger.Warning($"Unable to find scav raid time settings for map: {location}, using defaults");
            return new ScavRaidTimeLocationSettings();
        }

        return mapSettings;
    }

    /// <summary>
    ///     Adjust exit times to handle scavs entering raids part-way through
    /// </summary>
    /// <param name="mapBase">Map base file player is on</param>
    /// <param name="newRaidTimeMinutes">How long raid is in minutes</param>
    /// <returns>List of exit changes to send to client</returns>
    protected List<ExtractChange> GetExitAdjustments(LocationBase mapBase, double newRaidTimeMinutes)
    {
        List<ExtractChange> result = [];
        // Adjust train exits only
        foreach (var exit in mapBase.Exits)
        {
            if (exit.PassageRequirement != RequirementState.Train)
            {
                continue;
            }

            // Prepare train adjustment object
            var exitChange = new ExtractChange
            {
                Name = exit.Name,
                MinTime = null,
                MaxTime = null,
                Chance = null
            };

            // At what minute we simulate the player joining the raid
            var simulatedRaidEntryTimeMinutes = mapBase.EscapeTimeLimit - newRaidTimeMinutes;

            // How many seconds have elapsed in the raid when the player joins
            var reductionSeconds = simulatedRaidEntryTimeMinutes * 60;

            // Delay between the train extract activating and it becoming available to board
            //
            // Test method for determining this value:
            // 1) Set MinTime, MaxTime, and Count for the train extract all to 120
            // 2) Load into Reserve or Lighthouse as a PMC (both have the same result)
            // 3) Board the train when it arrives
            // 4) Check the raid time on the Raid Ended Screen (it should always be the same)
            //
            // trainArrivalDelaySeconds = [raid time on raid-ended screen] - MaxTime - Count - ExfiltrationTime
            // Example: Raid Time = 5:33 = 333 seconds
            //          trainArrivalDelaySeconds = 333 - 120 - 120 - 5 = 88
            //
            // I added 2 seconds just to be safe...
            //
            var trainArrivalDelaySeconds =
                _locationConfig.ScavRaidTimeSettings.Settings.TrainArrivalDelayObservedSeconds;

            // Determine the earliest possible time in the raid when the train would leave
            var earliestPossibleDepartureMinutes =
                (exit.MinTime + exit.Count + exit.ExfiltrationTime + trainArrivalDelaySeconds) / 60;

            // If raid is after last moment train can leave, assume train has already left, disable extract
            var mostPossibleTimeRemainingAfterDeparture = mapBase.EscapeTimeLimit - earliestPossibleDepartureMinutes;
            if (newRaidTimeMinutes < mostPossibleTimeRemainingAfterDeparture)
            {
                exitChange.Chance = 0;

                _logger.Debug(
                    $"Train Exit: {exit.Name} disabled as new raid time {newRaidTimeMinutes} minutes is below {mostPossibleTimeRemainingAfterDeparture} minutes"
                );

                result.Add(exitChange);

                continue;
            }

            // Reduce extract arrival times. Negative values seem to make extract turn red in game.
            exitChange.MinTime = Math.Max(exit.MinTime - reductionSeconds ?? 0, 0);
            exitChange.MaxTime = Math.Max(exit.MaxTime - reductionSeconds ?? 0, 0);

            _logger.Debug($"Train appears between: {exitChange.MinTime} and {exitChange.MaxTime} seconds raid time");

            result.Add(exitChange);
        }

        return result.Count > 0 ? result : null;
    }
}
