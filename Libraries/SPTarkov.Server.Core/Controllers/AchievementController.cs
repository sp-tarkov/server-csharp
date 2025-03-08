using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class AchievementController(
    ProfileHelper profileHelper,
    DatabaseService databaseService,
    ConfigServer configServer
)
{
    protected CoreConfig coreConfig = configServer.GetConfig<CoreConfig>();

    /// <summary>
    /// Get base achievements
    /// </summary>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public virtual GetAchievementsResponse GetAchievements(string sessionID)
    {
        return new GetAchievementsResponse
        {
            Elements = databaseService.GetAchievements()
        };
    }

    /// <summary>
    /// Shows % of 'other' players who've completed each achievement
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>CompletedAchievementsResponse</returns>
    public virtual CompletedAchievementsResponse GetAchievementStatics(string sessionId)
    {
        var stats = new Dictionary<string, int>();
        var profiles = profileHelper.GetProfiles();

        var achievements = databaseService.GetAchievements();
        foreach (var achievement in achievements) {
            var percentage = 0;
            foreach (var (profileId, profile) in profiles) {
                if (coreConfig.Features.AchievementProfileIdBlacklist.Contains(profileId))
                {
                    continue;
                }

                if (profile.CharacterData?.PmcData?.Achievements is null)
                {
                    continue;
                }

                if (!profile.CharacterData.PmcData.Achievements.ContainsKey(achievement.Id))
                {
                    continue;
                }

                percentage++;
            }

            percentage = (percentage / profiles.Count) * 100;
            stats.Add(achievement.Id, percentage);
        }

        return new CompletedAchievementsResponse{ Elements = stats };
    }
}
