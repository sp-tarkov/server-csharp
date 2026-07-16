using System.Globalization;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

[Injectable]
public sealed class InvalidSkillProgressFix : AbstractProfileMigration
{
    public override string MigrationName
    {
        get { return "InvalidSkillProgressFix"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (!profile.TryGetArray(out var skills, "characters", "pmc", "Skills", "Common"))
        {
            return false;
        }

        return skills.OfType<JsonObject>().Any(HasInvalidSkillProgress);
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (!profile.TryGetArray(out var skills, "characters", "pmc", "Skills", "Common"))
        {
            return base.Migrate(profile);
        }

        foreach (var skill in skills.OfType<JsonObject>())
        {
            FixSkillProperty(skill, "PointsEarnedDuringSession");
            FixSkillProperty(skill, "Progress");
        }

        return base.Migrate(profile);
    }

    private static bool HasInvalidSkillProgress(JsonObject skill)
    {
        return NeedsFixing(skill, "PointsEarnedDuringSession") || NeedsFixing(skill, "Progress");
    }

    private static void FixSkillProperty(JsonObject skill, string propertyName)
    {
        if (TryGetSkillValue(skill, propertyName, out var skillValue) && !NeedsFixing(skillValue))
        {
            return;
        }

        skill[propertyName] = GetFixedValue(skillValue);
    }

    private static bool NeedsFixing(JsonObject skill, string propertyName)
    {
        return !TryGetSkillValue(skill, propertyName, out var skillValue) || NeedsFixing(skillValue);
    }

    private static bool NeedsFixing(double skillValue)
    {
        return !double.IsFinite(skillValue) || skillValue > CommonSkill.MaxSkillProgress || IsNegativeOrNegativeZero(skillValue);
    }

    private static double GetFixedValue(double skillValue)
    {
        if (!double.IsFinite(skillValue) || IsNegativeOrNegativeZero(skillValue))
        {
            return 0;
        }

        return Math.Min(skillValue, CommonSkill.MaxSkillProgress);
    }

    private static bool TryGetSkillValue(JsonObject skill, string propertyName, out double skillValue)
    {
        skillValue = 0;

        if (!skill.TryGetNode(out var node, propertyName) || node is not JsonValue jsonValue)
        {
            return false;
        }

        if (jsonValue.TryGetValue(out skillValue))
        {
            return true;
        }

        if (
            jsonValue.TryGetValue<string>(out var stringValue)
            && double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out skillValue)
        )
        {
            return true;
        }

        skillValue = 0;
        return false;
    }

    private static bool IsNegativeOrNegativeZero(double skillValue)
    {
        return skillValue < 0 || BitConverter.DoubleToInt64Bits(skillValue) == long.MinValue;
    }
}
