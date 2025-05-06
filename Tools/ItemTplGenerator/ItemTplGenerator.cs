using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace ItemTplGenerator;

[Injectable]
public class ItemTplGenerator(
    ISptLogger<ItemTplGenerator> _logger,
    DatabaseServer _databaseServer,
    LocaleService _localeService,
    ItemHelper _itemHelper,
    FileUtil _fileUtil,
    IEnumerable<IOnLoad> _onLoadComponents
)
{
    private readonly HashSet<string> collidedEnumKeys = [];
    private string enumDir;
    private IDictionary<string, string> itemOverrides;
    private Dictionary<string, TemplateItem> items;

    public async Task Run()
    {
        itemOverrides = ItemOverrides.ItemOverridesDictionary;
        // Load all onload components, this gives us access to most of SPTs injections
        foreach (var onLoad in _onLoadComponents)
        {
            if (onLoad is HttpCallbacks)
            {
                continue;
            }

            await onLoad.OnLoad();
        }

        // Figure out our source and target directories
        var projectDir = Directory.GetParent("./").Parent.Parent.Parent.Parent.Parent;
        enumDir = Path.Combine(projectDir.FullName, "Libraries", "SPTarkov.Server.Core", "Models", "Enums");
        items = _databaseServer.GetTables().Templates.Items;

        // Generate an object containing all item name to ID associations
        var orderedItemsObject = GenerateItemsObject();

        // Log any changes to enum values, so the source can be updated as required
        LogEnumValueChanges(orderedItemsObject, "ItemTpl", typeof(ItemTpl));
        var itemTplOutPath = Path.Combine(enumDir, "ItemTpl.cs");
        WriteEnumsToFile(
            itemTplOutPath,
            new Dictionary<string, Dictionary<string, string>>
            {
                { nameof(ItemTpl), orderedItemsObject }
            }
        );

        // Handle the weapon type enums
        var weaponsObject = GenerateWeaponsObject();
        LogEnumValueChanges(weaponsObject, "Weapons", typeof(Weapons));
        var weaponTypeOutPath = Path.Combine(enumDir, "Weapons.cs");
        WriteEnumsToFile(
            weaponTypeOutPath,
            new Dictionary<string, Dictionary<string, string>>
            {
                { nameof(Weapons), weaponsObject }
            }
        );

        _logger.Info("Generating items finished");
    }

    /// <summary>
    ///     Return an object containing all items in the game with a generated name
    /// </summary>
    /// <returns>An object containing a generated item name to item ID association</returns>
    private Dictionary<string, string> GenerateItemsObject()
    {
        var itemsObject = new Dictionary<string, string>();
        foreach (var item in items.Values)
        {
            // Skip invalid items (Non-Item types, and shrapnel)
            if (!IsValidItem(item))
            {
                continue;
            }

            var itemParentName = GetParentName(item);
            var itemPrefix = GetItemPrefix(item);
            var itemName = GetItemName(item);
            var itemSuffix = GetItemSuffix(item);

            // Handle the case where the item starts with the parent category name. Avoids things like 'POCKETS_POCKETS'
            if (itemName.Length > itemParentName.Length &&
                itemParentName == itemName.Substring(1, itemParentName.Length) &&
                itemPrefix == "")
            {
                itemName = itemName.Substring(itemParentName.Length + 1);
                if (itemName.Length > 0 && itemName[0] != '_')
                {
                    itemName = $"_{itemName}";
                }
            }

            // Handle the case where the item ends with the parent category name. Avoids things like 'KEY_DORM_ROOM_103_KEY'
            if (itemName.Length >= itemParentName.Length &&
                itemParentName == itemName.Substring(itemName.Length - itemParentName.Length))
            {
                itemName = itemName.Substring(0, itemName.Length - itemParentName.Length);

                if (itemName.Substring(itemName.Length - 1) == "_")
                {
                    itemName = itemName.Substring(0, itemName.Length - 1);
                }
            }

            var itemKey = $"{itemParentName}{itemPrefix}{itemName}{itemSuffix}";

            // Strip out any remaining special characters
            itemKey = SanitizeEnumKey(itemKey);

            // If the key already exists, see if we can add a suffix to both this, and the existing conflicting item
            if (itemsObject.ContainsKey(itemKey) || collidedEnumKeys.Contains(itemKey))
            {
                // Keep track, so we can handle 3+ conflicts
                collidedEnumKeys.Add(itemKey);

                var itemNameSuffix = GetItemNameSuffix(item);
                if (!string.IsNullOrEmpty(itemNameSuffix))
                {
                    // Try to update the old key reference if we haven't already
                    if (itemsObject.ContainsKey(itemKey))
                    {
                        var oldItemId = itemsObject[itemKey];
                        var oldItemNameSuffix = GetItemNameSuffix(items[oldItemId]);
                        if (!string.IsNullOrEmpty(oldItemNameSuffix))
                        {
                            var oldItemNewKey = SanitizeEnumKey($"{itemKey}_{oldItemNameSuffix}");
                            itemsObject.Remove(itemKey);
                            itemsObject[oldItemNewKey] = oldItemId;
                        }
                    }

                    itemKey = SanitizeEnumKey($"{itemKey}_{itemNameSuffix}");

                    // If we still collide, log an error
                    if (itemsObject.ContainsKey(itemKey))
                    {
                        _logger.Error(
                            $"After rename, itemsObject already contains {itemKey}  {itemsObject[itemKey]} => {item.Id}"
                        );
                    }
                }
                else
                {
                    var val = itemsObject.ContainsKey(itemKey) ? itemsObject[itemKey] : itemKey;
                    _logger.Error($"New itemOverride entry required: itemsObject already contains {itemKey}  {val} => {item.Id}");
                    continue;
                }
            }

            itemsObject[itemKey] = item.Id;
        }

        // Sort the items object
        var itemList = itemsObject.ToList();
        itemList.Sort((kv1, kv2) => kv1.Key.CompareTo(kv2.Key));
        var orderedItemsObject = itemList.ToDictionary(kv => kv.Key, kv => kv.Value);
        return orderedItemsObject;
    }

    private Dictionary<string, string> GenerateWeaponsObject()
    {
        var weaponsObject = new Dictionary<string, string>();
        foreach (var kv /*[itemId, item]*/ in items)
        {
            if (!_itemHelper.IsOfBaseclass(kv.Key, BaseClasses.WEAPON))
            {
                continue;
            }

            var caliber = CleanCaliber(kv.Value.Properties.AmmoCaliber.ToUpper());
            var weaponShortName = _localeService.GetLocaleDb()[$"{kv.Key} ShortName"]?.ToUpper();

            // Special case for the weird duplicated grenade launcher
            if (kv.Key == "639c3fbbd0446708ee622ee9")
            {
                weaponShortName = "FN40GL_2";
            }

            // Include any bracketed suffixes that exist, handles the case of colored gun variants
            var weaponFullName = _localeService.GetLocaleDb()[$"{kv.Key} Name"]?.ToUpper();
            if (weaponFullName.RegexMatch(@"\((.+?)\)$", out var itemNameBracketSuffix) &&
                !weaponShortName.EndsWith(itemNameBracketSuffix.Groups[1].Value))
            {
                weaponShortName += $"_{itemNameBracketSuffix.Groups[1].Value}";
            }

            var parentName = GetParentName(kv.Value);

            // Handle special characters
            var weaponName = $"{parentName}_{caliber}_{weaponShortName}"
                .RegexReplace("[- ]", "_")
                .RegexReplace("[^a-zA-Z0-9_]", "")
                .ToUpper();

            if (weaponsObject.ContainsKey(weaponName))
            {
                _logger.Error($"weapon {weaponName} already exists");
                continue;
            }

            weaponsObject[weaponName] = kv.Key;
        }

        // Sort the weapons object
        var itemList = weaponsObject.ToList();
        itemList.Sort((kv1, kv2) => kv1.Key.CompareTo(kv2.Key));
        var orderedWeaponsObject = itemList.ToDictionary(kv => kv.Key, kv => kv.Value);
        return orderedWeaponsObject;
    }

    /// <summary>
    ///     Clear any non-alpha numeric characters, and fix multiple underscores
    /// </summary>
    /// <param name="enumKey">The enum key to sanitize</param>
    /// <returns>The sanitized enum key</returns>
    private string SanitizeEnumKey(string enumKey)
    {
        return enumKey
            .ToUpper()
            .RegexReplace("[^A-Z0-9_]", "")
            .RegexReplace("_+", "_");
    }

    private string GetParentName(TemplateItem item)
    {
        if (item.Properties?.QuestItem is true)
        {
            return "QUEST";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.BARTER_ITEM))
        {
            return "BARTER";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.THROW_WEAPON))
        {
            return "GRENADE";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.STIMULATOR))
        {
            return "STIM";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.MAGAZINE))
        {
            return "MAGAZINE";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.KEY_MECHANICAL))
        {
            return "KEY";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.MOB_CONTAINER))
        {
            return "SECURE";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.SIMPLE_CONTAINER))
        {
            return "CONTAINER";
        }

        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.PORTABLE_RANGE_FINDER))
        {
            return "RANGEFINDER";
        }

        // Why are flares grenade launcher...?
        if (item.Name.StartsWith("weapon_rsp30"))
        {
            return "FLARE";
        }

        // This is a special case for the signal pistol, I'm not adding it as a Grenade Launcher
        if (item.Id == "620109578d82e67e7911abf2")
        {
            return "SIGNALPISTOL";
        }

        var parentId = item.Parent;
        return items[parentId].Name.ToUpper();
    }

    private bool IsValidItem(TemplateItem item)
    {
        const string shrapnelId = "5943d9c186f7745a13413ac9";

        if (item.Type != "Item")
        {
            return false;
        }

        if (item.Prototype == shrapnelId)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Generate a prefix for the passed in item
    /// </summary>
    /// <param name="item">The item to generate the prefix for</param>
    /// <returns>The prefix of the given item</returns>
    private string GetItemPrefix(TemplateItem item)
    {
        var prefix = "";

        // Prefix ammo with its caliber
        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.AMMO))
        {
            prefix = GetAmmoPrefix(item);
        }
        // Prefix ammo boxes with their caliber
        else if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.AMMO_BOX))
        {
            prefix = GetAmmoBoxPrefix(item);
        }
        // Prefix magazines with their caliber
        else if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.MAGAZINE))
        {
            prefix = GetMagazinePrefix(item);
        }

        // Make sure there's an underscore separator
        if (prefix.Length > 0 && prefix[0] != '_')
        {
            prefix = $"_{prefix}";
        }

        return prefix;
    }

    private string GetItemSuffix(TemplateItem item)
    {
        var suffix = "";

        // Add mag size for magazines
        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.MAGAZINE))
        {
            suffix = $"{item.Properties?.Cartridges?[0].MaxCount?.ToString()}RND";
        }
        // Add pack size for ammo boxes
        else if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.AMMO_BOX))
        {
            suffix = $"{item.Properties.StackSlots[0]?.MaxCount.ToString()}RND";
        }

        // Add "DAMAGED" for damaged items
        if (item.Name.ToLower().Contains("damaged"))
        {
            suffix += "_DAMAGED";
        }

        // Make sure there's an underscore separator
        if (suffix.Length > 0 && suffix[0] != '_')
        {
            suffix = $"_{suffix}";
        }

        return suffix;
    }

    private string GetAmmoPrefix(TemplateItem item)
    {
        var prefix = item.Properties.Caliber.ToUpper();

        return CleanCaliber(prefix);
    }

    private string CleanCaliber(string ammoCaliber)
    {
        var ammoCaliberToClean = ammoCaliber;

        ammoCaliberToClean = ammoCaliberToClean.Replace("CALIBER", "");
        ammoCaliberToClean = ammoCaliberToClean.Replace("PARA", "");
        ammoCaliberToClean = ammoCaliberToClean.Replace("NATO", "");

        // Special case for 45ACP
        ammoCaliberToClean = ammoCaliberToClean.Replace("1143X23ACP", "45ACP");

        return ammoCaliberToClean;
    }

    private string GetAmmoBoxPrefix(TemplateItem item)
    {
        var ammoItem = item.Properties?.StackSlots?[0]?.Props?.Filters?[0]?.Filter?.FirstOrDefault();

        return GetAmmoPrefix(items[ammoItem]);
    }

    private string GetMagazinePrefix(TemplateItem item)
    {
        var ammoItem = item.Properties?.Cartridges?[0]?.Props?.Filters?[0]?.Filter?.FirstOrDefault();

        return GetAmmoPrefix(items[ammoItem]);
    }

    /// <summary>
    ///     Return the name of the passed in item, formatted for use in an enum
    /// </summary>
    /// <param name="item">The item to generate the name for</param>
    /// <returns>The name of the given item</returns>
    private string GetItemName(TemplateItem item)
    {
        string? itemName = null;
        var localeDb = _localeService.GetLocaleDb();

        // Manual item name overrides
        if (itemOverrides.ContainsKey(item.Id))
        {
            itemName = itemOverrides[item.Id].ToUpper();
        }
        // For the listed types, user the item's _name property
        else if (
            _itemHelper.IsOfBaseclasses(
                item.Id,
                [
                    BaseClasses.RANDOM_LOOT_CONTAINER,
                    BaseClasses.BUILT_IN_INSERTS,
                    BaseClasses.STASH
                ]
            )
        )
        {
            itemName = item.Name.ToUpper();
        }
        // For the listed types, use the short name
        else if (_itemHelper.IsOfBaseclasses(item.Id, [BaseClasses.AMMO, BaseClasses.AMMO_BOX, BaseClasses.MAGAZINE]))
        {
            if (localeDb.TryGetValue($"{item.Id} ShortName", out itemName))
            {
                itemName = itemName.ToUpper();
            }
        }
        // For everything else, use the full name
        else
        {
            if (localeDb.TryGetValue($"{item.Id} Name", out itemName))
            {
                itemName = itemName.ToUpper();
            }
        }

        // Fall back in the event we couldn't find a name
        if (string.IsNullOrEmpty(itemName))
        {
            if (localeDb.TryGetValue($"{item.Id} Name", out itemName))
            {
                itemName = itemName.ToUpper();
            }
        }

        if (string.IsNullOrEmpty(itemName))
        {
            itemName = item.Name?.ToUpper() ?? null;
        }

        if (string.IsNullOrEmpty(itemName))
        {
            Console.WriteLine($"Unable to get shortname for {item.Id}");
            return "";
        }

        itemName = itemName.Trim().RegexReplace("[-.()]", "");
        itemName = itemName.RegexReplace("[ ]", "_");

        return $"_{itemName}";
    }

    private string? GetItemNameSuffix(TemplateItem item)
    {
        var localeDb = _localeService.GetLocaleDb();
        localeDb.TryGetValue($"{item.Id} Name", out var itemName);

        // Add grid size for lootable containers
        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.LOOT_CONTAINER))
        {
            return $"{item.Properties.Grids[0]?.Props.CellsH}X{item.Properties.Grids[0]?.Props.CellsV}";
        }

        // Add ammo caliber to conflicting weapons
        if (_itemHelper.IsOfBaseclass(item.Id, BaseClasses.WEAPON))
        {
            var caliber = CleanCaliber(item.Properties.AmmoCaliber.ToUpper());

            // If the item has a bracketed section at the end of its name, include that
            if (itemName?.RegexMatch(@"\((.+?)\)$", out var itemNameBracketSuffix) ?? false)
            {
                return $"{caliber}_{itemNameBracketSuffix.Groups[1].Value}";
            }

            return caliber;
        }

        // Make sure we have a full name
        if (string.IsNullOrEmpty(itemName))
        {
            return "";
        }

        // If the item has a bracketed section at the end of its name, use that
        if (itemName.RegexMatch(@"\((.+?)\)$", out var itemNameBracker))
        {
            return itemNameBracker.Groups[1].Value;
        }

        // If the item has a number at the end of its name, use that
        if (itemName.RegexMatch("#([0-9]+)$", out var itemNameNumberSuffix))
        {
            return itemNameNumberSuffix.Groups[1].Value;
        }

        return "";
    }

    private void LogEnumValueChanges(Dictionary<string, string> data, string enumName, Type originalEnum)
    {
        // First generate a mapping of the original enum values to names
        var originalEnumValues = new Dictionary<string, string>();
        foreach (var field in originalEnum.GetFields())
        {
            originalEnumValues.Add(field.GetValue(null)!.ToString()!, field.Name);
        }

        // Loop through our new data, and find any where the given ID's name doesn't match the original enum
        foreach (var kv in data)
        {
            if (originalEnumValues.ContainsKey(kv.Value) && originalEnumValues[kv.Value] != kv.Key)
            {
                _logger.Warning(
                    $"Enum {enumName} key has changed for {kv.Value}, {originalEnumValues[kv.Value]} => {kv.Key}"
                );
            }
        }
    }

    private void WriteEnumsToFile(string outputPath, Dictionary<string, Dictionary<string, string>> enumEntries)
    {
        var enumFileData =
            "// This is an auto generated file, do not modify. Re-generate by running ItemTplGenerator.exe";

        foreach (var (enumName, data) in enumEntries)
        {
            enumFileData += $"\npublic static class {enumName}\n{{\n";

            foreach (var (key, value) in data)
            {
                enumFileData += $"    public const string {key} = \"{value}\";\n";
            }

            enumFileData += "}\n";
        }

        _fileUtil.WriteFile(outputPath, enumFileData);
    }
}
