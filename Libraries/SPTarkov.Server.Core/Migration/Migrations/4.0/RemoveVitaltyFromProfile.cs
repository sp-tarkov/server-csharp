using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Migration.Migrations._4._0;

[Injectable]
public sealed class RemoveVitaltyFromProfile : AbstractProfileMigration
{
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
        return profile.TryGetNode(out _, "vitality");
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        profile.Remove("vitality");

        return base.Migrate(profile);
    }
}
