using System.Text.Json.Nodes;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration;

public interface IProfileMigration
{
    /// <summary>
    /// Allows for adding checks if the profile in question can migrate
    /// </summary>
    /// <param name="profile">The profile to check</param>
    /// <param name="previouslyRanMigrations"></param>
    /// <returns>Returns true if the profile can migrate, returns false if not</returns>
    public bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations);

    /// <summary>
    /// Migrate the profile, this should be used to handle and fix old data that has been removed from the <see cref="SptProfile"/> record
    /// or a general incompatibility due to different typing
    /// </summary>
    /// <param name="profile">The profile to migrate</param>
    /// <returns>Returns the migrated profile on success, or null if it failed</returns>
    public JsonObject? Migrate(JsonObject profile);

    /// <summary>
    /// Handles post migration of the profile, this can be used to fill new types with (old) data gotten from <see cref="Migrate"/>
    /// </summary>
    /// <returns>Should return true if successful, should return false if not</returns>
    public bool PostMigrate(SptProfile profile);
}
