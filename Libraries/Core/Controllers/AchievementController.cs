using Core.Models.Eft.Profile;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Controllers;

[Injectable]
public class AchievementController(
    DatabaseService _databaseService
)
{
    public virtual GetAchievementsResponse GetAchievements(string sessionID)
    {
        return new GetAchievementsResponse
        {
            Elements = _databaseService.GetAchievements()
        };
    }

    public virtual CompletedAchievementsResponse GetAchievementStatics(string sessionID)
    {
        var achievements = _databaseService.GetAchievements();
        var stats = new Dictionary<string, int>();

        foreach (var achievement in achievements)
        {
            if (achievement.Id != null)
            {
                stats.Add(achievement.Id, 0);
            }
        }

        return new CompletedAchievementsResponse
        {
            Elements = stats
        };
    }
}
