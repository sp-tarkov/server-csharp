using Core.Helpers;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Controllers;

[Injectable]
public class AchievementController(
    ProfileHelper profileHelper,
    DatabaseService databaseService,
    ConfigServer configServer
)
{
    protected CoreConfig coreConfig = configServer.GetConfig<CoreConfig>();

    public virtual GetAchievementsResponse GetAchievements(string sessionID)
    {
        return new GetAchievementsResponse
        {
            Elements = databaseService.GetAchievements()
        };
    }

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
