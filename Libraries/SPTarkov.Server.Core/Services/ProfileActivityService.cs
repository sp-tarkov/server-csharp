using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileActivityService(TimeUtil timeUtil)
{
    private readonly ConcurrentDictionary<MongoId, ProfileActivityData> _activeProfiles = [];

    public void AddActiveProfile(MongoId sessionId, long clientStartedTimestamp)
    {
        _activeProfiles.AddOrUpdate(
            sessionId,
            // On add value
            key => new ProfileActivityData { ClientStartedTimestamp = clientStartedTimestamp, LastActive = timeUtil.GetTimeStamp() },
            // On Update value, client was started before but crashed or user restarted
            (key, existingValue) =>
            {
                existingValue.ClientStartedTimestamp = clientStartedTimestamp;
                existingValue.LastActive = timeUtil.GetTimeStamp();
                existingValue.RaidData = null;
                return existingValue;
            }
        );
    }

    public bool ContainsActiveProfile(MongoId sessionId)
    {
        if (_activeProfiles.ContainsKey(sessionId))
        {
            return true;
        }

        return false;
    }

    // Yes this is terrible, the other alternative is re-doing half of bot-gen which is currently doing guess-work anyway
    public ProfileActivityRaidData? GetFirstProfileActivityRaidData()
    {
        if (!_activeProfiles.IsEmpty)
        {
            return _activeProfiles.First().Value.RaidData;
        }

        return null;
    }

    public ProfileActivityRaidData GetProfileActivityRaidData(MongoId sessionId)
    {
        // Handle edge cases where people might close the server but keep the client alive
        if (!ContainsActiveProfile(sessionId))
        {
            AddActiveProfile(sessionId, timeUtil.GetTimeStamp());
        }

        if (_activeProfiles.TryGetValue(sessionId, out var currentActiveProfile))
        {
            currentActiveProfile.RaidData ??= new();

            return currentActiveProfile.RaidData;
        }

        throw new Exception($"Unable to retrieve active profile for session: {sessionId}");
    }

    /// <summary>
    ///     Was the requested profile active within the last x minutes
    /// </summary>
    /// <param name="sessionId"> Profile to check </param>
    /// <param name="minutes"> Minutes to check for activity in </param>
    /// <returns> True when profile was active within past x minutes </returns>
    public bool ActiveWithinLastMinutes(MongoId sessionId, int minutes)
    {
        if (!_activeProfiles.TryGetValue(sessionId, out var profileActivity))
        {
            // No record, exit early
            return false;
        }

        return timeUtil.GetTimeStamp() - profileActivity.LastActive < minutes * 60;
    }

    /// <summary>
    ///     Get a list of profile ids that were active in the last x minutes
    /// </summary>
    /// <param name="minutes"> How many minutes from now to search for profiles </param>
    /// <returns> List of active profile ids </returns>
    public List<string> GetActiveProfileIdsWithinMinutes(int minutes)
    {
        var currentTimestamp = timeUtil.GetTimeStamp();
        var result = new List<string>();

        foreach (var (sessionId, activeProfile) in _activeProfiles)
        {
            // Profile was active in last x minutes, add to return list
            if (currentTimestamp - activeProfile.LastActive < minutes * 60)
            {
                result.Add(sessionId);
            }
        }

        return result;
    }

    /// <summary>
    ///     Update the timestamp a profile was last observed active
    /// </summary>
    /// <param name="sessionId"> Profile to update </param>
    public void SetActivityTimestamp(MongoId sessionId)
    {
        if (_activeProfiles.TryGetValue(sessionId, out var currentActiveProfile))
        {
            currentActiveProfile.LastActive = timeUtil.GetTimeStamp();
        }
    }

    public IReadOnlyList<ProfileActiveClientMods> GetProfileActiveClientMods(MongoId sessionId)
    {
        if (!ContainsActiveProfile(sessionId))
        {
            return [];
        }

        if (_activeProfiles.TryGetValue(sessionId, out var currentActiveProfile))
        {
            return currentActiveProfile.ActiveClientMods;
        }

        throw new Exception($"Unable to retrieve active client mods for session: {sessionId}");
    }

    public void SetProfileActiveClientMods(MongoId sessionId, IReadOnlyList<ProfileActiveClientMods> activeClientMods)
    {
        if (!ContainsActiveProfile(sessionId))
        {
            AddActiveProfile(sessionId, timeUtil.GetTimeStamp());
        }

        if (_activeProfiles.TryGetValue(sessionId, out var currentActiveProfile))
        {
            currentActiveProfile.ActiveClientMods = activeClientMods;
        }
    }
}
