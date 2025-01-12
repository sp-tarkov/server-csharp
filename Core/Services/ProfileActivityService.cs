using Core.Annotations;
using Core.Utils;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class ProfileActivityService
{
    private TimeUtil _timeUtil;
    private Dictionary<string, long> profileActivityTimestamps = new();

    public ProfileActivityService(TimeUtil timeUtil)
    {
        _timeUtil = timeUtil;
    }

    /**
     * Was the requested profile active in the last requested minutes
     * @param sessionId Profile to check
     * @param minutes Minutes to check for activity in
     * @returns True when profile was active within past x minutes
     */
    public bool ActiveWithinLastMinutes(string sessionId, int minutes)
    {
        var currentTimestamp = _timeUtil.GetTimeStamp();
        var storedActivityTimestamp = profileActivityTimestamps[sessionId];

        if (storedActivityTimestamp != null)
            return false;

        return currentTimestamp - storedActivityTimestamp < minutes * 60;
    }

    /**
     * Get a list of profile ids that were active in the last x minutes
     * @param minutes How many minutes from now to search for profiles
     * @returns List of profile ids
     */
    public List<string> GetActiveProfileIdsWithinMinutes(int minutes)
    {
        var currentTimestamp = _timeUtil.GetTimeStamp();
        var result = new List<string>();

        foreach (var activity in profileActivityTimestamps ?? new())
        {
            var lastActivityTimestamp = activity.Value;
            if (lastActivityTimestamp == null)
                continue;
            
            // Profile was active in last x minutes, add to return list
            if (currentTimestamp - lastActivityTimestamp < minutes * 60)
                result.Add(activity.Key);
        }
        
        return result;
    }

    /**
     * Update the timestamp a profile was last observed active
     * @param sessionId Profile to update
     */
    public void SetActivityTimestamp(string sessionId)
    {
        profileActivityTimestamps[sessionId] = _timeUtil.GetTimeStamp();
    }
}
