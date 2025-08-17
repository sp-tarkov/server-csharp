using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class BotGeneratorHelper(
    ISptLogger<BotGeneratorHelper> logger,
    RandomUtil randomUtil,
    DurabilityLimitsHelper durabilityLimitsHelper,
    ItemHelper itemHelper,
    InventoryHelper inventoryHelper,
    ProfileActivityService profileActivityService,
    ServerLocalisationService serverLocalisationService,
    BotInventoryContainerService botInventoryContainerService,
    ConfigServer configServer
)
{
    // Equipment slot ids that do not conflict with other slots
    private static readonly FrozenSet<string> _slotsWithNoCompatIssues =
    [
        nameof(EquipmentSlots.Scabbard),
        nameof(EquipmentSlots.Backpack),
        nameof(EquipmentSlots.SecuredContainer),
        nameof(EquipmentSlots.Holster),
        nameof(EquipmentSlots.ArmBand),
    ];

    private static readonly FrozenSet<string> _pmcTypes = [Sides.PmcBear.ToLowerInvariant(), Sides.PmcUsec.ToLowerInvariant()];

    private readonly BotConfig _botConfig = configServer.GetConfig<BotConfig>();

    /// <summary>
    ///     Adds properties to an item
    ///     e.g. Repairable / HasHinge / Foldable / MaxDurability
    /// </summary>
    /// <param name="itemTemplate">Item extra properties are being generated for</param>
    /// <param name="botRole">Used by weapons to randomize the durability values. Null for non-equipped items</param>
    /// <returns>Item Upd object with extra properties</returns>
    public Upd GenerateExtraPropertiesForItem(TemplateItem? itemTemplate, string? botRole = null)
    {
        // Get raid settings, if no raid, default to day
        var raidSettings = profileActivityService.GetFirstProfileActivityRaidData()?.RaidConfiguration;

        RandomisedResourceDetails? randomisationSettings = null;
        if (botRole is not null)
        {
            _botConfig.LootItemResourceRandomization.TryGetValue(botRole, out randomisationSettings);
        }

        Upd itemProperties = new();
        var hasProperties = false;

        if (itemTemplate?.Properties?.MaxDurability is not null && itemTemplate.Properties.MaxDurability > 0)
        {
            if (itemTemplate.Properties.WeapClass is not null)
            {
                // Is weapon
                itemProperties.Repairable = GenerateWeaponRepairableProperties(itemTemplate, botRole);
                hasProperties = true;
            }
            else if (itemTemplate.Properties.ArmorClass is not null)
            {
                // Is armor
                itemProperties.Repairable = GenerateArmorRepairableProperties(itemTemplate, botRole);
                hasProperties = true;
            }
        }

        if (itemTemplate?.Properties?.HasHinge ?? false)
        {
            itemProperties.Togglable = new UpdTogglable { On = true };
            hasProperties = true;
        }

        if (itemTemplate?.Properties?.Foldable ?? false)
        {
            itemProperties.Foldable = new UpdFoldable { Folded = false };
            hasProperties = true;
        }

        if (itemTemplate?.Properties?.WeapFireType?.Count == 0)
        {
            itemProperties.FireMode = itemTemplate.Properties.WeapFireType.Contains("fullauto")
                ? new UpdFireMode { FireMode = "fullauto" }
                : new UpdFireMode { FireMode = randomUtil.GetArrayValue(itemTemplate.Properties.WeapFireType) };
            hasProperties = true;
        }

        if (itemTemplate?.Properties?.MaxHpResource is not null)
        {
            itemProperties.MedKit = new UpdMedKit
            {
                HpResource = GetRandomizedResourceValue(itemTemplate.Properties.MaxHpResource ?? 0, randomisationSettings?.Meds),
            };
            hasProperties = true;
        }

        if (itemTemplate?.Properties?.MaxResource is not null && itemTemplate.Properties?.FoodUseTime is not null)
        {
            itemProperties.FoodDrink = new UpdFoodDrink
            {
                HpPercent = GetRandomizedResourceValue(itemTemplate.Properties.MaxResource ?? 0, randomisationSettings?.Food),
            };
            hasProperties = true;
        }

        var equipmentSettings = GetBotEquipmentSettingFromConfig(botRole);
        if (itemTemplate?.Parent == BaseClasses.FLASHLIGHT)
        {
            var lightLaserActiveChance =
                raidSettings?.IsNightRaid ?? false // Higher chance of laser/light at night
                    ? equipmentSettings?.LightIsActiveNightChancePercent ?? 50
                    : equipmentSettings?.LightIsActiveDayChancePercent ?? 25;

            itemProperties.Light = new UpdLight { IsActive = randomUtil.GetChance100(lightLaserActiveChance), SelectedMode = 0 };
            hasProperties = true;
        }
        else if (itemTemplate?.Parent == BaseClasses.TACTICAL_COMBO)
        {
            // Get chance from botconfig for bot type, use 50% if no value found
            var lightLaserActiveChance = equipmentSettings?.LaserIsActiveChancePercent ?? 50;

            itemProperties.Light = new UpdLight { IsActive = randomUtil.GetChance100(lightLaserActiveChance), SelectedMode = 0 };
            hasProperties = true;
        }

        if (itemTemplate?.Parent == BaseClasses.NIGHT_VISION)
        {
            // Get chance from botconfig for bot type
            var nvgActiveChance =
                raidSettings?.IsNightRaid ?? false
                    ? equipmentSettings?.NvgIsActiveChanceNightPercent ?? 90
                    : equipmentSettings?.NvgIsActiveChanceDayPercent ?? 15;

            itemProperties.Togglable = new UpdTogglable { On = randomUtil.GetChance100(nvgActiveChance) };
            hasProperties = true;
        }

        // Togglable face shield
        if ((itemTemplate?.Properties?.HasHinge ?? false) && (itemTemplate.Properties.FaceShieldComponent ?? false))
        {
            var faceShieldActiveChance = equipmentSettings?.FaceShieldIsActiveChancePercent ?? 75;
            itemProperties.Togglable = new UpdTogglable { On = randomUtil.GetChance100(faceShieldActiveChance) };
            hasProperties = true;
        }

        // Get chance from botconfig for bot type, use 75% if no value found
        return hasProperties ? itemProperties : null;
    }

    /// <summary>
    ///     Randomize the HpResource for bots e.g (245/400 resources)
    /// </summary>
    /// <param name="maxResource">Max resource value of medical items</param>
    /// <param name="randomizationValues">Value provided from config</param>
    /// <returns>Randomized value from maxHpResource</returns>
    protected double GetRandomizedResourceValue(double maxResource, RandomisedResourceValues? randomizationValues)
    {
        if (randomizationValues is null)
        {
            return maxResource;
        }

        if (randomUtil.GetChance100(randomizationValues.ChanceMaxResourcePercent))
        {
            return maxResource;
        }

        return randomUtil.GetDouble(randomUtil.GetPercentOfValue(randomizationValues.ResourcePercent, maxResource, 0), maxResource);
    }

    /// <summary>
    /// Get equipment specific flags (e.g. nvg settings) for a particular bot type
    /// </summary>
    /// <param name="botRole">bot to get settings for</param>
    /// <returns>Equipment filter settings</returns>
    protected EquipmentFilters? GetBotEquipmentSettingFromConfig(string botRole)
    {
        return _botConfig.Equipment.GetValueOrDefault(GetBotEquipmentRole(botRole));
    }

    /// <summary>
    ///     Create a repairable object for a weapon that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    protected UpdRepairable GenerateWeaponRepairableProperties(TemplateItem itemTemplate, string? botRole = null)
    {
        var maxDurability = durabilityLimitsHelper.GetRandomizedMaxWeaponDurability(botRole);
        var currentDurability = durabilityLimitsHelper.GetRandomizedWeaponDurability(botRole, maxDurability);

        return new UpdRepairable { Durability = Math.Round(currentDurability, 5), MaxDurability = Math.Round(maxDurability, 5) };
    }

    /// <summary>
    ///     Create a repairable object for an armor that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    protected UpdRepairable GenerateArmorRepairableProperties(TemplateItem itemTemplate, string? botRole = null)
    {
        double maxDurability;
        double currentDurability;
        if (itemTemplate.Properties?.ArmorClass == 0)
        {
            maxDurability = itemTemplate.Properties.MaxDurability.Value;
            currentDurability = itemTemplate.Properties.MaxDurability.Value;
        }
        else
        {
            maxDurability = durabilityLimitsHelper.GetRandomizedMaxArmorDurability(itemTemplate, botRole);
            currentDurability = durabilityLimitsHelper.GetRandomizedArmorDurability(itemTemplate, botRole, maxDurability);
        }

        return new UpdRepairable { Durability = Math.Round(currentDurability, 5), MaxDurability = Math.Round(maxDurability, 5) };
    }

    /// <summary>
    ///     Can item be added to another item without conflict
    /// </summary>
    /// <param name="itemsEquipped">Items to check compatibilities with</param>
    /// <param name="tplToCheck">Tpl of the item to check for incompatibilities</param>
    /// <param name="equipmentSlot">Slot the item will be placed into</param>
    /// <returns>false if no incompatibilities, also has incompatibility reason</returns>
    public ChooseRandomCompatibleModResult IsItemIncompatibleWithCurrentItems(
        IEnumerable<Item> itemsEquipped,
        MongoId tplToCheck,
        string equipmentSlot
    )
    {
        // Skip slots that have no incompatibilities
        if (_slotsWithNoCompatIssues.Contains(equipmentSlot))
        {
            return new ChooseRandomCompatibleModResult
            {
                Incompatible = false,
                Found = false,
                Reason = string.Empty,
            };
        }

        // TODO: Can probably be optimized to cache itemTemplates as items are added to inventory
        var equippedItemsDb = itemsEquipped.Select(equippedItem => itemHelper.GetItem(equippedItem.Template).Value);

        var (itemIsValid, itemToEquip) = itemHelper.GetItem(tplToCheck);

        if (!itemIsValid)
        {
            logger.Warning(
                serverLocalisationService.GetText(
                    "bot-invalid_item_compatibility_check",
                    new { itemTpl = tplToCheck, slot = equipmentSlot }
                )
            );

            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"item: {tplToCheck} does not exist in the database",
            };
        }

        if (itemToEquip?.Properties is null)
        {
            logger.Warning(
                serverLocalisationService.GetText(
                    "bot-compatibility_check_missing_props",
                    new
                    {
                        id = itemToEquip?.Id,
                        name = itemToEquip?.Name,
                        slot = equipmentSlot,
                    }
                )
            );

            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"item: {tplToCheck} does not have a _props field",
            };
        }

        // Does an equipped item have a property that blocks the desired item - check for prop "BlocksX" .e.g BlocksEarpiece / BlocksFaceCover
        var templateItems = equippedItemsDb;
        var blockingItem = templateItems.FirstOrDefault(item => HasBlockingProperty(item, equipmentSlot));
        if (blockingItem is not null)
        // this.logger.warning(`1 incompatibility found between - {itemToEquip[1]._name} and {blockingItem._name} - {equipmentSlot}`);
        {
            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"{tplToCheck} {itemToEquip.Name} in slot: {equipmentSlot} blocked by: {blockingItem.Id} {blockingItem.Name}",
                SlotBlocked = true,
            };
        }

        // Check if any of the current inventory templates have the incoming item defined as incompatible
        blockingItem = templateItems.FirstOrDefault(x => x?.Properties?.ConflictingItems?.Contains(tplToCheck) ?? false);
        if (blockingItem is not null)
        // this.logger.warning(`2 incompatibility found between - {itemToEquip[1]._name} and {blockingItem._props.Name} - {equipmentSlot}`);
        {
            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"{tplToCheck} {itemToEquip.Name} in slot: {equipmentSlot} blocked by: {blockingItem.Id} {blockingItem.Name}",
                SlotBlocked = true,
            };
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksHeadwear ?? false)
        {
            var existingHeadwear = itemsEquipped.FirstOrDefault(x => x.SlotId == Containers.Headwear);
            if (existingHeadwear is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason =
                        $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingHeadwear.Template} in slot: {existingHeadwear.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksFaceCover.GetValueOrDefault(false))
        {
            var existingFaceCover = itemsEquipped.FirstOrDefault(item => item.SlotId == Containers.FaceCover);
            if (existingFaceCover is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason =
                        $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingFaceCover.Template} in slot: {existingFaceCover.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksEarpiece.GetValueOrDefault(false))
        {
            var existingEarpiece = itemsEquipped.FirstOrDefault(item => item.SlotId == Containers.Earpiece);
            if (existingEarpiece is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason =
                        $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingEarpiece.Template} in slot: {existingEarpiece.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksArmorVest.GetValueOrDefault(false))
        {
            var existingArmorVest = itemsEquipped.FirstOrDefault(item => item.SlotId == Containers.ArmorVest);
            if (existingArmorVest is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason =
                        $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingArmorVest.Template} in slot: {existingArmorVest.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Check if the incoming item has any inventory items defined as incompatible
        var blockingInventoryItem = itemsEquipped.FirstOrDefault(x =>
            itemToEquip.Properties.ConflictingItems?.Contains(x.Template) ?? false
        );
        if (blockingInventoryItem is not null)
        // this.logger.warning(`3 incompatibility found between - {itemToEquip[1]._name} and {blockingInventoryItem._tpl} - {equipmentSlot}`)
        {
            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"{tplToCheck} blocks existing item {blockingInventoryItem.Template} in slot {blockingInventoryItem.SlotId}",
            };
        }

        return new ChooseRandomCompatibleModResult { Incompatible = false, Reason = string.Empty };
    }

    protected bool HasBlockingProperty(TemplateItem? item, string blockingPropertyName)
    {
        return item != null && item.Blocks.TryGetValue(blockingPropertyName, out var blocks) && blocks;
    }

    /// <summary>
    ///     Convert a bots role to the equipment role used in config/bot.json
    /// </summary>
    /// <param name="botRole">Role to convert</param>
    /// <returns>Equipment role (e.g. pmc / assault / bossTagilla)</returns>
    public string GetBotEquipmentRole(string botRole)
    {
        return _pmcTypes.Contains(botRole.ToLower()) ? Sides.PmcEquipmentRole : botRole;
    }

    /// <summary>
    ///     Adds an item with all its children into specified equipmentSlots, wherever it fits
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="equipmentSlots">Slot to try and add item+children into</param>
    /// <param name="rootItemId">Root item id to use as mod items parentId</param>
    /// <param name="rootItemTplId">Root items tpl id</param>
    /// <param name="itemWithChildren">Item to add</param>
    /// <param name="inventory">Inventory to add item+children into</param>
    /// <param name="containersIdFull">Container Ids with no space for more items</param>
    /// <returns>ItemAddedResult result object</returns>
    public ItemAddedResult AddItemWithChildrenToEquipmentSlot(
        MongoId botId,
        HashSet<EquipmentSlots> equipmentSlots,
        MongoId rootItemId,
        MongoId rootItemTplId,
        IEnumerable<Item> itemWithChildren,
        BotBaseInventory inventory,
        HashSet<string>? containersIdFull = null
    )
    {
        var itemWithChildrenList = itemWithChildren.ToList();

        // Track how many containers are unable to be found
        var missingContainerCount = 0;
        foreach (var equipmentSlotId in equipmentSlots)
        {
            if (containersIdFull is not null && containersIdFull.Contains(equipmentSlotId.ToString()))
            {
                // Container has been flagged as full already, skip trying to add item into it
                continue;
            }

            // Get container from inventory to put item into
            var container = inventory.Items?.FirstOrDefault(item => item.SlotId == equipmentSlotId.ToString());
            if (container is null)
            {
                missingContainerCount++;
                if (missingContainerCount == equipmentSlots.Count)
                {
                    // Bot doesn't have any containers we want to add item to
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            $"Unable to add item: {itemWithChildrenList.FirstOrDefault()?.Template} to bot as it lacks the following containers: {string.Join(",", equipmentSlots)}"
                        );
                    }

                    return ItemAddedResult.NO_CONTAINERS;
                }

                // No container of desired type found, skip to next container type
                continue;
            }

            // Get container details from db
            var (isValidItem, containerDbDetails) = itemHelper.GetItem(container.Template);
            if (!isValidItem)
            {
                logger.Warning(serverLocalisationService.GetText("bot-missing_container_with_tpl", container.Template));

                // Bad item, skip
                continue;
            }

            if (containerDbDetails?.Properties?.Grids is null || !containerDbDetails.Properties.Grids.Any())
            {
                // Container has no slots to hold items, skip to next container
                continue;
            }

            // Get x/y grid size of item
            var (itemWidth, itemHeight) = inventoryHelper.GetItemSize(rootItemTplId, rootItemId, itemWithChildrenList);

            var result = botInventoryContainerService.AddItemToBotContainer(
                botId,
                equipmentSlotId,
                itemWithChildrenList,
                inventory,
                itemWidth,
                itemHeight
            );
            if (result != ItemAddedResult.SUCCESS)
            {
                // Failed to add to container, try next
                continue;
            }

            return result;
        }

        return ItemAddedResult.NO_SPACE;
    }
}
