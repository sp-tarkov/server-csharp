using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Health;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Extensions;
using BodyPartHealth = Core.Models.Eft.Common.Tables.BodyPartHealth;
using Effects = Core.Models.Eft.Profile.Effects;
using Health = Core.Models.Eft.Profile.Health;
using Vitality = Core.Models.Eft.Profile.Vitality;

namespace Core.Helpers;

[Injectable]
public class HealthHelper(
    ISptLogger<HealthHelper> _logger,
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    DatabaseService _databaseService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected HealthConfig _healthConfig = _configServer.GetConfig<HealthConfig>();
    
    /// <summary>
    /// Resets the profiles vitality/health and vitality/effects properties to their defaults
    /// </summary>
    /// <param name="sessionID">Session Id</param>
    /// <returns>Updated profile</returns>
    public SptProfile ResetVitality(string sessionID)
    {
        var profile = _saveServer.GetProfile(sessionID);

        profile.VitalityData ??= new Vitality { Health = null, Effects = null };
        
        profile.VitalityData.Health = new Health {
            Hydration = 0,
            Energy = 0,
            Temperature = 0,
            Head = 0,
            Chest = 0,
            Stomach = 0,
            LeftArm = 0,
            RightArm = 0,
            LeftLeg = 0,
            RightLeg = 0,
        };

        profile.VitalityData.Effects = new Effects {
            Head = new Head(),
            Chest = new Chest(),
            Stomach = new Stomach(),
            LeftArm = new LeftArm(),
            RightArm = new RightArm(),
            LeftLeg = new LeftLeg(),
            RightLeg = new RightLeg(),
        };

        return profile;
    }

    /// <summary>
    /// Update player profile vitality values with changes from client request object
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="postRaidHealth">Post raid data</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="isDead">Is player dead</param>
    /// <param name="addEffects">Should effects be added to profile (default - true)</param>
    /// <param name="deleteExistingEffects">Should all prior effects be removed before apply new ones  (default - true)</param>
    public void UpdateProfileHealthPostRaid(
        PmcData pmcData,
        BotBaseHealth postRaidHealth,
        string sessionID,
        bool isDead)
    {
        var fullProfile = _saveServer.GetProfile(sessionID);
        var profileEdition = fullProfile.ProfileInfo.Edition;
        var profileSide = fullProfile.CharacterData.PmcData.Info.Side;

        var defaultTemperature =
        _databaseService.GetProfiles()
            .GetByJsonProp<ProfileSides>(profileEdition)
            .GetByJsonProp<TemplateSide>(profileSide.ToLower())
            ?.Character?.Health?.Temperature ?? new CurrentMinMax { Current = 36.6 };

        StoreHydrationEnergyTempInProfile(
            fullProfile,
            postRaidHealth.Hydration.Current ?? 0,
            postRaidHealth.Energy.Current ?? 0,
            defaultTemperature.Current ?? 0 // Reset profile temp to the default to prevent very cold/hot temps persisting into next raid
        );

        // Store limb effects from post-raid in profile
        foreach (var bodyPart in postRaidHealth.BodyParts) {
            // Effects
            if (postRaidHealth.BodyParts[bodyPart.Key].Effects is not null) {
                // fullProfile.VitalityData.Effects[bodyPart.Key] = postRaidHealth.BodyParts[bodyPart.Key].Effects;
                // TODO: this will need to change, typing is all fucked up
            }

            // Limb hp
            if (!isDead)
            {
                // Player alive, not is limb alive
                var byJsonProp = fullProfile.VitalityData.Health.GetByJsonProp<double>(bodyPart.Key);
                byJsonProp = postRaidHealth.BodyParts[bodyPart.Key].Health.Current ?? 0;
            } else {
                var byJsonProp = fullProfile.VitalityData.Health.GetByJsonProp<double>(bodyPart.Key);
                byJsonProp = (pmcData.Health.BodyParts[bodyPart.Key].Health.Maximum * _healthConfig.HealthMultipliers.Death) ?? 0;
            }
        }

        TransferPostRaidLimbEffectsToProfile(postRaidHealth.BodyParts, pmcData);

        // Adjust hydration/energy/temp and limb hp using temp storage hydated above
        SaveHealth(pmcData, sessionID);

        // Reset temp storage
        ResetVitality(sessionID);

        // Update last edited timestamp
        pmcData.Health.UpdateTime = _timeUtil.GetTimeStamp();
    }

    protected void StoreHydrationEnergyTempInProfile(
        SptProfile fullProfile,
        double hydration,
        double energy,
        double temprature)
    {
        fullProfile.VitalityData.Health.Hydration = hydration;
        fullProfile.VitalityData.Health.Energy = energy;
        fullProfile.VitalityData.Health.Temperature = temprature;
    }

    /// <summary>
    /// Take body part effects from client profile and apply to server profile
    /// </summary>
    /// <param name="postRaidBodyParts">Post-raid body part data</param>
    /// <param name="profileData">Player profile on server</param>
    protected void TransferPostRaidLimbEffectsToProfile(Dictionary<string, BodyPartHealth> postRaidBodyParts, PmcData profileData)
    {
        // Iterate over each body part
        List<string> effectsToIgnore = ["Dehydration", "Exhaustion"];
        foreach (var bodyPartId in postRaidBodyParts) {
            // Get effects on body part from profile
            var bodyPartEffects = postRaidBodyParts[bodyPartId.Key].Effects;
            foreach (var effect in bodyPartEffects) {
                var effectDetails = bodyPartEffects[effect.Key];

                // Null guard
                profileData.Health.BodyParts[bodyPartId.Key].Effects ??= new Dictionary<string, BodyPartEffectProperties>();

                // Effect already exists on limb in server profile, skip
                var profileBodyPartEffects = profileData.Health.BodyParts[bodyPartId.Key].Effects;
                if (profileBodyPartEffects[effect.Key] is not null) {
                    if (effectsToIgnore.Contains(effect.Key)) {
                        // Get rid of certain effects we dont want to persist out of raid
                        profileBodyPartEffects[effect.Key] = null;
                    }

                    continue;
                }

                if (effectsToIgnore.Contains(effect.Key)) {
                    // Do not pass some effects to out of raid profile
                    continue;
                }

                // Add effect to server profile
                profileBodyPartEffects[effect.Key] = new BodyPartEffectProperties { Time = effectDetails.Time ?? -1 };
            }
        }
    }

    /// <summary>
    /// Update player profile vitality values with changes from client request object
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Heal request</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="addEffects">Should effects be added to profile (default - true)</param>
    /// <param name="deleteExistingEffects">Should all prior effects be removed before apply new ones  (default - true)</param>
    public void SaveVitality(
        PmcData pmcData,
        SyncHealthRequestData request,
        string sessionID,
        bool addEffects = true,
        bool deleteExistingEffects = true)
    {
        var postRaidBodyParts = request.Health; // post raid health settings
        var fullProfile = _saveServer.GetProfile(sessionID);
        var profileEffects = fullProfile.VitalityData.Effects;

        StoreHydrationEnergyTempInProfile(fullProfile, request.Hydration ?? 0, request.Energy ?? 0, request.Temperature ?? 0);

        // Process request data into profile
        foreach (var bodyPart in postRaidBodyParts) {
            // Transfer effects from request to profile
            if (bodyPart.Effects is not null) {
                // profileEffects[bodyPart] = postRaidBodyParts[bodyPart].Effects;
            }

            if (request.IsAlive ?? false) {
                // Player alive, not is limb alive
                // fullProfile.VitalityData.Health[bodyPart] = postRaidBodyParts[bodyPart].Current;
            } else {
                // fullProfile.VitalityData.Health[bodyPart] =
                //     pmcData.Health.BodyParts[bodyPart].Health.Maximum * _healthConfig.HealthMultipliers.Death;
            }// TODO: this will need to change, typing is all fucked up
        }

        // Add effects to body parts if enabled
        if (addEffects) {
            SaveEffects(
                pmcData,
                sessionID,
                _cloner.Clone(_saveServer.GetProfile(sessionID).VitalityData.Effects),
                deleteExistingEffects
            );
        }

        // Adjust hydration/energy/temp and limb hp
        SaveHealth(pmcData, sessionID);

        ResetVitality(sessionID);

        // Update last edited timestamp
        pmcData.Health.UpdateTime = _timeUtil.GetTimeStamp();
    }

    /// <summary>
    /// Adjust hydration/energy/temperate and body part hp values in player profile to values in profile.vitality
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="sessionID">Session id</param>
    protected void SaveHealth(PmcData pmcData, string sessionID)
    {
        // TODO: this will need to change, typing is all fucked up
        // if (!_healthConfig.Save.Health) {
        //     return;
        // }
        //
        // var profileHealth = _saveServer.GetProfile(sessionID).VitalityData.Health;
        // foreach (var healthModifier in profileHealth) {
        //     let target = profileHealth[healthModifier];
        //
        //     if (["Hydration", "Energy", "Temperature"].includes(healthModifier)) {
        //         // Set resources
        //         if (target > pmcData.Health[healthModifier].Maximum) {
        //             target = pmcData.Health[healthModifier].Maximum;
        //         }
        //
        //         pmcData.Health[healthModifier].Current = Math.round(target);
        //     } else {
        //         // Over max, limit
        //         if (target > pmcData.Health.BodyParts[healthModifier].Health.Maximum) {
        //             target = pmcData.Health.BodyParts[healthModifier].Health.Maximum;
        //         }
        //
        //         // Part was zeroed out in raid
        //         if (target === 0) {
        //             // Blacked body part
        //             target = Math.round(
        //                 pmcData.Health.BodyParts[healthModifier].Health.Maximum *
        //                 this.healthConfig.healthMultipliers.blacked,
        //             );
        //         }
        //
        //         pmcData.Health.BodyParts[healthModifier].Health.Current = Math.round(target);
        //     }
        // }
    }

    /// <summary>
    /// Save effects to profile
    /// Works by removing all effects and adding them back from profile
    /// Removes empty 'Effects' objects if found
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="bodyPartsWithEffects">Dictionary of body parts with effects that should be added to profile</param>
    /// <param name="deleteExistingEffects">Should effects be added back to profile</param>
    protected void SaveEffects(
        PmcData pmcData,
        string sessionID,
        Effects bodyPartsWithEffects,
        bool deleteExistingEffects = true)
    {
        // TODO: this will need to change, typing is all fucked up
        // if (!this.healthConfig.save.effects) {
        //     return;
        // }
        //
        // for (const bodyPart in bodyPartsWithEffects) {
        //     // clear effects from profile bodyPart
        //     if (deleteExistingEffects) {
        //         // biome-ignore lint/performance/noDelete: Delete is fine here as we entirely want to get rid of the effect.
        //         delete pmcData.Health.BodyParts[bodyPart].Effects;
        //     }
        //
        //     for (const effectType in bodyPartsWithEffects[bodyPart]) {
        //         if (typeof effectType !== "string") {
        //             this.logger.warning(`Effect ${effectType} on body part ${bodyPart} not a string, report this`);
        //         }
        //
        //         // // data can be index or the effect string (e.g. "Fracture") itself
        //         // const effect = /^-?\d+$/.test(effectValue) // is an int
        //         //     ? nodeEffects[bodyPart][effectValue]
        //         //     : effectValue;
        //         let time = bodyPartsWithEffects[bodyPart][effectType];
        //         if (time) {
        //             // Sometimes the value can be Infinity instead of -1, blame HealthListener.cs in modules
        //             if (time === "Infinity") {
        //                 this.logger.warning(
        //                     `Effect ${effectType} found with value of Infinity, changed to -1, this is an issue with HealthListener.cs`,
        //                     );
        //                 time = -1;
        //             }
        //             this.addEffect(pmcData, bodyPart, effectType, time);
        //         } else {
        //             this.addEffect(pmcData, bodyPart, effectType);
        //         }
        //     }
        // }
    }

    /// <summary>
    /// Add effect to body part in profile
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="effectBodyPart">Body part to edit</param>
    /// <param name="effectType">Effect to add to body part</param>
    /// <param name="duration">How long the effect has left in seconds (-1 by default, no duration).</param>
    protected void AddEffect(PmcData pmcData, string effectBodyPart, string effectType, int duration = -1)
    {
        // TODO: this will need to change, typing is all fucked up
        // const profileBodyPart = pmcData.Health.BodyParts[effectBodyPart];
        // if (!profileBodyPart.Effects) {
        //     profileBodyPart.Effects = {};
        // }
        //
        // profileBodyPart.Effects[effectType] = { Time: duration };
        //
        // // Delete empty property to prevent client bugs
        // if (this.isEmpty(profileBodyPart.Effects)) {
        //     // biome-ignore lint/performance/noDelete: Delete is fine here, we're removing an empty property to prevent game bugs.
        //     delete profileBodyPart.Effects;
        // }
    }
}
