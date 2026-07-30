using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

/// <summary>
/// This migration fixes an issue in SPT 4+ where we have made StackObjectsCount a double rather than an int, this should not be the case
/// as it breaks the game due to the game expecting an int
/// </summary>
[Injectable]
public sealed class InvalidStackObjectsCountFix : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "InvalidStackObjectsCountFix"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (!profile.TryGetArray(out var items, "characters", "pmc", "Inventory", "items"))
        {
            return false;
        }

        foreach (var itemNode in items)
        {
            if (itemNode is not JsonObject itemObj)
            {
                continue;
            }

            if (!itemObj.TryGetObject(out var updObj, "upd"))
            {
                continue;
            }

            if (updObj.TryGetNode(out var stackNode, "StackObjectsCount") && stackNode is JsonValue stackValue)
            {
                // Check if the value will fit into an int
                // If it wont return false, as it's a double
                if (!stackValue.TryGetValue<int>(out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (!profile.TryGetArray(out var items, "characters", "pmc", "Inventory", "items"))
        {
            return base.Migrate(profile);
        }

        foreach (var itemNode in items)
        {
            if (itemNode is not JsonObject itemObj)
            {
                continue;
            }

            if (!itemObj.TryGetObject(out var updObj, "upd"))
            {
                continue;
            }

            if (updObj.TryGetNode(out var stackNode, "StackObjectsCount") && stackNode is JsonValue stackValue)
            {
                if (stackValue.TryGetValue<double>(out var doubleValue))
                {
                    if (doubleValue is >= int.MinValue and <= int.MaxValue)
                    {
                        updObj["StackObjectsCount"] = (int)Math.Round(doubleValue);
                    }
                    else
                    {
                        //Value is way lower or higher than an int can take, too bad!
                        updObj["StackObjectsCount"] = 1;
                    }
                }
            }
        }

        return base.Migrate(profile);
    }
}
