using System.Text.Json.Nodes;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration;

public interface IProfileMigration
{
    /// <summary>
    /// The name of the migration
    /// </summary>
    public abstract string MigrationName { get; }

    /// <summary>
    /// An IEnumerable of migrations that need to come before the current one
    /// </summary>
    public abstract IEnumerable<Type> PrerequisiteMigrations { get; }

    /// <summary>
    /// Allows for adding checks if the profile in question can migrate
    /// </summary>
    /// <param name="profile">The profile to check</param>
    /// <param name="previouslyRanMigrations"></param>
    /// <returns>Returns true if the profile can migrate, returns false if not</returns>
    public bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations);

    /// <summary>
    /// Allows for adding checks if the profile in question can migrate with per-profile migration context
    /// </summary>
    /// <param name="profile">The profile to check</param>
    /// <param name="context">Per-profile context shared between migration steps</param>
    /// <param name="previouslyRanMigrations"></param>
    /// <returns>Returns true if the profile can migrate, returns false if not</returns>
    public bool CanMigrate(JsonObject profile, ProfileMigrationContext context, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return CanMigrate(profile, previouslyRanMigrations);
    }

    /// <summary>
    /// Migrate the profile, this should be used to handle and fix old data that has been removed from the <see cref="SptProfile"/> record
    /// or a general incompatibility due to different typing
    /// </summary>
    /// <param name="profile">The profile to migrate</param>
    /// <returns>Returns the migrated profile on success, or null if it failed</returns>
    public JsonObject? Migrate(JsonObject profile);

    /// <summary>
    /// Migrate the profile with per-profile migration context.
    /// </summary>
    /// <param name="profile">The profile to migrate</param>
    /// <param name="context">Per-profile context shared between migration steps</param>
    /// <returns>Returns the migrated profile on success, or null if it failed</returns>
    public JsonObject? Migrate(JsonObject profile, ProfileMigrationContext context)
    {
        return Migrate(profile);
    }

    /// <summary>
    /// Handles post migration of the profile, this can be used to fill new types with (old) data gotten from <see cref="Migrate"/>
    /// </summary>
    /// <returns>Should return true if successful, should return false if not</returns>
    public bool PostMigrate(SptProfile profile);

    /// <summary>
    /// Handles post migration of the profile with per-profile migration context.
    /// </summary>
    /// <param name="profile">The deserialized profile to update</param>
    /// <param name="context">Per-profile context shared between migration steps</param>
    /// <returns>Should return true if successful, should return false if not</returns>
    public bool PostMigrate(SptProfile profile, ProfileMigrationContext context)
    {
        return PostMigrate(profile);
    }
}
