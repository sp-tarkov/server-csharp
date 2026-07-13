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

                var endTimeNode = quest["endTime"];
                var endTime = endTimeNode?.GetValue<long>() ?? 0;

                if (endTime != 0 && quest["changeRequirement"] is null)
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

                var endTime = quest["endTime"]?.GetValue<long>() ?? 0;

                if (endTime != 0 && quest["changeRequirement"] is null)
                {
                    quest["endTime"] = 0;
                }
            }
        }

        return base.Migrate(profile);
    }
}
