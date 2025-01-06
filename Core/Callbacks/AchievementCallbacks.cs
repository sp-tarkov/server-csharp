using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Profile;

namespace Core.Callbacks;

public class AchievementCallbacks
{
    public AchievementCallbacks()
    {
        
    }

    public GetBodyResponseData<GetAchievementsResponse> GetAchievements(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public GetBodyResponseData<CompletedAchievementsResponse> Statistic(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}