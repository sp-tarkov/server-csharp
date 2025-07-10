using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using Range = SemanticVersioning.Range;

namespace SPTarkov.Server.Core.Migration.Migrations
{
    /// <summary>
    /// In 16.8.0.37972 BSG added customization for voices, technically this only affects BE profiles, but this should fix these.
    /// </summary>
    [Injectable]
    public class TheVoices(DatabaseService databaseService, ConfigServer configServer)
        : AbstractProfileMigration
    {
        private readonly CoreConfig _coreConfig = configServer.GetConfig<CoreConfig>();

        public override string FromVersion
        {
            get { return "~4.0"; }
        }

        public override string ToVersion
        {
            get { return "~4.0"; }
        }

        public override string MigrationName
        {
            get { return "TheVoices400"; }
        }

        public override IEnumerable<Type> PrerequisiteMigrations
        {
            get { return []; }
        }

        public override bool CanMigrate(JsonObject profile)
        {
            var sptVersion = ProgramStatics.SPT_VERSION() ?? _coreConfig.SptVersion;

            if (!SemanticVersioning.Version.TryParse(sptVersion, out var actualVersion))
            {
                return false;
            }

            var fromRange = Range.Parse(FromVersion);
            var toRange = Range.Parse(ToVersion);

            bool versionMatches =
                fromRange.IsSatisfied(actualVersion) && toRange.IsSatisfied(actualVersion);
            bool voiceIsMissing = profile["characters"]?["pmc"]?["Customization"]?["Voice"] == null;

            return versionMatches && voiceIsMissing;
        }

        public override JsonObject? Migrate(JsonObject profile)
        {
            HandlePmcVoice(profile);
            HandleScavVoice(profile);

            return profile;
        }

        private void HandlePmcVoice(JsonObject profileObject)
        {
            var pmcInfo = profileObject["characters"]!["pmc"]!["Info"] as JsonObject;

            var oldVoice = pmcInfo["Voice"]?.ToString() ?? "";
            pmcInfo.Remove("Voice");

            var voiceMongoId = databaseService
                .GetCustomization()
                .FirstOrDefault(x => x.Value.Properties.Name == oldVoice)
                .Key;

            profileObject["characters"]!["pmc"]!["Customization"]!["Voice"] =
                voiceMongoId.ToString();
        }

        private void HandleScavVoice(JsonObject profileObject)
        {
            var pmcInfo = profileObject["characters"]!["scav"]!["Info"] as JsonObject;

            var oldVoice = pmcInfo["Voice"]?.ToString() ?? "";
            pmcInfo.Remove("Voice");

            var voiceMongoId = databaseService
                .GetCustomization()
                .FirstOrDefault(x => x.Value.Properties.Name == oldVoice)
                .Key;

            profileObject["characters"]!["scav"]!["Customization"]!["Voice"] =
                voiceMongoId.ToString();
        }
    }
}
