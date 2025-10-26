using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Exceptions.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using BodyPartHealth = SPTarkov.Server.Core.Models.Eft.Common.Tables.BodyPartHealth;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class HealthHelper(ISptLogger<HealthHelper> logger, TimeUtil timeUtil, ConfigServer configServer)
{
    protected readonly HealthConfig HealthConfig = configServer.GetConfig<HealthConfig>();
    protected readonly HashSet<string> EffectsToSkip = ["Dehydration", "Exhaustion"];

    /// <summary>
    ///     Update player profile vitality values with changes from client request object
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcProfileToUpdate">Player profile to apply changes to</param>
    /// <param name="healthChanges">Changes to apply </param>
    public void ApplyHealthChangesToProfile(MongoId sessionId, PmcData pmcProfileToUpdate, BotBaseHealth healthChanges, bool isDead)
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
            const string message = "healthChanges.BodyParts is null when trying to apply health changes";
            logger.Error(message);
            throw new HealthHelperException(message);
        }

        var playerWasCursed = !PlayerHadGearOnRaidStart(pmcProfileToUpdate.Inventory);

        // Alter saved profiles Health with values from post-raid client data
        ModifyProfileHealthProperties(pmcProfileToUpdate, healthChanges.BodyParts, EffectsToSkip, isDead, playerWasCursed);

        // Adjust hydration/energy/temperature
        AdjustProfileHydrationEnergyTemperature(pmcProfileToUpdate, healthChanges);

        if (pmcProfileToUpdate.Health is null)
        {
            const string message = "pmcProfileToUpdate.Health is null when trying to apply health changes";
            logger.Error(message);
            throw new HealthHelperException(message);
        }

        // Update last edited timestamp
        pmcProfileToUpdate.Health.UpdateTime = timeUtil.GetTimeStamp();
    }

    /// <summary>
    /// Did the player start raid with gear, if false, they are 'cursed'
    /// </summary>
    /// <param name="inventory">Players inventory at start of raid</param>
    /// <returns>True = they had enough gear to not be classed as 'cursed'</returns>
    protected bool PlayerHadGearOnRaidStart(BotBaseInventory inventory)
    {
        if (inventory?.Items == null)
        {
            return false;
        }

        var hasWeapon = false;
        var hasVestRigOrBackpack = false;
        foreach (var item in inventory.Items)
        {
            // Possible early escape
            if (hasWeapon && hasVestRigOrBackpack)
            {
                return true;
            }

            if (item.SlotId is "FirstPrimaryWeapon" or "SecondPrimaryWeapon" or "Holster")
            {
                hasWeapon = true;
                continue;
            }

            if (item.SlotId is "Backpack" or "ArmorVest" or "TacticalVest")
            {
                hasVestRigOrBackpack = true;
            }
        }

        return hasWeapon && hasVestRigOrBackpack;
    }

    /// <summary>
    ///     Apply Health values to profile
    /// </summary>
    /// <param name="profileToAdjust">Player profile on server</param>
    /// <param name="bodyPartChanges">Changes to apply</param>
    /// <param name="effectsToSkip"></param>
    /// <param name="isDead"></param>
    /// <param name="playerWasCursed">Did player enter raid with no equipment</param>
    protected void ModifyProfileHealthProperties(
        PmcData profileToAdjust,
        Dictionary<string, BodyPartHealth> bodyPartChanges,
        HashSet<string>? effectsToSkip = null,
        bool isDead = false,
        bool playerWasCursed = false
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
                const string message =
                    "partProperties.Health or matchingBodyPart.Health is null when trying to modify profile health properties";
                logger.Error(message);
                throw new HealthHelperException(message);
            }

            if (HealthConfig.Save.Health)
            {
                // Apply hp changes to profile
                if (!isDead)
                {
                    // If the player isn't dead, restore blacked limbs with a penalty
                    matchingProfilePart.Health.Current =
                        partProperties.Health.Current == 0
                            ? matchingProfilePart.Health.Maximum * HealthConfig.HealthMultipliers.Blacked
                            : partProperties.Health.Current;
                }
                else
                {
                    // If the player died, set all limbs with a penalty
                    matchingProfilePart.Health.Current = matchingProfilePart.Health.Maximum * HealthConfig.HealthMultipliers.Death;

                    // Cursed player, body part gets set to 1 on death
                    if (playerWasCursed)
                    {
                        matchingProfilePart.Health.Current = 1;
                    }
                }
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
