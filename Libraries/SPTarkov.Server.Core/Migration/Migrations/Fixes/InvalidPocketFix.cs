using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Migration.Migrations.Fixes;

[Injectable]
public sealed class InvalidPocketFix(TemplateTable templateTable) : AbstractProfileMigration
{
    public const string DEFAULT_POCKETS = "627a4e6b255f7527fb05a0f6";
    public const string UNHEARD_POCKETS = "65e080be269cbd5c5005e529";

    public override string MigrationName
    {
        get { return "InvalidPocketFix"; }
    }

    private enum PocketStatus
    {
        Valid,
        Missing,
        Invalid,
    }

    private PocketStatus GetPmcPocketStatus(JsonObject profile)
    {
        if (!profile.TryGetArray(out var items, "characters", "pmc", "Inventory", "items"))
        {
            // Uninitialized profile, just pass valid
            return PocketStatus.Valid;
        }

        foreach (var itemNode in items)
        {
            if (itemNode is not JsonObject itemObj)
            {
                continue;
            }

            if (
                itemObj.TryGetValue<string>(out var slotId, "slotId")
                && slotId == "Pockets"
            )
            {
                if (itemObj.TryGetValue<string>(out var template, "_tpl"))
                {
                    return templateTable.Items.ContainsKey(template) ? PocketStatus.Valid : PocketStatus.Invalid;
                }
            }
        }

        return PocketStatus.Missing;
    }

    private PocketStatus GetScavPocketStatus(JsonObject profile)
    {
        if (!profile.TryGetArray(out var items, "characters", "scav", "Inventory", "items"))
        {
            // Uninitialized profile, just pass valid
            return PocketStatus.Valid;
        }

        foreach (var itemNode in items)
        {
            if (itemNode is not JsonObject itemObj)
            {
                continue;
            }

            if (
                itemObj.TryGetValue<string>(out var slotId, "slotId")
                && slotId == "Pockets"
            )
            {
                if (itemObj.TryGetValue<string>(out var template, "_tpl"))
                {
                    return templateTable.Items.ContainsKey(template) ? PocketStatus.Valid : PocketStatus.Invalid;
                }
            }
        }

        return PocketStatus.Missing;
    }

    private bool HasCompletedOldPatterns(JsonObject profile)
    {
        if (!profile.TryGetArray(out var quests, "characters", "pmc", "Quests"))
        {
            return false;
        }

        foreach (var questNode in quests)
        {
            if (questNode is not JsonObject questObj)
            {
                continue;
            }

            if (
                questObj.TryGetValue<string>(out var qId, "qid")
                && qId == QuestTpl.OLD_PATTERNS.ToString()
                && questObj.TryGetValue<string>(out var status, "status")
                && status.Equals(nameof(QuestStatusEnum.Success), StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }

        return false;
    }

    private bool IsUnheardProfile(JsonObject profile)
    {
        profile.TryGetValue<string>(out var gameVersion, "characters", "pmc", "Info", "GameVersion");

        if (!string.IsNullOrEmpty(gameVersion))
        {
            return gameVersion.Equals("unheard_edition", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private JsonObject CreatePocketItem(string parentId, string defaultPocketTpl)
    {
        return new JsonObject
        {
            ["_id"] = new MongoId().ToString(),
            ["_tpl"] = defaultPocketTpl,
            ["parentId"] = parentId,
            ["slotId"] = "Pockets",
        };
    }

    // Set slotId to hideout, parentId to sorting table & remove location so that the sorting table will automatically pick a location
    private void MoveItemsToSortingTable(JsonArray items, string? sortingTableId)
    {
        foreach (var item in items.OfType<JsonObject>())
        {
            if (
                item.TryGetValue<string>(out var slotId, "slotId")
                && (
                    (
                        slotId.StartsWith("pocket", StringComparison.OrdinalIgnoreCase)
                        // Exclude the pcokets itself
                        && !slotId.Equals("Pockets", StringComparison.OrdinalIgnoreCase)
                    )
                    // Special slots are also keyed to the pockets
                    || slotId.StartsWith("SpecialSlot", StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                item["slotId"] = "hideout";
                item["parentId"] = sortingTableId;
                item.Remove("location");
            }
        }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (GetPmcPocketStatus(profile) != PocketStatus.Valid || GetScavPocketStatus(profile) != PocketStatus.Valid)
        {
            return true;
        }

        return false;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        var pmcPocketStatus = GetPmcPocketStatus(profile);
        var scavPocketStatus = GetScavPocketStatus(profile);

        if (pmcPocketStatus != PocketStatus.Valid)
        {
            profile.TryGetArray(out var items, "characters", "pmc", "Inventory", "items");
            profile.TryGetObject(out var pmcInventory, "characters", "pmc", "Inventory");
            pmcInventory.TryGetValue<string>(out var pmcSortingTable, "sortingTable");
            pmcInventory.TryGetValue<string>(out var pmcEquipment, "equipment");

            var pmcPocketTpl = DEFAULT_POCKETS;

            if (IsUnheardProfile(profile) || HasCompletedOldPatterns(profile))
            {
                pmcPocketTpl = UNHEARD_POCKETS;
            }

            if (pmcPocketStatus == PocketStatus.Missing)
            {
                if (items != null && pmcEquipment != null)
                {
                    items.Add(CreatePocketItem(pmcEquipment, pmcPocketTpl));
                    MoveItemsToSortingTable(items, pmcSortingTable);
                }
            }
            else if (pmcPocketStatus == PocketStatus.Invalid)
            {
                foreach (var item in items.OfType<JsonObject>())
                {
                    if (
                        item.TryGetValue<string>(out var slotId, "slotId")
                        && slotId == "Pockets"
                    )
                    {
                        item["_tpl"] = pmcPocketTpl;

                        MoveItemsToSortingTable(items, pmcSortingTable);
                    }
                }
            }
        }

        if (scavPocketStatus != PocketStatus.Valid)
        {
            profile.TryGetArray(out var scavItems, "characters", "scav", "Inventory", "items");
            profile.TryGetObject(out var scavInventory, "characters", "scav", "Inventory");
            scavInventory.TryGetValue<string>(out var scavEquipment, "equipment");

            if (scavPocketStatus == PocketStatus.Missing)
            {
                if (scavItems != null && scavEquipment != null)
                {
                    scavItems.Add(CreatePocketItem(scavEquipment, DEFAULT_POCKETS));
                }
            }
            else if (scavPocketStatus == PocketStatus.Invalid)
            {
                foreach (var item in scavItems.OfType<JsonObject>())
                {
                    if (
                        item.TryGetValue<string>(out var slotId, "slotId")
                        && slotId == "Pockets"
                    )
                    {
                        item["_tpl"] = DEFAULT_POCKETS;
                    }
                }
            }
        }

        return base.Migrate(profile);
    }
}
