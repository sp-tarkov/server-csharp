using Microsoft.Extensions.Logging;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Services.InRaid;

/// <summary>
/// Handles rotating the goons between maps on a configurable interval
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class GoonLocationSpawnService(ISptLogger<GoonLocationSpawnService> logger, LocationTable locationTable, BotConfig botConfig)
    : IOnUpdate
{
    private int _lastAppliedSeed;
    private bool _hasApplied;

    /// <summary>
    /// Re-evaluate the goon spawn location whenever the rotation window changes
    /// </summary>
    public Task<bool> OnUpdateAsync(long secondsSinceLastRun, CancellationToken cancellationToken)
    {
        if (!botConfig.GoonSpawnSystem.Enabled)
        {
            return Task.FromResult(true);
        }

        // Goons already placed for the current rotation window, skip
        if (_hasApplied && GetRotationSeed() == _lastAppliedSeed)
        {
            return Task.FromResult(true);
        }

        AdjustGoonMapSpawns();

        return Task.FromResult(true);
    }

    /// <summary>
    /// Create a consistent seed for the current rotation window, we mirror generating a seed for this the same way we do the boss of the week
    /// </summary>
    /// <returns>Seed for the current rotation window</returns>
    private int GetRotationSeed()
    {
        var now = DateTime.UtcNow;

        // Number of hours to keep goons on the same map before rotating (min 1)
        var intervalHours = Math.Max(1, botConfig.GoonSpawnSystem.RotationIntervalHours);

        // Index of the current rotation window, incrementing every intervalHours
        var windowIndex = ((now.DayOfYear * 24) + now.Hour) / intervalHours;

        // Create consistent seed for the rotation window (use prime)
        return (now.Year * 1009) + windowIndex;
    }

    /// <summary>
    /// Goons will spawn on one map, rotating every GoonSpawnSystem.RotationIntervalHours hours,
    /// changing randomly based on a consistent seed made from the current utc time window
    /// </summary>
    /// <param name="locationBlacklist">LocationIds to always ignore when choosing a spawn</param>
    public void AdjustGoonMapSpawns(HashSet<string>? locationBlacklist = null)
    {
        locationBlacklist ??= ["hideout", "develop"];

        // Reset all maps with goons to 0% spawn, ignore blacklisted locations
        var allLocations = locationTable.GetDictionary();
        foreach (var (locationId, location) in allLocations)
        {
            if (!locationBlacklist.Contains(locationId) && location?.Base?.BossLocationSpawn is not null)
            {
                foreach (var goonSpawn in location.Base.BossLocationSpawn.Where(x => x.BossName == "bossKnight"))
                {
                    goonSpawn.BossChance = 0;
                }
            }
        }

        // Seed stays consistent for the whole rotation window
        var seed = GetRotationSeed();
        _lastAppliedSeed = seed;
        _hasApplied = true;

        // Init Random class with unique seed
        var random = new Random(seed);

        // Filter locations pool
        var validLocationIds = botConfig
            .GoonSpawnSystem.LocationPool.Where(locationId =>
                !locationBlacklist.Contains(locationId)
                && allLocations.TryGetValue(locationId, out var location)
                && location?.Base?.BossLocationSpawn is not null
            )
            .ToList();

        if (validLocationIds.Count == 0)
        {
            logger.Error("Unable to adjust goon spawn chance, no valid locations found");

            return;
        }

        // Choose a spawn location for goons
        var chosenMapId = validLocationIds[random.Next(0, validLocationIds.Count)];
        var chosenMap = allLocations[chosenMapId];

        // "Where" just incase there's multiple knight spawns for some reason
        var goonSpawns = chosenMap.Base.BossLocationSpawn.Where(x => x.BossName == "bossKnight");

        foreach (var goonSpawn in goonSpawns)
        {
            goonSpawn.BossChance = botConfig.GoonSpawnSystem.SpawnChance;
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug($"Goons are now on {chosenMapId}");
        }
    }
}
