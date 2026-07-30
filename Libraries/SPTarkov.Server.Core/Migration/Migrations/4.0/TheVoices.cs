using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Migration.Migrations._3._11;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Migration.Migrations._4._0;

/// <summary>
/// In 16.8.0.37972 BSG added customization for voices, technically this only affects BE profiles, but this should fix these.
/// </summary>
[Injectable]
public sealed class TheVoices(TemplateTable templateTable) : AbstractProfileMigration
{
    private const string PmcVoiceIsMissingContextKey = "PmcVoiceIsMissing";
    private const string ScavVoiceIsMissingContextKey = "ScavVoiceIsMissing";
    private const string HasScavVoiceFromPreviousSptVersionContextKey = "HasScavVoiceFromPreviousSptVersion";

    public override string MigrationName
    {
        get { return "TheVoices400"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        // Requires ThreeTenToThreeEleven on legacy profiles, due to that adding the customization object for the first time
        get { return [typeof(ThreeTenToThreeEleven)]; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return CanMigrate(profile, new ProfileMigrationContext(), previouslyRanMigrations);
    }

    public override bool CanMigrate(
        JsonObject profile,
        ProfileMigrationContext context,
        IEnumerable<IProfileMigration> previouslyRanMigrations
    )
    {
        var pmcVoiceIsMissing = !profile.TryGetNode(out _, "characters", "pmc", "Customization", "Voice");
        var scavVoiceIsMissing = !profile.TryGetNode(out _, "characters", "scav", "Customization", "Voice");
        var hasScavVoiceFromPreviousSptVersion = profile.TryGetNode(out _, "characters", "scav", "Info", "Voice");

        context.Set(PmcVoiceIsMissingContextKey, pmcVoiceIsMissing);
        context.Set(ScavVoiceIsMissingContextKey, scavVoiceIsMissing);
        context.Set(HasScavVoiceFromPreviousSptVersionContextKey, hasScavVoiceFromPreviousSptVersion);

        return pmcVoiceIsMissing || scavVoiceIsMissing || hasScavVoiceFromPreviousSptVersion;
    }

    public override JsonObject? Migrate(JsonObject profile, ProfileMigrationContext context)
    {
        var pmcVoiceIsMissing = context.Get<bool>(PmcVoiceIsMissingContextKey);
        var scavVoiceIsMissing = context.Get<bool>(ScavVoiceIsMissingContextKey);
        var hasScavVoiceFromPreviousSptVersion = context.Get<bool>(HasScavVoiceFromPreviousSptVersionContextKey);

        if (pmcVoiceIsMissing)
        {
            HandlePmcVoice(profile);
        }

        if (scavVoiceIsMissing)
        {
            HandleScavVoice(profile);
        }

        // Handle this only if scavVoiceIsMissing hasn't already processed, there was a time the SPT server still saved this
        // Old var to profiles
        if (hasScavVoiceFromPreviousSptVersion && !scavVoiceIsMissing)
        {
            if (profile.TryGetObject(out var scavInfo, "characters", "scav", "Info"))
            {
                scavInfo.Remove("Voice");
            }
        }

        return base.Migrate(profile);
    }

    private void HandlePmcVoice(JsonObject profileObject)
    {
        if (
            !profileObject.TryGetObject(out var pmcInfo, "characters", "pmc", "Info")
            || !profileObject.TryGetObject(out var pmcCustomization, "characters", "pmc", "Customization")
        )
        {
            return;
        }

        pmcInfo.TryGetValue<string>(out var oldVoice, "Voice");
        pmcInfo.Remove("Voice");

        var voiceMongoId = templateTable.Customization.FirstOrDefault(x => x.Value.Properties.Name == (oldVoice ?? "")).Key;

        pmcCustomization["Voice"] = voiceMongoId.ToString();
    }

    private void HandleScavVoice(JsonObject profileObject)
    {
        if (
            !profileObject.TryGetObject(out var scavInfo, "characters", "scav", "Info")
            || !profileObject.TryGetObject(out var scavCustomization, "characters", "scav", "Customization")
        )
        {
            return;
        }

        scavInfo.TryGetValue<string>(out var oldVoice, "Voice");
        scavInfo.Remove("Voice");

        var voiceMongoId = templateTable.Customization.FirstOrDefault(x => x.Value.Properties.Name == (oldVoice ?? "")).Key;

        scavCustomization["Voice"] = voiceMongoId.ToString();
    }
}
