using Core.Annotations;
using Core.Models.Eft.Profile;
using Core.Services;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class AchievementController
{
    protected ILogger _logger;
    protected DatabaseService _databaseService;
    
    public AchievementController
    (
        ILogger logger,
        DatabaseService databaseService
    )
    {
        _logger = logger;
        _databaseService = databaseService;
    }

    public GetAchievementsResponse GetAchievements(string sessionID)
    {
        return new GetAchievementsResponse
        {
            Achievements = _databaseService.GetAchievements()
        };
    }

    public CompletedAchievementsResponse GetAchievementStatics(string sessionID)
    {
        var achievements = _databaseService.GetAchievements();
        var stats = new Dictionary<string, int>();

        foreach (var achievement in achievements)
        {
            stats.Add(achievement.Id, 0);
        }

        return new()
        {
            Elements = stats
        };
    }
}
