using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration
{
    public abstract class AbstractProfileMigration : IProfileMigration
    {
        public abstract string FromVersion { get; }
        public abstract string ToVersion { get; }
        public abstract string MigrationName { get; }

        public abstract bool CanMigrate(SptProfile profile);
        public abstract SptProfile? Migrate(SptProfile profile);
    }
}
