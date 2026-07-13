using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

[Injectable]
public sealed class InvalidTerraGroupEmployeeQuestFix : AbstractProfileMigration
{
    private const string ColleaguesPartThreeQuestId = "5edac34d0bb72a50635c2bfa";
    private const string SadistQuestId = "5edab4b1218d181e29451435";

    public override string MigrationName
    {
        get { return "InvalidTerraGroupEmployeeQuestFix"; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        return TryGetQuestPair(profile, out var colleaguesPartThree, out var sadist)
            && IsInvalidQuestPair(colleaguesPartThree, sadist);
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (!TryGetQuestPair(profile, out var colleaguesPartThree, out var sadist))
        {
            return base.Migrate(profile);
        }

        if (
            QuestHasStatus(sadist, QuestStatusEnum.Success)
            && QuestHasStatus(colleaguesPartThree, QuestStatusEnum.Locked)
        )
        {
            colleaguesPartThree["status"] = (int)QuestStatusEnum.Fail;
        }
        else if (
            QuestHasStatus(sadist, QuestStatusEnum.Locked)
            && QuestHasStatus(colleaguesPartThree, QuestStatusEnum.Success)
        )
        {
            sadist["status"] = (int)QuestStatusEnum.Fail;
        }

        return base.Migrate(profile);
    }

    private static bool TryGetQuestPair(JsonObject profile, out JsonObject colleaguesPartThree, out JsonObject sadist)
    {
        colleaguesPartThree = null!;
        sadist = null!;

        if (!profile.TryGetArray(out var quests, "characters", "pmc", "Quests"))
        {
            return false;
        }

        foreach (var quest in quests.OfType<JsonObject>())
        {
            if (!quest.TryGetValue<string>(out var questId, "qid"))
            {
                continue;
            }

            if (questId == ColleaguesPartThreeQuestId)
            {
                colleaguesPartThree = quest;
            }
            else if (questId == SadistQuestId)
            {
                sadist = quest;
            }
        }

        return colleaguesPartThree is not null && sadist is not null;
    }

    private static bool IsInvalidQuestPair(JsonObject colleaguesPartThree, JsonObject sadist)
    {
        return QuestHasStatus(sadist, QuestStatusEnum.Success)
                && QuestHasStatus(colleaguesPartThree, QuestStatusEnum.Locked)
            || QuestHasStatus(sadist, QuestStatusEnum.Locked)
                && QuestHasStatus(colleaguesPartThree, QuestStatusEnum.Success);
    }

    private static bool QuestHasStatus(JsonObject quest, QuestStatusEnum status)
    {
        return quest.TryGetValue<int>(out var questStatus, "status") && questStatus == (int)status;
    }
}
