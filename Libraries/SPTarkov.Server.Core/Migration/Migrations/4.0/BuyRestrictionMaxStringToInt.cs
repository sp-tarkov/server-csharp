using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Migration.Migrations._4._0;

[Injectable]
public sealed class BuyRestrictionMaxStringToInt : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "BuyRestrictionMaxStringToInt400"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return [typeof(ThreeElevenToFourZero)]; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (profile.TryGetArray(out var items, "characters", "pmc", "Inventory", "items"))
        {
            foreach (var itemNode in items)
            {
                if (itemNode is not JsonObject itemObj)
                {
                    continue;
                }

                if (itemObj.TryGetObject(out var updObj, "upd"))
                {
                    if (updObj.TryGetValue<string?>(out _, "BuyRestrictionMax"))
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
        if (profile.TryGetArray(out var items, "characters", "pmc", "Inventory", "items"))
        {
            foreach (var itemNode in items)
            {
                if (itemNode is not JsonObject itemObj)
                {
                    continue;
                }

                if (itemObj.TryGetObject(out var updObj, "upd") && updObj.TryGetValue<string?>(out var strValue, "BuyRestrictionMax"))
                {
                    if (int.TryParse(strValue, out var intValue))
                    {
                        updObj["BuyRestrictionMax"] = intValue;
                    }
                    else
                    {
                        updObj.Remove("BuyRestrictionMax");
                    }
                }
            }
        }

        return base.Migrate(profile);
    }
}
