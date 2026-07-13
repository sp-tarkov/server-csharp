using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Profile;

namespace SPTarkov.Server.Core.Migration.Migrations._4._1;

[Injectable]
public sealed class AddMissingPrestigesToProfile : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "AddMissingPrestigesToProfile"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (
            profile.TryGetObject(out var prestiges, "characters", "pmc", "Prestige")
            && profile.TryGetObject(out var achievements, "characters", "pmc", "Achievements")
        )
        {
            foreach (var prestige in prestiges)
            {
                var id = prestige.Key;

                if (PrestigeHelper.PrestigeAchievements.TryGetValue(id, out var AchievementId))
                {
                    if (!achievements.ContainsKey(AchievementId))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (
            profile.TryGetObject(out var prestiges, "characters", "pmc", "Prestige")
            && profile.TryGetObject(out var achievements, "characters", "pmc", "Achievements")
        )
        {
            foreach (var prestige in prestiges)
            {
                var id = prestige.Key;
                var timestamp = prestige.Value!.GetValue<long>();

                if (PrestigeHelper.PrestigeAchievements.TryGetValue(id, out var AchievementId))
                {
                    if (!achievements.ContainsKey(AchievementId))
                    {
                        achievements.Add(AchievementId, timestamp);
                    }
                }
            }
        }

        return base.Migrate(profile);
    }
}
