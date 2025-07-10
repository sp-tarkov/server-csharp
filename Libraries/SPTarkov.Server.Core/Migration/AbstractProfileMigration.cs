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

        public abstract bool CanMigrate(
            JsonObject profile,
            IEnumerable<IProfileMigration> previouslyRanMigrations
        );
        public abstract JsonObject? Migrate(JsonObject profile);

        public virtual bool PostMigrate(SptProfile profile)
        {
            return true;
        }

        protected SemanticVersioning.Version? GetProfileVersion(JsonObject profile)
        {
            var versionString = profile["spt"]?["version"]?.GetValue<string>();

            if (versionString is null)
            {
                return null;
            }

            var versionNumber = versionString.Split(' ')[0];

            return SemanticVersioning.Version.TryParse(versionNumber, out var version)
                ? version
                : null;
        }
    }
}
