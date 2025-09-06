using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Migration.Migrations;

[Injectable]
public class RemoveVitaltyFromProfile : AbstractProfileMigration
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
        get { return "RemoveVitaltyFromProfile400"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return [typeof(ThreeElevenToFourZero)]; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return profile["vitality"] is not null;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        profile.Remove("vitality");

        return base.Migrate(profile);
    }
}
