using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

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
    ///     Get base achievements
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
    ///     Shows % of 'other' players who've completed each achievement
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>CompletedAchievementsResponse</returns>
    public virtual CompletedAchievementsResponse GetAchievementStatics(string sessionId)
    {
        var stats = new Dictionary<string, int>();
        var profiles = profileHelper.GetProfiles();

        var achievements = databaseService.GetAchievements();
        foreach (var achievementId in achievements.Select(achievement =>
        {
            return achievement.Id;
        }).Where(achievementId =>
{
    return !string.IsNullOrEmpty(achievementId);
}))
        {
            var percentage = 0;
            foreach (var (profileId, profile) in profiles)
            {
                if (coreConfig.Features.AchievementProfileIdBlacklist.Contains(profileId))
                {
                    continue;
                }

                if (profile.CharacterData?.PmcData?.Achievements is null)
                {
                    continue;
                }

                if (!profile.CharacterData.PmcData.Achievements.ContainsKey(achievementId))
                {
                    continue;
                }

                percentage++;
            }

            percentage = percentage / profiles.Count * 100;
            stats.Add(achievementId, percentage);
        }

        return new CompletedAchievementsResponse
        {
            Elements = stats
        };
    }
}
