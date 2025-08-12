using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using BodyPartHealth = SPTarkov.Server.Core.Models.Eft.Common.Tables.BodyPartHealth;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class HealthHelper(TimeUtil timeUtil, ConfigServer configServer)
{
    protected readonly HealthConfig HealthConfig = configServer.GetConfig<HealthConfig>();
    protected readonly HashSet<string> EffectsToSkip = ["Dehydration", "Exhaustion"];

    /// <summary>
    ///     Update player profile vitality values with changes from client request object
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcProfileToUpdate">Player profile to apply changes to</param>
    /// <param name="healthChanges">Changes to apply </param>
    public void ApplyHealthChangesToProfile(MongoId sessionId, PmcData pmcProfileToUpdate, BotBaseHealth healthChanges)
    {
        /* TODO: Not used here, need to check node or a live profile, commented out for now to avoid the potential alloc - Cj
        var fullProfile = saveServer.GetProfile(sessionId);
        var profileEdition = fullProfile.ProfileInfo?.Edition;
        var profileSide = fullProfile.CharacterData?.PmcData?.Info?.Side;
        // Get matching 'side' e.g. USEC
        var matchingSide = profileHelper.GetProfileTemplateForSide(profileEdition, profileSide);
        var defaultTemperature = matchingSide?.Character?.Health?.Temperature ?? new CurrentMinMax { Current = 36.6 };
        */

        if (healthChanges.BodyParts is null)
        {
            throw new HealthHelperException("healthChanges.BodyParts is null when trying to apply health changes");
        }

        // Alter saved profiles Health with values from post-raid client data
        ModifyProfileHealthProperties(pmcProfileToUpdate, healthChanges.BodyParts, EffectsToSkip);

        // Adjust hydration/energy/temperature
        AdjustProfileHydrationEnergyTemperature(pmcProfileToUpdate, healthChanges);

        if (pmcProfileToUpdate.Health is null)
        {
            throw new HealthHelperException("pmcProfileToUpdate.Health is null when trying to apply health changes");
        }

        // Update last edited timestamp
        pmcProfileToUpdate.Health.UpdateTime = timeUtil.GetTimeStamp();
    }

    /// <summary>
    ///     Apply Health values to profile
    /// </summary>
    /// <param name="profileToAdjust">Player profile on server</param>
    /// <param name="bodyPartChanges">Changes to apply</param>
    /// <param name="effectsToSkip"></param>
    protected void ModifyProfileHealthProperties(
        PmcData profileToAdjust,
        Dictionary<string, BodyPartHealth> bodyPartChanges,
        HashSet<string>? effectsToSkip = null
    )
    {
        foreach (var (partName, partProperties) in bodyPartChanges)
        {
            // Pattern matching null and false because otherwise the compiler throws a fit because `matchingProfilePart`
            // might not be initialized, very cool
            if (profileToAdjust.Health?.BodyParts?.TryGetValue(partName, out var matchingProfilePart) is null or false)
            {
                continue;
            }

            if (partProperties.Health is null || matchingProfilePart.Health is null)
            {
                throw new HealthHelperException(
                    "partProperties.Health or matchingBodyPart.Health is null when trying to modify profile health properties"
                );
            }

            if (HealthConfig.Save.Health)
            {
                // Apply hp changes to profile
                matchingProfilePart.Health.Current =
                    partProperties.Health.Current == 0
                        ? partProperties.Health.Maximum * HealthConfig.HealthMultipliers.Blacked
                        : partProperties.Health.Current;

                matchingProfilePart.Health.Maximum = partProperties.Health.Maximum;
            }

            // Process each effect for each part
            foreach (var (key, effectDetails) in partProperties.Effects ?? [])
            {
                // Null guard
                matchingProfilePart.Effects ??= new Dictionary<string, BodyPartEffectProperties?>();

                // Effect already exists on limb in server profile, skip
                if (matchingProfilePart.Effects.ContainsKey(key))
                {
                    // Edge case - effect already exists at destination, but we don't want to overwrite details
                    if (effectsToSkip is not null && effectsToSkip.Contains(key))
                    {
                        matchingProfilePart.Effects[key] = null;
                    }

                    continue;
                }

                if (effectsToSkip is not null && effectsToSkip.Contains(key))
                // Do not pass skipped effect into profile
                {
                    continue;
                }

                var effectToAdd = new BodyPartEffectProperties { Time = effectDetails?.Time ?? -1 };
                // Add effect to server profile
                if (matchingProfilePart.Effects.TryAdd(key, effectToAdd))
                {
                    matchingProfilePart.Effects[key] = effectToAdd;
                }
            }
        }
    }

    /// <summary>
    ///     Adjust hydration/energy/temperate
    /// </summary>
    /// <param name="profileToUpdate">Profile to update</param>
    /// <param name="healthChanges"></param>
    protected void AdjustProfileHydrationEnergyTemperature(PmcData profileToUpdate, BotBaseHealth healthChanges)
    {
        // Ensure current hydration/energy/temp are copied over and don't exceed maximum
        var profileHealth = profileToUpdate.Health;
        profileHealth.Hydration.Current =
            profileHealth.Hydration.Current > healthChanges.Hydration.Maximum
                ? healthChanges.Hydration.Maximum
                : Math.Round(healthChanges.Hydration.Current ?? 0);

        profileHealth.Energy.Current =
            profileHealth.Energy.Current > healthChanges.Energy.Maximum
                ? healthChanges.Energy.Maximum
                : Math.Round(healthChanges.Energy.Current ?? 0);

        profileHealth.Temperature.Current =
            profileHealth.Temperature.Current > healthChanges.Temperature.Maximum
                ? healthChanges.Temperature.Maximum
                : Math.Round(healthChanges.Temperature.Current ?? 0);
    }
}
