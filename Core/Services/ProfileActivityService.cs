namespace Core.Services;

public class ProfileActivityService
{
    /**
     * Was the requested profile active in the last requested minutes
     * @param sessionId Profile to check
     * @param minutes Minutes to check for activity in
     * @returns True when profile was active within past x minutes
     */
    public bool ActiveWithinLastMinutes(string sessionId, int minutes)
    {
        throw new NotImplementedException();
    }

    /**
     * Get a list of profile ids that were active in the last x minutes
     * @param minutes How many minutes from now to search for profiles
     * @returns List of profile ids
     */
    public List<string> GetActiveProfileIdsWithinMinutes(int minutes)
    {
        throw new NotImplementedException();
    }

    /**
     * Update the timestamp a profile was last observed active
     * @param sessionId Profile to update
     */
    public void SetActivityTimestamp(string sessionId)
    {
        throw new NotImplementedException();
    }
}
