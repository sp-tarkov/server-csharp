using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Health;
using Core.Models.Eft.Profile;
using BodyPartHealth = Core.Models.Eft.Common.Tables.BodyPartHealth;
using Effects = Core.Models.Eft.Profile.Effects;
using Health = Core.Models.Eft.Profile.Health;

namespace Core.Helpers;

[Injectable]
public class HealthHelper
{
    /// <summary>
    /// Resets the profiles vitality/health and vitality/effects properties to their defaults
    /// </summary>
    /// <param name="sessionID">Session Id</param>
    /// <returns>Updated profile</returns>
    public SptProfile ResetVitality(string sessionID)
    {
        throw new NotImplementedException();
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
        Health postRaidHealth,
        string sessionID,
        bool isDead)
    {
        throw new NotImplementedException();
    }

    protected void StoreHydrationEnergyTempInProfile(
        SptProfile fullProfile,
        double hydration,
        double energy,
        double temprature)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take body part effects from client profile and apply to server profile
    /// </summary>
    /// <param name="postRaidBodyParts">Post-raid body part data</param>
    /// <param name="profileData">Player profile on server</param>
    protected void TransferPostRaidLimbEffectsToProfile(Dictionary<string, BodyPartHealth> postRaidBodyParts, PmcData profileData)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust hydration/energy/temperate and body part hp values in player profile to values in profile.vitality
    /// </summary>
    /// <param name="pmcData">Profile to update</param>
    /// <param name="sessionID">Session id</param>
    protected void SaveHealth(PmcData pmcData, string sessionID)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}
