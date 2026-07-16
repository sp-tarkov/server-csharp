using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

[Injectable]
public sealed class InvalidRepeatableQuestFix : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "InvalidRepeatableQuestFix"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (profile.TryGetArray(out var repeatables, "characters", "pmc", "RepeatableQuests"))
        {
            foreach (var node in repeatables)
            {
                if (node is not JsonObject quest)
                {
                    continue;
                }

                quest.TryGetValue<long>(out var endTime, "endTime");

                if (endTime != 0 && !quest.TryGetNode(out _, "changeRequirement"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (profile.TryGetArray(out var repeatables, "characters", "pmc", "RepeatableQuests"))
        {
            foreach (var node in repeatables)
            {
                if (node is not JsonObject quest)
                {
                    continue;
                }

                quest.TryGetValue<long>(out var endTime, "endTime");

                if (endTime != 0 && !quest.TryGetNode(out _, "changeRequirement"))
                {
                    quest["endTime"] = 0;
                }
            }
        }

        return base.Migrate(profile);
    }
}
