using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

public class AchievementCallbacks
{
    public AchievementCallbacks()
    {
        
    }

    /// <summary>
    /// Handle client/achievement/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<GetAchievementsResponse> GetAchievements(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/achievement/statistic
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public GetBodyResponseData<CompletedAchievementsResponse> Statistic(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}