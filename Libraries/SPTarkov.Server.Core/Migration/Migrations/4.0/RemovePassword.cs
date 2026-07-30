using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;

namespace SPTarkov.Server.Core.Migration.Migrations._4._0;

/// <summary>
/// Password property was removed from profile.info in 4.0
/// </summary>
[Injectable]
public sealed class RemovePassword : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "RemovePassword-SPTSharp"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return []; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        var hasPassword = profile.TryGetNode(out _, "info", "password");

        return hasPassword;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (profile.TryGetObject(out var profileInfo, "info"))
        {
            profileInfo.Remove("password");
        }

        return base.Migrate(profile);
    }
}
