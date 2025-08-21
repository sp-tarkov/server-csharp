using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Migration.Migrations;

[Injectable]
public class RemoveGInterfaceFromVictims : AbstractProfileMigration
{
    public override string FromVersion
    {
        get { return "~4.0"; }
    }

    public override string ToVersion
    {
        get { return "~4.0"; }
    }

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
        if (profile?["characters"]?["pmc"]?["Stats"]?["Eft"]?["Victims"] is JsonArray victims)
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
        else if (profile?["characters"]?["pmc"]?["Stats"]?["Eft"]?["Aggressor"] is JsonObject aggressorObj)
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
        if (profile?["characters"]?["pmc"]?["Stats"]?["Eft"] is not JsonNode eftStats)
        {
            return null;
        }

        CleanJsonNode(eftStats["Victims"]);
        CleanJsonNode(eftStats["Aggressor"]);

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
