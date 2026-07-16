using System.Security.Cryptography;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using Range = SemanticVersioning.Range;

namespace SPTarkov.Server.Core.Migration.Migrations._3._11;

/// <summary>
/// In 0.16.1.3.35312 BSG changed this to from an int to a hex64 encoded value.
/// </summary>
[Injectable]
public sealed class HideoutSeed : AbstractProfileMigration
{
    public string FromVersion
    {
        get { return "~3.10"; }
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

        // Check if the seed still has it's numeric value, this is not valid anymore
        var seedIsNumeric = profile.TryGetValue<long>(out _, "characters", "pmc", "Hideout", "Seed");

        return profileVersionMatches && seedIsNumeric;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (profile.TryGetObject(out var hideout, "characters", "pmc", "Hideout"))
        {
            hideout["Seed"] = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
        }

        return base.Migrate(profile);
    }
}
