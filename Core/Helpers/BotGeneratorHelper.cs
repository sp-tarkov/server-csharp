using Core.Annotations;
using Core.Context;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Helpers;

[Injectable]
public class BotGeneratorHelper
{
    private readonly ISptLogger<BotGeneratorHelper> _logger;
    private readonly RandomUtil _randomUtil;
    private readonly DurabilityLimitsHelper _durabilityLimitsHelper;
    private readonly ItemHelper _itemHelper;
    private readonly InventoryHelper _inventoryHelper;
    private readonly ContainerHelper _containerHelper;
    private readonly ApplicationContext _applicationContext;
    private readonly LocalisationService _localisationService;
    private readonly ConfigServer _configServer;

    private readonly BotConfig _botConfig;
    private readonly PmcConfig _pmcConfig;

    public BotGeneratorHelper
    (
        ISptLogger<BotGeneratorHelper> logger,
        RandomUtil randomUtil,
        DurabilityLimitsHelper durabilityLimitsHelper,
        ItemHelper itemHelper,
        InventoryHelper inventoryHelper,
        ContainerHelper containerHelper,
        ApplicationContext applicationContext,
        LocalisationService localisationService,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _durabilityLimitsHelper = durabilityLimitsHelper;
        _itemHelper = itemHelper;
        _inventoryHelper = inventoryHelper;
        _containerHelper = containerHelper;
        _applicationContext = applicationContext;
        _localisationService = localisationService;
        _configServer = configServer;

        _botConfig = _configServer.GetConfig<BotConfig>();
        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    /// <summary>
    /// Adds properties to an item
    /// e.g. Repairable / HasHinge / Foldable / MaxDurability
    /// </summary>
    /// <param name="itemTemplate">Item extra properties are being generated for</param>
    /// <param name="botRole">Used by weapons to randomize the durability values. Null for non-equipped items</param>
    /// <returns>Item Upd object with extra properties</returns>
    public Upd GenerateExtraPropertiesForItem(TemplateItem? itemTemplate, string? botRole = null)
    {
        // Get raid settings, if no raid, default to day
        var raidSettings = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();
        var raidIsNight = raidSettings?.TimeVariant == DateTimeEnum.PAST;

        Upd itemProperties = new();

        if (itemTemplate?.Properties?.MaxDurability is not null)
        {
            if (itemTemplate.Properties.WeapClass is not null)
            {
                // Is weapon
                itemProperties.Repairable = GenerateWeaponRepairableProperties(itemTemplate, botRole);
            }
            else if (itemTemplate.Properties.ArmorClass is not null)
            {
                // Is armor
                itemProperties.Repairable = GenerateArmorRepairableProperties(itemTemplate, botRole);
            }
        }

        if (itemTemplate?.Properties?.HasHinge ?? false)
        {
            itemProperties.Togglable = new UpdTogglable { On = true };
        }

        if (itemTemplate?.Properties?.Foldable ?? false)
        {
            itemProperties.Foldable = new UpdFoldable { Folded = false };
        }

        if (itemTemplate?.Properties?.WeapFireType?.Count == 0)
        {
            itemProperties.FireMode = itemTemplate.Properties.WeapFireType.Contains("fullauto") 
                ? new UpdFireMode { FireMode = "fullauto" } 
                : new UpdFireMode { FireMode = _randomUtil.GetArrayValue(itemTemplate.Properties.WeapFireType) };
        }

        if (itemTemplate?.Properties?.MaxHpResource is not null)
        {
            itemProperties.MedKit = new UpdMedKit
            {
                HpResource = GetRandomizedResourceValue(
                    itemTemplate.Properties.MaxHpResource ?? 0,
                    _botConfig.LootItemResourceRandomization[botRole ?? string.Empty].Meds
                )
            };
        }

        if (itemTemplate?.Properties?.MaxResource is not null && itemTemplate.Properties?.FoodUseTime is not null)
        {
            itemProperties.FoodDrink = new UpdFoodDrink
            {
                HpPercent = GetRandomizedResourceValue(
                    itemTemplate.Properties.MaxResource ?? 0,
                    _botConfig.LootItemResourceRandomization[botRole ?? string.Empty].Food
                ),
            };
        }

        if (itemTemplate?.Parent == BaseClasses.FLASHLIGHT)
        {
            // Get chance from botconfig for bot type
            var lightLaserActiveChance = raidIsNight
                ? GetBotEquipmentSettingFromConfig(botRole, "lightIsActiveNightChancePercent", 50)
                : GetBotEquipmentSettingFromConfig(botRole, "lightIsActiveDayChancePercent", 25);
            itemProperties.Light = new UpdLight { IsActive = _randomUtil.GetChance100(lightLaserActiveChance), SelectedMode = 0, };
        }
        else if (itemTemplate?.Parent == BaseClasses.TACTICAL_COMBO)
        {
            // Get chance from botconfig for bot type, use 50% if no value found
            var lightLaserActiveChance = GetBotEquipmentSettingFromConfig(
                botRole,
                "laserIsActiveChancePercent",
                50
            );
            itemProperties.Light = new UpdLight
            {
                IsActive = _randomUtil.GetChance100(lightLaserActiveChance),
                SelectedMode = 0,
            };
        }

        if (itemTemplate?.Parent == BaseClasses.NIGHTVISION)
        {
            // Get chance from botconfig for bot type
            var nvgActiveChance = raidIsNight
                ? GetBotEquipmentSettingFromConfig(botRole, "nvgIsActiveChanceNightPercent", 90)
                : GetBotEquipmentSettingFromConfig(botRole, "nvgIsActiveChanceDayPercent", 15);
            itemProperties.Togglable = new UpdTogglable { On = _randomUtil.GetChance100(nvgActiveChance) };
        }

        // Togglable face shield
        if (!(itemTemplate?.Properties?.HasHinge ?? false) || !(itemTemplate.Properties.FaceShieldComponent ?? false)) return itemProperties;
        
        // Get chance from botconfig for bot type, use 75% if no value found
        var faceShieldActiveChance = GetBotEquipmentSettingFromConfig(
            botRole,
            "faceShieldIsActiveChancePercent",
            75
        );
        itemProperties.Togglable = new UpdTogglable { On = _randomUtil.GetChance100(faceShieldActiveChance) };

        return itemProperties;
    }

    /// <summary>
    /// Randomize the HpResource for bots e.g (245/400 resources)
    /// </summary>
    /// <param name="maxResource">Max resource value of medical items</param>
    /// <param name="randomizationValues">Value provided from config</param>
    /// <returns>Randomized value from maxHpResource</returns>
    private double GetRandomizedResourceValue(double maxResource, RandomisedResourceValues? randomizationValues)
    {
        if (randomizationValues is null)
        {
            return maxResource;
        }

        if (_randomUtil.GetChance100(randomizationValues.ChanceMaxResourcePercent))
        {
            return maxResource;
        }

        return _randomUtil.GetInt(
            (int)_randomUtil.GetPercentOfValue(randomizationValues.ResourcePercent, maxResource, 0),
            (int)maxResource
        );
    }

    /// <summary>
    /// Get the chance for the weapon attachment or helmet equipment to be set as activated
    /// </summary>
    /// <param name="botRole">role of bot with weapon/helmet</param>
    /// <param name="setting">the setting of the weapon attachment/helmet equipment to be activated</param>
    /// <param name="defaultValue">default value for the chance of activation if the botrole or bot equipment role is undefined</param>
    /// <returns>Percent chance to be active</returns>
    private double? GetBotEquipmentSettingFromConfig(string? botRole, string setting, double defaultValue)
    {
        if (botRole is null)
        {
            return defaultValue;
        }

        var botEquipmentSettings = _botConfig.Equipment[GetBotEquipmentRole(botRole)];
        if (botEquipmentSettings is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-missing_equipment_settings",
                    new
                    {
                        botRole,
                        setting,
                        defaultValue
                    }
                )
            );

            return defaultValue;
        }

