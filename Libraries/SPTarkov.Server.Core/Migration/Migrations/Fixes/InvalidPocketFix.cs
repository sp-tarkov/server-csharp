using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
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
        if (profile["characters"]?["pmc"]?["Inventory"]?["items"] is not JsonArray items)
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
                itemObj.TryGetPropertyValue("slotId", out var slotNode)
                && slotNode is JsonValue slotValue
                && slotValue.TryGetValue<string>(out var slotId)
                && slotId == "Pockets"
            )
            {
                if (
                    itemObj.TryGetPropertyValue("_tpl", out var tplNode)
                    && tplNode is JsonValue tplValue
                    && tplValue.TryGetValue<string>(out var template)
                )
                {
                    return templateTable.Items.ContainsKey(template) ? PocketStatus.Valid : PocketStatus.Invalid;
                }
            }
        }

        return PocketStatus.Missing;
    }

    private PocketStatus GetScavPocketStatus(JsonObject profile)
    {
        if (profile["characters"]?["scav"]?["Inventory"]?["items"] is not JsonArray items)
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
                itemObj.TryGetPropertyValue("slotId", out var slotNode)
                && slotNode is JsonValue slotValue
                && slotValue.TryGetValue<string>(out var slotId)
                && slotId == "Pockets"
            )
            {
                if (
                    itemObj.TryGetPropertyValue("_tpl", out var tplNode)
                    && tplNode is JsonValue tplValue
                    && tplValue.TryGetValue<string>(out var template)
                )
                {
                    return templateTable.Items.ContainsKey(template) ? PocketStatus.Valid : PocketStatus.Invalid;
                }
            }
        }

        return PocketStatus.Missing;
    }

    private bool HasCompletedOldPatterns(JsonObject profile)
    {
        if (profile["characters"]?["pmc"]?["Quests"] is not JsonArray quests)
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
                questObj.TryGetPropertyValue("qid", out var qIdNode)
                && qIdNode is JsonValue qIdValue
                && qIdValue.TryGetValue<string>(out var qId)
                && qId == QuestTpl.OLD_PATTERNS.ToString()
                && questObj.TryGetPropertyValue("status", out var statusNode)
                && statusNode is JsonValue statusValue
                && statusValue.TryGetValue<string>(out var status)
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
        var gameVersion = profile?["characters"]?["pmc"]?["Info"]?["GameVersion"]?.GetValue<string>();

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
    private void MoveItemsToSortingTable(JsonArray items, string sortingTableId)
    {
        foreach (var item in items.OfType<JsonObject>())
        {
            if (
                item.TryGetPropertyValue("slotId", out var slotNode)
                && slotNode is JsonValue slotNodeValue
                && slotNodeValue.TryGetValue<string>(out var slotId)
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
            var items = profile["characters"]?["pmc"]?["Inventory"]?["items"] as JsonArray;
            var pmcInventory = profile["characters"]?["pmc"]?["Inventory"] as JsonObject;
            var pmcSortingTable = pmcInventory?["sortingTable"]?.GetValue<string>()!;
            var pmcEquipment = pmcInventory?["equipment"]?.GetValue<string>();

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
                        item.TryGetPropertyValue("slotId", out var slotNode)
                        && slotNode is JsonValue slotNodeValue
                        && slotNodeValue.TryGetValue<string>(out var slotId)
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
            var scavItems = profile["characters"]?["scav"]?["Inventory"]?["items"] as JsonArray;
            var scavInventory = profile["characters"]?["scav"]?["Inventory"] as JsonObject;
            var scavEquipment = scavInventory?["equipment"]?.GetValue<string>();

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
                        item.TryGetPropertyValue("slotId", out var slotNode)
                        && slotNode is JsonValue slotNodeValue
                        && slotNodeValue.TryGetValue<string>(out var slotId)
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
