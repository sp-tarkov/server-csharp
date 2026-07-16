using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Migration.Migrations._4._0;

[Injectable]
public sealed class RemoveGInterfaceFromVictims : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "RemoveGInterfaceFromVictims400"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return []; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (profile.TryGetArray(out var victims, "characters", "pmc", "Stats", "Eft", "Victims"))
        {
            foreach (var victim in victims)
            {
                if (victim is JsonObject victimObj)
                {
                    if (victimObj.Any(kvp => kvp.Key.StartsWith("GInterface")))
                    {
                        return true;
                    }
                }
            }
        }
        else if (profile.TryGetObject(out var aggressorObj, "characters", "pmc", "Stats", "Eft", "Aggressor"))
        {
            if (aggressorObj.Any(kvp => kvp.Key.StartsWith("GInterface")))
            {
                return true;
            }
        }

        return false;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (!profile.TryGetNode(out var eftStats, "characters", "pmc", "Stats", "Eft"))
        {
            return null;
        }

        eftStats.TryGetNode(out var victims, "Victims");
        eftStats.TryGetNode(out var aggressor, "Aggressor");

        CleanJsonNode(victims);
        CleanJsonNode(aggressor);

        return profile;
    }

    private void CleanJsonNode(JsonNode? node)
    {
        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item is JsonObject obj)
                {
                    var keysToRemove = obj.Where(kvp => kvp.Key.StartsWith("GInterface")).Select(kvp => kvp.Key).ToList();

                    foreach (var key in keysToRemove)
                    {
                        obj.Remove(key);
                    }
                }
            }
        }
        else if (node is JsonObject obj)
        {
            var keysToRemove = obj.Where(kvp => kvp.Key.StartsWith("GInterface")).Select(kvp => kvp.Key).ToList();

            foreach (var key in keysToRemove)
            {
                obj.Remove(key);
            }
        }
    }
}
