using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

[Injectable]
public sealed class InvalidPmcHeadFix(TemplateTable templateTable) : AbstractProfileMigration
{
    private const string BearSide = "Bear";
    private const string UsecSide = "Usec";
    private const string HeadBodyPart = "Head";

    public override string MigrationName
    {
        get { return "InvalidPmcHeadFix"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return TryGetProfileCustomization(profile, out var side, out var customization)
            && (!customization.TryGetValue<string>(out var currentHead, "Head") || !IsValidHead(currentHead, side));
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (!TryGetProfileCustomization(profile, out var side, out var customization))
        {
            return base.Migrate(profile);
        }

        if (
            (!customization.TryGetValue<string>(out var currentHead, "Head") || !IsValidHead(currentHead, side))
            && TryGetDefaultHead(side, out var defaultHead)
        )
        {
            customization["Head"] = defaultHead.Id.ToString();
        }

        return base.Migrate(profile);
    }

    private static bool TryGetProfileCustomization(JsonObject profile, out string side, out JsonObject customization)
    {
        side = "";
        customization = null!;

        return profile.TryGetValue(out side, "characters", "pmc", "Info", "Side")
            && IsKnownSide(side)
            && profile.TryGetObject(out customization, "characters", "pmc", "Customization");
    }

    private bool IsValidHead(string headId, string side)
    {
        return MongoId.IsValidMongoId(headId)
            && templateTable.Customization.TryGetValue(new MongoId(headId), out var head)
            && IsHeadCustomization(head)
            && HasSide(head.Properties.Side, side);
    }

    private bool TryGetDefaultHead(string side, out CustomizationItem defaultHead)
    {
        var defaultHeadName = side.Equals(UsecSide, StringComparison.OrdinalIgnoreCase) ? "DefaultUsecHead" : "DefaultBearHead";

        defaultHead = templateTable.Customization.Values.FirstOrDefault(customization =>
            customization.Name == defaultHeadName && IsHeadCustomization(customization) && HasSide(customization.Properties.Side, side)
        )!;
        return defaultHead is not null;
    }

    private static bool IsHeadCustomization(CustomizationItem customization)
    {
        return customization.Properties.BodyPart == HeadBodyPart;
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
