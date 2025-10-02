using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Migration.Migrations;

/// <summary>
/// Password property was removed from profile.info in 4.0
/// </summary>
[Injectable]
public class RemovePassword : AbstractProfileMigration
{
    public override string FromVersion
    {
        get { return "~3.11"; }
    }

    public override string ToVersion
    {
        get { return "4.0"; }
    }

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
        var hasPassword = profile["info"]?["password"] != null;

        return hasPassword;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        var profileInfo = profile["info"] as JsonObject;
        profileInfo?.Remove("password");

        return base.Migrate(profile);
    }
}
