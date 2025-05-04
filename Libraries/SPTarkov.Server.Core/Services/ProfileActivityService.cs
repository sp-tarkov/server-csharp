using System.Collections.Concurrent;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileActivityService(TimeUtil timeUtil)
{
    private readonly ConcurrentDictionary<string, long> _profileActivityTimestamps = new();

    /// <summary>
    ///     Was the requested profile active within the last x minutes
    /// </summary>
    /// <param name="sessionId"> Profile to check </param>
    /// <param name="minutes"> Minutes to check for activity in </param>
    /// <returns> True when profile was active within past x minutes </returns>
    public bool ActiveWithinLastMinutes(string sessionId, int minutes)
    {
        if (!_profileActivityTimestamps.TryGetValue(sessionId, out var storedActivityTimestamp))
        {
            // No record, exit early
            return false;
        }

        return timeUtil.GetTimeStamp() - storedActivityTimestamp < minutes * 60;
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

        foreach (var (sessionId, lastActivityTimestamp) in _profileActivityTimestamps)
        {
            // Profile was active in last x minutes, add to return list
            if (currentTimestamp - lastActivityTimestamp < minutes * 60)
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
    public void SetActivityTimestamp(string sessionId)
    {
        if (!_profileActivityTimestamps.TryAdd(sessionId, timeUtil.GetTimeStamp()))
        {
            _profileActivityTimestamps[sessionId] = timeUtil.GetTimeStamp();
        }
    }
}
