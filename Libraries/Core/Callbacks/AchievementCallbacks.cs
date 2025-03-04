using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(AchievementCallbacks))]
public class AchievementCallbacks(
    AchievementController _achievementController,
    HttpResponseUtil _httpResponseUtil
)
{
    /// <summary>
    ///     Handle client/achievement/list
    /// </summary>
    /// <returns></returns>
    public string GetAchievements(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_achievementController.GetAchievements(sessionID));
    }

    /// <summary>
    ///     Handle client/achievement/statistic
    /// </summary>
    /// <returns></returns>
    public string Statistic(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.GetBody(_achievementController.GetAchievementStatics(sessionID));
    }
}
