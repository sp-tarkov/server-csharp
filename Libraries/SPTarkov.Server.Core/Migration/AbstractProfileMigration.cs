using System.Text.Json.Nodes;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Migration
{
    public abstract class AbstractProfileMigration : IProfileMigration
    {
        public abstract string FromVersion { get; }
        public abstract string ToVersion { get; }
        public abstract string MigrationName { get; }

        public abstract IEnumerable<Type> PrerequisiteMigrations { get; }

        public abstract bool CanMigrate(JsonObject profile);
        public abstract JsonObject? Migrate(JsonObject profile);
    }
}
