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
    private readonly DatabaseService _databaseService;
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
        DatabaseService databaseService,
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
        _databaseService = databaseService;
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
    public Upd GenerateExtraPropertiesForItem(TemplateItem itemTemplate, string botRole = null)
    {
        // Get raid settings, if no raid, default to day
        var raidSettings = _applicationContext
            .GetLatestValue(ContextVariableType.RAID_CONFIGURATION)
            ?.GetValue<GetRaidConfigurationRequestData>();
        var raidIsNight = raidSettings?.TimeVariant == DateTimeEnum.PAST;

        Upd itemProperties = new();

        if (itemTemplate.Properties.MaxDurability is not null)
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

        if (itemTemplate.Properties.HasHinge ?? false)
        {
            itemProperties.Togglable = new() { On = true };
        }

        if (itemTemplate.Properties.Foldable ?? false)
        {
            itemProperties.Foldable = new() { Folded = false };
        }

        if (itemTemplate.Properties.WeapFireType?.Any() ?? false)
        {
            if (itemTemplate.Properties.WeapFireType.Contains("fullauto"))
            {
                itemProperties.FireMode = new() { FireMode = "fullauto" };
            }
            else
            {
                itemProperties.FireMode = new() { FireMode = _randomUtil.GetArrayValue(itemTemplate.Properties.WeapFireType) };
            }
        }

        if (itemTemplate.Properties.MaxHpResource is not null)
        {
            itemProperties.MedKit = new()
            {
                HpResource = (int)GetRandomizedResourceValue(
                    itemTemplate.Properties.MaxHpResource ?? 0,
                    _botConfig.LootItemResourceRandomization[botRole]?.Meds
                )
            };
        }

        if (itemTemplate.Properties.MaxResource is not null && itemTemplate.Properties.FoodUseTime is not null)
        {
            itemProperties.FoodDrink = new()
            {
                HpPercent = (int)GetRandomizedResourceValue(
                    itemTemplate.Properties.MaxResource ?? 0,
                    _botConfig.LootItemResourceRandomization[botRole]?.Food
                ),
            };
        }

        if (itemTemplate.Parent == BaseClasses.FLASHLIGHT)
        {
            // Get chance from botconfig for bot type
            var lightLaserActiveChance = raidIsNight
                ? GetBotEquipmentSettingFromConfig(botRole, "lightIsActiveNightChancePercent", 50)
                : GetBotEquipmentSettingFromConfig(botRole, "lightIsActiveDayChancePercent", 25);
            itemProperties.Light = new()
            {
                IsActive = _randomUtil.GetChance100(lightLaserActiveChance),
                SelectedMode = 0,
            };
        }
        else if (itemTemplate.Parent == BaseClasses.TACTICAL_COMBO)
        {
            // Get chance from botconfig for bot type, use 50% if no value found
            var lightLaserActiveChance = GetBotEquipmentSettingFromConfig(
                botRole,
                "laserIsActiveChancePercent",
                50
            );
            itemProperties.Light = new()
            {
                IsActive = _randomUtil.GetChance100(lightLaserActiveChance),
                SelectedMode = 0,
            };
        }

        if (itemTemplate.Parent == BaseClasses.NIGHTVISION)
        {
            // Get chance from botconfig for bot type
            var nvgActiveChance = raidIsNight
                ? GetBotEquipmentSettingFromConfig(botRole, "nvgIsActiveChanceNightPercent", 90)
                : GetBotEquipmentSettingFromConfig(botRole, "nvgIsActiveChanceDayPercent", 15);
            itemProperties.Togglable = new()
            {
                On = _randomUtil.GetChance100(nvgActiveChance)
            };
        }

        // Togglable face shield
        if ((itemTemplate.Properties.HasHinge ?? false) && (itemTemplate.Properties.FaceShieldComponent ?? false))
        {
            // Get chance from botconfig for bot type, use 75% if no value found
            var faceShieldActiveChance = GetBotEquipmentSettingFromConfig(
                botRole,
                "faceShieldIsActiveChancePercent",
                75
            );
            itemProperties.Togglable = new()
                {
                    On = _randomUtil.GetChance100(faceShieldActiveChance)
                }
                ;
        }

        return itemProperties;
    }

    /// <summary>
    /// Randomize the HpResource for bots e.g (245/400 resources)
    /// </summary>
    /// <param name="maxResource">Max resource value of medical items</param>
    /// <param name="randomizationValues">Value provided from config</param>
    /// <returns>Randomized value from maxHpResource</returns>
    protected double GetRandomizedResourceValue(double maxResource, RandomisedResourceValues randomizationValues)
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
    protected double GetBotEquipmentSettingFromConfig(string botRole, string setting, double defaultValue)
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
                        botRole = botRole,
                        setting = setting,
                        defaultValue = defaultValue
                    }
                )
            );

            return defaultValue;
        }

        var props = botEquipmentSettings.GetType().GetProperties();
        var propValue = (double?)props.FirstOrDefault(x => x.Name.ToLower() == setting.ToLower()).GetValue(botEquipmentSettings);

        if (propValue is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "bot-missing_equipment_settings_property",
                    new
                    {
                        botRole = botRole,
                        setting = setting,
                        defaultValue = defaultValue
                    }
                )
            );

            return defaultValue;
        }

        return propValue ?? 0;
    }

    /// <summary>
    /// Create a repairable object for a weapon that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    protected UpdRepairable GenerateWeaponRepairableProperties(TemplateItem itemTemplate, string botRole = null)
    {
        var maxDurability = _durabilityLimitsHelper.GetRandomizedMaxWeaponDurability(itemTemplate, botRole);
        var currentDurability = _durabilityLimitsHelper.GetRandomizedWeaponDurability(
            itemTemplate,
            botRole,
            maxDurability
        );

        return new() { Durability = (int)currentDurability, MaxDurability = (int)maxDurability };
    }

    /// <summary>
    /// Create a repairable object for an armor that containers durability + max durability properties
    /// </summary>
    /// <param name="itemTemplate">weapon object being generated for</param>
    /// <param name="botRole">type of bot being generated for</param>
    /// <returns>Repairable object</returns>
    protected UpdRepairable GenerateArmorRepairableProperties(TemplateItem itemTemplate, string botRole = null)
    {
        double? maxDurability = null;
        double? currentDurability = null;
        if (itemTemplate.Properties.ArmorClass == 0)
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
                maxDurability ?? 0
            );
        }

        return new() { Durability = (int)currentDurability, MaxDurability = (int)maxDurability };
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
        throw new NotImplementedException();
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
    /// <returns>ItemAddedResult result object</returns>
    public ItemAddedResult AddItemWithChildrenToEquipmentSlot(
        List<string> equipmentSlots,
        string rootItemId,
        string rootItemTplId,
        List<Item> itemWithChildren,
        BotBaseInventory inventory,
        HashSet<string> containersIdFull = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided item allowed inside a container
    /// </summary>
    /// <param name="slotGrid">Items sub-grid we want to place item inside</param>
    /// <param name="itemTpl">Item tpl being placed</param>
    /// <returns>True if allowed</returns>
    protected bool ItemAllowedInContainer(Grid slotGrid, string itemTpl)
    {
        throw new NotImplementedException();
    }
}
