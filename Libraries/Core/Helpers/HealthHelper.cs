using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;
using SptCommon.Extensions;
using BodyPartHealth = Core.Models.Eft.Common.Tables.BodyPartHealth;
using Vitality = Core.Models.Eft.Profile.Vitality;

namespace Core.Helpers;

[Injectable]
public class HealthHelper(
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    DatabaseService _databaseService,
    ConfigServer _configServer
)
{
    protected HealthConfig _healthConfig = _configServer.GetConfig<HealthConfig>();

    /// <summary>
    ///     Resets the profiles vitality/health and vitality/effects properties to their defaults
    /// </summary>
    /// <param name="sessionID">Session Id</param>
    /// <returns>Updated profile</returns>
    public SptProfile ResetVitality(string sessionID)
    {
        var profile = _saveServer.GetProfile(sessionID);

        DefaultVitality(profile.VitalityData);

        return profile;
    }

    public void DefaultVitality(Vitality? vitality)
    {
        vitality ??= new Vitality
        {
            Health = null,
            Energy = 0,
            Temperature = 0,
            Hydration = 0
        };

        vitality.Health = new Dictionary<string, BodyPartHealth>
        {
            {
                "Head", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "Chest", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "Stomach", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "LeftArm", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "RightArm", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "LeftLeg", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "RightLeg", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            }
        };
    }

    /// <summary>
    ///     Update player profile vitality values with changes from client request object
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
                ?.Character?.Health?.Temperature ??
            new CurrentMinMax
            {
                Current = 36.6
            };

        StoreHydrationEnergyTempInProfile(
            fullProfile,
            postRaidHealth.Hydration.Current ?? 0,
            postRaidHealth.Energy.Current ?? 0,
            defaultTemperature.Current ?? 0 // Reset profile temp to the default to prevent very cold/hot temps persisting into next raid
        );

        // Store limb effects from post-raid in profile
        foreach (var bodyPart in postRaidHealth.BodyParts)
        {
            // Effects
            if (postRaidHealth.BodyParts[bodyPart.Key].Effects is not null)
            {
                fullProfile.VitalityData.Health[bodyPart.Key].Effects = postRaidHealth.BodyParts[bodyPart.Key].Effects;
            }

            // Limb hp
            if (!isDead)
                // Player alive, not is limb alive
            {
                fullProfile.VitalityData.Health[bodyPart.Key].Health.Current = postRaidHealth.BodyParts[bodyPart.Key].Health.Current ?? 0;
            }
            else
            {
                fullProfile.VitalityData.Health[bodyPart.Key].Health.Current =
                    pmcData.Health.BodyParts[bodyPart.Key].Health.Maximum * _healthConfig.HealthMultipliers.Death ?? 0;
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
        fullProfile.VitalityData.Hydration = hydration;
        fullProfile.VitalityData.Energy = energy;
        fullProfile.VitalityData.Temperature = temprature;
    }

    /// <summary>
    ///     Take body part effects from client profile and apply to server profile
    /// </summary>
    /// <param name="postRaidBodyParts">Post-raid body part data</param>
    /// <param name="profileData">Player profile on server</param>
    protected void TransferPostRaidLimbEffectsToProfile(Dictionary<string, BodyPartHealth> postRaidBodyParts, PmcData profileData)
    {
        // Iterate over each body part
        List<string> effectsToIgnore = ["Dehydration", "Exhaustion"];
        foreach (var bodyPartId in postRaidBodyParts)
        {
            // Get effects on body part from profile
            var bodyPartEffects = postRaidBodyParts[bodyPartId.Key].Effects;
            foreach (var effect in bodyPartEffects)
            {
                var effectDetails = bodyPartEffects[effect.Key];

                // Null guard
                profileData.Health.BodyParts[bodyPartId.Key].Effects ??= new Dictionary<string, BodyPartEffectProperties>();

                // Effect already exists on limb in server profile, skip
                var profileBodyPartEffects = profileData.Health.BodyParts[bodyPartId.Key].Effects;
                if (profileBodyPartEffects.TryGetValue(effect.Key, out var dictEffect))
                {
                    if (effectsToIgnore.Contains(effect.Key))
                        // Get rid of certain effects we dont want to persist out of raid
                    {
                        dictEffect = null;
                    }

                    continue;
                }

                if (effectsToIgnore.Contains(effect.Key))
                    // Do not pass some effects to out of raid profile
                {
                    continue;
                }

                var effectToAdd = new BodyPartEffectProperties
                {
                    Time = effectDetails.Time ?? -1
                };
                // Add effect to server profile
                if (profileBodyPartEffects.TryAdd(effect.Key, effectToAdd))
                {
                    profileBodyPartEffects[effect.Key] = effectToAdd;
                }
            }
        }
    }

    /// <summary>
    ///     Adjust hydration/energy/temperate and body part hp values in player profile to values in profile.vitality
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="sessionID">Session id</param>
    protected void SaveHealth(PmcData pmcData, string sessionID)
    {
        if (!_healthConfig.Save.Health)
        {
            return;
        }

        var profileHealth = _saveServer.GetProfile(sessionID).VitalityData;

        if (profileHealth.Hydration > pmcData.Health.Hydration.Maximum)
        {
            profileHealth.Hydration = pmcData.Health.Hydration.Maximum;
        }

        if (profileHealth.Energy > pmcData.Health.Energy.Maximum)
        {
            profileHealth.Energy = pmcData.Health.Energy.Maximum;
        }

        if (profileHealth.Temperature > pmcData.Health.Temperature.Maximum)
        {
            profileHealth.Temperature = pmcData.Health.Temperature.Maximum;
        }

        pmcData.Health.Hydration.Current = Math.Round(profileHealth.Hydration ?? 0);
        pmcData.Health.Energy.Current = Math.Round(profileHealth.Energy ?? 0);
        pmcData.Health.Temperature.Current = Math.Round(profileHealth.Temperature ?? 0);

        foreach (var bodyPart in pmcData.Health.BodyParts)
        {
            if (profileHealth.Health[bodyPart.Key].Health.Maximum > bodyPart.Value.Health.Maximum)
            {
                profileHealth.Health[bodyPart.Key].Health.Maximum = bodyPart.Value.Health.Maximum;
            }

            if (profileHealth.Health[bodyPart.Key].Health.Current == 0)
            {
                profileHealth.Health[bodyPart.Key].Health.Current = bodyPart.Value.Health.Maximum * _healthConfig.HealthMultipliers.Blacked;
            }

            bodyPart.Value.Health.Current = Math.Round(profileHealth.Health[bodyPart.Key].Health.Current ?? 0);
        }
    }

    /// <summary>
    ///     Save effects to profile
    ///     Works by removing all effects and adding them back from profile
    ///     Removes empty 'Effects' objects if found
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="sessionID">Session id</param>
    /// <param name="bodyPartsWithEffects">Dictionary of body parts with effects that should be added to profile</param>
    /// <param name="deleteExistingEffects">Should effects be added back to profile</param>
    protected void SaveEffects(
        PmcData pmcData,
        string sessionID,
        Dictionary<string, BodyPartHealth> bodyPartsWithEffects,
        bool deleteExistingEffects = true)
    {
        // TODO: this will need to change, typing is all fucked up
        if (!_healthConfig.Save.Effects)
        {
            return;
        }

        foreach (var bodyPart in bodyPartsWithEffects)
        {
            // clear effects from profile bodyPart
            if (deleteExistingEffects)
            {
                pmcData.Health.BodyParts[bodyPart.Key].Effects = new Dictionary<string, BodyPartEffectProperties>();
            }

            foreach (var effectType in bodyPartsWithEffects[bodyPart.Key].Effects)
            {
                var time = effectType.Value.Time;
                if (time is not null && time > 0)
                {
                    AddEffect(pmcData, effectType, time);
                }
                else
                {
                    AddEffect(pmcData, effectType);
                }
            }
        }
    }

    /// <summary>
    ///     Add effect to body part in profile
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="effectBodyPart">Body part to edit</param>
    /// <param name="effectType">Effect to add to body part</param>
    /// <param name="duration">How long the effect has left in seconds (-1 by default, no duration).</param>
    protected void AddEffect(PmcData pmcData, KeyValuePair<string, BodyPartEffectProperties> effectType, double? duration = -1)
    {
        var profileBodyPart = pmcData.Health.BodyParts[effectType.Key];
        profileBodyPart.Effects ??= new Dictionary<string, BodyPartEffectProperties>();

        profileBodyPart.Effects[effectType.Key] = new BodyPartEffectProperties
        {
            Time = duration
        };
    }
}
