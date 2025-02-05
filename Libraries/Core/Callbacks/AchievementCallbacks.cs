using SptCommon.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Utils;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(AchievementCallbacks))]
public class AchievementCallbacks(
    AchievementController _achievementController,
    HttpResponseUtil _httpResponseUtil
)
{
    /// <summary>
    /// Handle client/achievement/list
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetAchievements(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_achievementController.GetAchievements(sessionID));
    }

    /// <summary>
    /// Handle client/achievement/statistic
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string Statistic(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_achievementController.GetAchievementStatics(sessionID));
    }
}
