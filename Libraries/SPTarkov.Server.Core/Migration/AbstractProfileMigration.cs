using System.Text.Json.Nodes;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration;

public abstract class AbstractProfileMigration : IProfileMigration
{
    public abstract string MigrationName { get; }
    public virtual IEnumerable<Type> PrerequisiteMigrations { get; } = [];

    public abstract bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations);

    public virtual bool CanMigrate(
        JsonObject profile,
        ProfileMigrationContext context,
        IEnumerable<IProfileMigration> previouslyRanMigrations
    )
    {
        return CanMigrate(profile, previouslyRanMigrations);
    }

    public virtual JsonObject? Migrate(JsonObject profile)
    {
        return profile;
    }

    public virtual JsonObject? Migrate(JsonObject profile, ProfileMigrationContext context)
    {
        return Migrate(profile);
    }

    public virtual bool PostMigrate(SptProfile profile)
    {
        return true;
    }

    public virtual bool PostMigrate(SptProfile profile, ProfileMigrationContext context)
    {
        return PostMigrate(profile);
    }

    protected SemanticVersioning.Version? GetProfileVersion(JsonObject profile)
    {
        var versionString = profile["spt"]?["version"]?.GetValue<string>();

        if (versionString is null)
        {
            return null;
        }

        var versionNumber = versionString.Split(' ')[0];

        return SemanticVersioning.Version.TryParse(versionNumber, out var version) ? version : null;
    }
}
