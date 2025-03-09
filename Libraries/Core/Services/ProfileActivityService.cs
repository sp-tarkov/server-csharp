using Core.Utils;
using SptCommon.Annotations;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileActivityService(
    TimeUtil _timeUtil
)
{
    private readonly Dictionary<string, long> profileActivityTimestamps = new();

    /// <summary>
    /// Was the requested profile active in the last requested minutes
    /// </summary>
    /// <param name="sessionId"> Profile to check </param>
    /// <param name="minutes"> Minutes to check for activity in </param>
    /// <returns> True when profile was active within past x minutes </returns>
    public bool ActiveWithinLastMinutes(string sessionId, int minutes)
    {
        var currentTimestamp = _timeUtil.GetTimeStamp();
        if (!profileActivityTimestamps.TryGetValue(sessionId, out var storedActivityTimestamp))
        {
            return false;
        }

        return currentTimestamp - storedActivityTimestamp < minutes * 60;
    }

    /// <summary>
    /// Get a list of profile ids that were active in the last x minutes
    /// </summary>
    /// <param name="minutes"> How many minutes from now to search for profiles </param>
    /// <returns> List of profile ids </returns>
    public List<string> GetActiveProfileIdsWithinMinutes(int minutes)
    {
        var currentTimestamp = _timeUtil.GetTimeStamp();
        var result = new List<string>();

        foreach (var activity in profileActivityTimestamps ?? new Dictionary<string, long>())
        {
            var lastActivityTimestamp = activity.Value;
            if (lastActivityTimestamp == null)
            {
                continue;
            }

            // Profile was active in last x minutes, add to return list
            if (currentTimestamp - lastActivityTimestamp < minutes * 60)
            {
                result.Add(activity.Key);
            }
        }

        return result;
    }

    /// <summary>
    /// Update the timestamp a profile was last observed active
    /// </summary>
    /// <param name="sessionId"> Profile to update </param>
    public void SetActivityTimestamp(string sessionId)
    {
        profileActivityTimestamps[sessionId] = _timeUtil.GetTimeStamp();
    }
}
