using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;
using Core.Utils;

namespace Core.Callbacks;

public class AchievementCallbacks
{
    protected AchievementController _achievementController;
    protected ProfileController _profileController;
    protected HttpResponseUtil _httpResponseUtil;
    
    public AchievementCallbacks
    (
        AchievementController achievementController,
        ProfileController profileController,
        HttpResponseUtil httpResponseUtil
    )
    {
        _achievementController = achievementController;
        _profileController = profileController;
        _httpResponseUtil = httpResponseUtil;
    }

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
