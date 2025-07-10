using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration
{
    public interface IProfileMigration
    {
        /// <summary>
        /// Allows for adding checks if the profile in question can migrate
        /// </summary>
        /// <param name="profile">The profile to check</param>
        /// <returns>Returns true if the profile can migrate, returns false if not</returns>
        public bool CanMigrate(SptProfile profile);

        /// <summary>
        /// Migrate the profile
        /// </summary>
        /// <param name="profile">The profile to migrate</param>
        /// <returns>Returns the migrated profile on success, or null if it failed</returns>
        public SptProfile? Migrate(SptProfile profile);
    }
}
