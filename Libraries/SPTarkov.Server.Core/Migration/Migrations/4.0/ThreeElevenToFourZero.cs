using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Migration.Migrations._3._11;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils;
using Range = SemanticVersioning.Range;

namespace SPTarkov.Server.Core.Migration.Migrations._4._0;

[Injectable]
public sealed class ThreeElevenToFourZero(Watermark watermark) : AbstractProfileMigration
{
    public string FromVersion
    {
        get { return "~3.11"; }
    }

    public string ToVersion
    {
        get { return "4.0"; }
    }

    public override string MigrationName
    {
        get { return "311x-SPTSharp"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return [typeof(ThreeTenToThreeEleven)]; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        var profileVersion = GetProfileVersion(profile);

        var fromRange = Range.Parse(FromVersion);

        var versionMatches =
            fromRange.IsSatisfied(profileVersion)
            || PrerequisiteMigrations.All(prereq => previouslyRanMigrations.Any(r => r.GetType() == prereq));

        return versionMatches;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (profile.TryGetObject(out var production, "characters", "pmc", "Hideout", "Production"))
        {
            foreach (var entry in production)
            {
                if (
                    entry.Value is JsonObject productionEntry
                    && productionEntry.TryGetValue<string>(out var startTimestampStr, "StartTimestamp")
                    && long.TryParse(startTimestampStr, out var startTimestampInt)
                )
                {
                    productionEntry["StartTimestamp"] = startTimestampInt;
                }
            }
        }

        if (profile.TryGetArray(out var insuranceArray, "insurance"))
        {
            foreach (var item in insuranceArray)
            {
                if (item is JsonObject insuranceEntry)
                {
                    if (insuranceEntry.TryGetValue<double>(out var timeAsDouble, "scheduledTime"))
                    {
                        // Handle the node server having turned this value into a double
                        insuranceEntry["scheduledTime"] = Convert.ToInt32(timeAsDouble);
                    }
                }
            }
        }

        return base.Migrate(profile);
    }

    public override bool PostMigrate(SptProfile profile)
    {
        profile.SptData.ExtraRepeatableQuests = [];

        profile.SptData.Version = $"{watermark.GetVersionTag()} (Migrated from 3.11)";

        return base.PostMigrate(profile);
    }
}
