using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

[Injectable]
public sealed class InvalidPmcVoiceFix(TemplateTable templateTable, GlobalTable globalTable) : AbstractProfileMigration
{
    private const string BearSide = "Bear";
    private const string UsecSide = "Usec";
    private const string VoiceParentId = "5fc100cf95572123ae738483";

    public override string MigrationName
    {
        get { return "InvalidPmcVoiceFix"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return TryGetProfileCustomization(profile, out var side, out var customization)
            && (!customization.TryGetValue<string>(out var currentVoice, "Voice") || !IsValidVoice(currentVoice, side));
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (!TryGetProfileCustomization(profile, out var side, out var customization))
        {
            return base.Migrate(profile);
        }

        if (
            (!customization.TryGetValue<string>(out var currentVoice, "Voice") || !IsValidVoice(currentVoice, side))
            && TryGetDefaultVoice(side, out var defaultVoice)
        )
        {
            customization["Voice"] = defaultVoice.Id.ToString();
        }

        return base.Migrate(profile);
    }

    private static bool TryGetProfileCustomization(JsonObject profile, out string side, out JsonObject customization)
    {
        side = "";
        customization = null!;

        return profile.TryGetValue<string>(out side, "characters", "pmc", "Info", "Side")
            && IsKnownSide(side)
            && profile.TryGetObject(out customization, "characters", "pmc", "Customization");
    }

    private bool IsValidVoice(string voiceId, string side)
    {
        return MongoId.IsValidMongoId(voiceId)
            && templateTable.Customization.TryGetValue(new MongoId(voiceId), out var voice)
            && IsVoiceCustomization(voice)
            && HasSide(voice.Properties.Side, side);
    }

    private bool TryGetDefaultVoice(string side, out CustomizationItem defaultVoice)
    {
        defaultVoice = null!;

        var defaultVoiceName = globalTable
            .Configuration.Customization.VoiceOptions.FirstOrDefault(voiceOption => HasSide(voiceOption.Side, side))
            ?.Voice;

        if (defaultVoiceName is null)
        {
            return false;
        }

        defaultVoice =
            templateTable.Customization.Values.FirstOrDefault(customization =>
                customization.Properties.Name == defaultVoiceName && IsVoiceCustomization(customization) && HasSide(customization.Properties.Side, side)
            )
            ?? templateTable.Customization.Values.FirstOrDefault(customization =>
                IsVoiceCustomization(customization) && HasSide(customization.Properties.Side, side) && customization.Properties.AvailableAsDefault
            )!;

        return defaultVoice is not null;
    }

    private static bool IsVoiceCustomization(CustomizationItem customization)
    {
        return customization.Parent == VoiceParentId;
    }

    private static bool IsKnownSide(string side)
    {
        return side.Equals(BearSide, StringComparison.OrdinalIgnoreCase) || side.Equals(UsecSide, StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasSide(IEnumerable<string>? sides, string side)
    {
        return sides?.Any(value => value.Equals(side, StringComparison.OrdinalIgnoreCase)) ?? false;
    }
}
