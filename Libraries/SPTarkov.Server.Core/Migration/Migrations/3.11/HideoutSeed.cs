using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using Range = SemanticVersioning.Range;

namespace SPTarkov.Server.Core.Migration.Migrations
{
    /// <summary>
    /// In 0.16.1.3.35312 BSG changed this to from an int to a hex64 encoded value.
    /// </summary>
    [Injectable]
    public class HideoutSeed : AbstractProfileMigration
    {
        public override string FromVersion
        {
            get { return "~3.10"; }
        }

        public override string ToVersion
        {
            get { return "3.11"; }
        }

        public override string MigrationName
        {
            get { return "HideoutSeed311-SPTSharp"; }
        }

        public override IEnumerable<Type> PrerequisiteMigrations
        {
            get { return []; }
        }

        public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
        {
            var profileVersion = GetProfileVersion(profile);
            var fromRange = Range.Parse(FromVersion);
            var profileVersionMatches = fromRange.IsSatisfied(profileVersion);

            var seedNode = profile["characters"]?["pmc"]?["Hideout"]?["Seed"];

            // Check if the seed still has it's numeric value, this is not valid anymore
            var seedIsNumeric = seedNode is JsonValue seedValue && seedValue.TryGetValue<long>(out _);

            return profileVersionMatches && seedIsNumeric;
        }

        public override JsonObject? Migrate(JsonObject profile)
        {
            profile["characters"]!["pmc"]!["Hideout"]!["Seed"] = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));

            return base.Migrate(profile);
        }
    }
}