        var props = botEquipmentSettings.GetType().GetProperties();
        var propValue = (double?)props.FirstOrDefault(x => string.Equals(x.Name, setting, StringComparison.CurrentCultureIgnoreCase))
            ?.GetValue(botEquipmentSettings);

        if (propValue is not null) return propValue;

        _logger.Warning(
            _localisationService.GetText(
                "bot-missing_equipment_settings_property",
                new
                {
                    botRole,
                    setting,
                    defaultValue
                }
            )
        );

        return defaultValue;
    }

    /// <summary>
    /// Create a repairable object for a weapon that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    private UpdRepairable GenerateWeaponRepairableProperties(TemplateItem itemTemplate, string? botRole = null)
    {
        var maxDurability = _durabilityLimitsHelper.GetRandomizedMaxWeaponDurability(itemTemplate, botRole);
        var currentDurability = _durabilityLimitsHelper.GetRandomizedWeaponDurability(
            itemTemplate,
            botRole,
            maxDurability
        );

        return new UpdRepairable { Durability = (int)currentDurability, MaxDurability = (int)maxDurability };
    }

    /// <summary>
    /// Create a repairable object for an armor that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    private UpdRepairable GenerateArmorRepairableProperties(TemplateItem itemTemplate, string? botRole = null)
    {
        double? maxDurability;
        double? currentDurability;
        if (itemTemplate.Properties?.ArmorClass == 0)
        {
            maxDurability = itemTemplate.Properties.MaxDurability;
            currentDurability = itemTemplate.Properties.MaxDurability;
        }
        else
        {
            maxDurability = _durabilityLimitsHelper.GetRandomizedMaxArmorDurability(itemTemplate, botRole);
            currentDurability = _durabilityLimitsHelper.GetRandomizedArmorDurability(
                itemTemplate,
                botRole,
                maxDurability
            );
        }

        return new UpdRepairable { Durability = (int)currentDurability!, MaxDurability = (int)maxDurability! };
    }

    /// <summary>
    /// Can item be added to another item without conflict
    /// </summary>
    /// <param name="itemsEquipped">Items to check compatibilities with</param>
    /// <param name="tplToCheck">Tpl of the item to check for incompatibilities</param>
    /// <param name="equipmentSlot">Slot the item will be placed into</param>
    /// <returns>false if no incompatibilities, also has incompatibility reason</returns>
    public ChooseRandomCompatibleModResult IsItemIncompatibleWithCurrentItems(List<Item> itemsEquipped, string tplToCheck, string equipmentSlot)
    {
        // Skip slots that have no incompatibilities
        List<string> slotsToCheck = ["Scabbard", "Backpack", "SecureContainer", "Holster", "ArmBand"];
        if (slotsToCheck.Contains(equipmentSlot))
        {
            return new ChooseRandomCompatibleModResult { Incompatible = false, Found = false, Reason = "" };
        }

        // TODO: Can probably be optimized to cache itemTemplates as items are added to inventory
        var equippedItemsDb = itemsEquipped.Select(equippedItem => _itemHelper.GetItem(equippedItem.Template).Value).ToList();
        var (key, itemToEquip) = _itemHelper.GetItem(tplToCheck);

        if (!key)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-invalid_item_compatibility_check",
                    new
                    {
                        itemTpl = tplToCheck,
                        slot = equipmentSlot,
                    }
                )
            );

            return new ChooseRandomCompatibleModResult { Incompatible = true, Found = false, Reason = $"item: {tplToCheck} does not exist in the database" };
        }

        if (itemToEquip?.Properties is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-compatibility_check_missing_props",
                    new
                    {
                        id = itemToEquip?.Id,
                        name = itemToEquip?.Name,
                        slot = equipmentSlot,
                    }
                )
            );

            return new ChooseRandomCompatibleModResult { Incompatible = true, Found = false, Reason = $"item: {tplToCheck} does not have a _props field" };
        }

        // Does an equipped item have a property that blocks the desired item - check for prop "BlocksX" .e.g BlocksEarpiece / BlocksFaceCover
        var templateItems = equippedItemsDb.ToList();
        var blockingItem = templateItems.FirstOrDefault(
            item => item?.Properties?.GetType().GetProperties().FirstOrDefault(x => x.Name.ToLower() == $"blocks{equipmentSlot}")?.GetValue(item) is not null
        );
        if (blockingItem is not null)
        {
            // this.logger.warning(`1 incompatibility found between - ${itemToEquip[1]._name} and ${blockingItem._name} - ${equipmentSlot}`);

            return new()
            {
                Incompatible = true, Found = false,
                Reason = $"{tplToCheck} {itemToEquip.Name} in slot: {equipmentSlot} blocked by: {blockingItem.Id} {blockingItem.Name}", SlotBlocked = true
            };
        }

        // Check if any of the current inventory templates have the incoming item defined as incompatible
        blockingItem = templateItems.FirstOrDefault(x => x?.Properties?.ConflictingItems?.Contains(tplToCheck) ?? false);
        if (blockingItem is not null)
        {
            // this.logger.warning(`2 incompatibility found between - ${itemToEquip[1]._name} and ${blockingItem._props.Name} - ${equipmentSlot}`);
            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"{tplToCheck} {itemToEquip.Name} in slot: {equipmentSlot} blocked by: {blockingItem.Id} {blockingItem.Name}",
                SlotBlocked = true
            };
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksHeadwear ?? false)
        {
            var existingHeadwear = itemsEquipped.FirstOrDefault((x) => x.SlotId == "Headwear");
            if (existingHeadwear is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason = $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingHeadwear.Template} in slot: {existingHeadwear.SlotId}",
                    SlotBlocked = true
                };
            }
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksFaceCover ?? false)
        {
            var existingFaceCover = itemsEquipped.FirstOrDefault((item) => item.SlotId == "FaceCover");
            if (existingFaceCover is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason = $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingFaceCover.Template} in slot: {existingFaceCover.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksEarpiece ?? false)
        {
            var existingEarpiece = itemsEquipped.FirstOrDefault((item) => item.SlotId == "Earpiece");
            if (existingEarpiece is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason = $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingEarpiece.Template} in slot: {existingEarpiece.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Does item being checked get blocked/block existing item
        if (itemToEquip.Properties.BlocksArmorVest is not null)
        {
            var existingArmorVest = itemsEquipped.FirstOrDefault((item) => item.SlotId == "ArmorVest");
            if (existingArmorVest is not null)
            {
                return new ChooseRandomCompatibleModResult
                {
                    Incompatible = true,
                    Found = false,
                    Reason = $"{tplToCheck} {itemToEquip.Name} is blocked by: {existingArmorVest.Template} in slot: {existingArmorVest.SlotId}",
                    SlotBlocked = true,
                };
            }
        }

        // Check if the incoming item has any inventory items defined as incompatible
        var blockingInventoryItem = itemsEquipped.FirstOrDefault((x) => itemToEquip.Properties.ConflictingItems?.Contains(x.Template) ?? false);
        if (blockingInventoryItem is not null)
        {
            // this.logger.warning(`3 incompatibility found between - ${itemToEquip[1]._name} and ${blockingInventoryItem._tpl} - ${equipmentSlot}`)
            return new ChooseRandomCompatibleModResult
            {
                Incompatible = true,
                Found = false,
                Reason = $"{tplToCheck} blocks existing item {blockingInventoryItem.Template} in slot {blockingInventoryItem.SlotId}",
            };
        }

        return new ChooseRandomCompatibleModResult { Incompatible = false, Reason = "" };
    }

    /// <summary>
    /// Convert a bots role to the equipment role used in config/bot.json
    /// </summary>
    /// <param name="botRole">Role to convert</param>
    /// <returns>Equipment role (e.g. pmc / assault / bossTagilla)</returns>
    public string GetBotEquipmentRole(string botRole)
    {
        string[] pmcs = [_pmcConfig.UsecType.ToLower(), _pmcConfig.BearType.ToLower()];
        return pmcs.Contains(
            botRole.ToLower()
        )
            ? "pmc"
            : botRole;
    }

    /// <summary>
    /// Adds an item with all its children into specified equipmentSlots, wherever it fits.
    /// </summary>
    /// <param name="equipmentSlots">Slot to add item+children into</param>
    /// <param name="rootItemId">Root item id to use as mod items parentid</param>
    /// <param name="rootItemTplId">Root itms tpl id</param>
    /// <param name="itemWithChildren">Item to add</param>
    /// <param name="inventory">Inventory to add item+children into</param>
    /// <param name="containersIdFull"></param>
    /// <returns>ItemAddedResult result object</returns>
    public ItemAddedResult AddItemWithChildrenToEquipmentSlot(
        List<EquipmentSlots> equipmentSlots,
        string rootItemId,
        string? rootItemTplId,
        List<Item> itemWithChildren,
        BotBaseInventory inventory,
        List<string>? containersIdFull = null)
    {
        // Track how many containers are unable to be found
        var missingContainerCount = 0;
        foreach (var equipmentSlotId in equipmentSlots)
        {
            if (containersIdFull?.Contains(equipmentSlotId.ToString()) ?? false)
            {
                continue;
            }

            // Get container to put item into
            var container = (inventory.Items ?? []).FirstOrDefault(item => item.SlotId == equipmentSlotId.ToString());
            if (container is null)
            {
                missingContainerCount++;
                if (missingContainerCount == equipmentSlots.Count)
                {
                    // Bot doesnt have any containers we want to add item to
                    _logger.Debug(
                        $"Unable to add item: {itemWithChildren.FirstOrDefault()?.Template} to bot as it lacks the following containers: {string.Join(",", equipmentSlots)}"
                    );

                    return ItemAddedResult.NO_CONTAINERS;
                }

                // No container of desired type found, skip to next container type
                continue;
            }

            // Get container details from db
            var (key, value) = _itemHelper.GetItem(container.Template);
            if (!key)
            {
                _logger.Warning(_localisationService.GetText("bot-missing_container_with_tpl", container.Template));

                // Bad item, skip
                continue;
            }

            if (value?.Properties?.Grids?.Count == 0)
            {
                // Container has no slots to hold items
                continue;
            }

            // Get x/y grid size of item
            var itemSize = _inventoryHelper.GetItemSize(rootItemTplId, rootItemId, itemWithChildren);

            // Iterate over each grid in the container and look for a big enough space for the item to be placed in
            var currentGridCount = 1;
            var totalSlotGridCount = value?.Properties?.Grids?.Count;
            foreach (var slotGrid in value?.Properties?.Grids ?? [])
            {
                // Grid is empty, skip or item size is bigger than grid
                if (slotGrid.Props?.CellsH == 0 || slotGrid.Props?.CellsV == 0 || itemSize[0] * itemSize[1] > slotGrid.Props?.CellsV * slotGrid.Props?.CellsH)
                    continue;

                // Can't put item type in grid, skip all grids as we're assuming they have the same rules
                if (!ItemAllowedInContainer(slotGrid, rootItemTplId))
                {
                    // Multiple containers, maybe next one allows item, only break out of loop for this containers grids
                    break;
                }

                // Get all root items in found container
                var existingContainerItems = (inventory.Items ?? []).Where(
                        item => item.ParentId == container.Id && item.SlotId == slotGrid.Name
                    )
                    .ToList();

                // Get root items in container we can iterate over to find out what space is free
                var containerItemsToCheck = existingContainerItems.Where(x => x.SlotId == slotGrid.Name);
                foreach (var item in containerItemsToCheck)
                {
                    // Look for children on items, insert into array if found
                    // (used later when figuring out how much space weapon takes up)
                    var itemWithChildrens = _itemHelper.FindAndReturnChildrenAsItems(inventory.Items, item.Id);
                    if (itemWithChildrens.Count <= 1) continue;

                    existingContainerItems.Remove(item);
                    existingContainerItems.AddRange(itemWithChildrens);
                }

                // Get rid of items free/used spots in current grid
                if (slotGrid.Props is not null)
                {
                    var slotGridMap = _inventoryHelper.GetContainerMap(
                        slotGrid.Props.CellsH.GetValueOrDefault(),
                        slotGrid.Props.CellsV.GetValueOrDefault(),
                        existingContainerItems,
                        container.Id
                    );

                    // Try to fit item into grid
                    var findSlotResult = _containerHelper.FindSlotForItem(slotGridMap, itemSize[0], itemSize[1]);

                    // Open slot found, add item to inventory
                    if (findSlotResult.Success ?? false)
                    {
                        var parentItem = itemWithChildren.FirstOrDefault((i) => i.Id == rootItemId);

                        // Set items parent to container id
                        if (parentItem is not null)
                        {
                            parentItem.ParentId = container.Id;
                            parentItem.SlotId = slotGrid.Name;
                            parentItem.Location = new ItemLocation()
                                {
                                    X = findSlotResult.X,
                                    Y = findSlotResult.Y,
                                    R = (findSlotResult.Rotation ?? false) ? 1 : 0,
                                }
                                ;
                        }

                        (inventory.Items ?? []).AddRange(itemWithChildren);

                        return ItemAddedResult.SUCCESS;
                    }
                }

                // If we've checked all grids in container and reached this point, there's no space for item
                if (currentGridCount >= totalSlotGridCount)
                {
                    break;
                }

                currentGridCount++;
                // No space in this grid, move to next container grid and try again
            }

            // if we got to this point, the item couldnt be placed on the container
            if (containersIdFull is null) continue;

            // if the item was a one by one, we know it must be full. Or if the maps cant find a slot for a one by one
            if (itemSize[0] == 1 && itemSize[1] == 1)
            {
                containersIdFull.Add(equipmentSlotId.ToString());
            }
        }

        return ItemAddedResult.NO_SPACE;
    }

    /// <summary>
    /// Is the provided item allowed inside a container
    /// </summary>
    /// <param name="slotGrid">Items sub-grid we want to place item inside</param>
    /// <param name="itemTpl">Item tpl being placed</param>
    /// <returns>True if allowed</returns>
    private bool ItemAllowedInContainer(Grid? slotGrid, string? itemTpl)
    {
        var propFilters = slotGrid?.Props?.Filters;
        var excludedFilter = propFilters?.FirstOrDefault()?.ExcludedFilter ?? [];
        var filter = propFilters?.FirstOrDefault()?.Filter ?? [];

        if (propFilters?.Count == 0)
        {
            // no filters, item is fine to add
            return true;
        }

        // Check if item base type is excluded
        var itemDetails = _itemHelper.GetItem(itemTpl).Value;

        // if item to add is found in exclude filter, not allowed
        if (excludedFilter.Contains(itemDetails?.Parent ?? string.Empty))
        {
            return false;
        }

        // If Filter array only contains 1 filter and its for basetype 'item', allow it
        if (filter.Count == 1 && filter.Contains(BaseClasses.ITEM))
        {
            return true;
        }

        // If allowed filter has something in it + filter doesnt have basetype 'item', not allowed
        if (filter.Count > 0 && !filter.Contains(itemDetails?.Parent ?? string.Empty))
        {
            return false;
        }

        return true;
    }
}
