using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Generators;

[Injectable]
public class BotWeaponGenerator
{
    private readonly ISptLogger<BotWeaponGenerator> _logger;
    private readonly HashUtil _hashUtil;
    private readonly RandomUtil _randomUtil;
    private readonly LocalisationService _localisationService;
    private readonly ItemHelper _itemHelper;
    private readonly BotGeneratorHelper _botGeneratorHelper;
    private readonly WeightedRandomHelper _weightedRandomHelper;
    private readonly RepairService _repairService;
    private readonly ConfigServer _configServer;
    private const string _modMagazineSlotId = "mod_magazine";
    private BotConfig _botConfig;
    private PmcConfig _pmcConfig;
    private RepairConfig _repairConfig;

    public BotWeaponGenerator(
        ISptLogger<BotWeaponGenerator> logger,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        LocalisationService localisationService,
        ItemHelper itemHelper,
        BotGeneratorHelper botGeneratorHelper,
        WeightedRandomHelper weightedRandomHelper,
        RepairService repairService,
        ConfigServer configServer)
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _randomUtil = randomUtil;
        _localisationService = localisationService;
        _itemHelper = itemHelper;
        _botGeneratorHelper = botGeneratorHelper;
        _weightedRandomHelper = weightedRandomHelper;
        _repairService = repairService;
        _configServer = configServer;

        _botConfig = _configServer.GetConfig<BotConfig>();
    }

    /// <summary>
    /// Pick a random weapon based on weightings and generate a functional weapon
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="equipmentSlot">Primary/secondary/holster</param>
    /// <param name="botTemplateInventory">e.g. assault.json</param>
    /// <param name="weaponParentId"></param>
    /// <param name="modChances"></param>
    /// <param name="botRole">Role of bot, e.g. assault/followerBully</param>
    /// <param name="isPmc">Is weapon generated for a pmc</param>
    /// <param name="botLevel"></param>
    /// <returns>GenerateWeaponResult object</returns>
    public GenerateWeaponResult GenerateRandomWeapon(string sessionId, EquipmentSlots equipmentSlot, BotTypeInventory botTemplateInventory, string weaponParentId,
        Dictionary<string, double> modChances, string botRole, bool isPmc, int botLevel)
    {
        var weaponTpl = PickWeightedWeaponTemplateFromPool(equipmentSlot, botTemplateInventory);
        return GenerateWeaponByTpl(
            sessionId,
            weaponTpl,
            equipmentSlot,
            botTemplateInventory,
            weaponParentId,
            modChances,
            botRole,
            isPmc,
            botLevel);
    }

    /// <summary>
    /// Gets a random weighted weapon from a bot's pool of weapons.
    /// </summary>
    /// <param name="equipmentSlot">Primary/secondary/holster</param>
    /// <param name="botTemplateInventory">e.g. assault.json</param>
    /// <returns>Weapon template</returns>
    public string PickWeightedWeaponTemplateFromPool(EquipmentSlots equipmentSlot, BotTypeInventory botTemplateInventory)
    {
        var weaponPool = botTemplateInventory.Equipment[equipmentSlot];
        return _weightedRandomHelper.GetWeightedValue(weaponPool);
    }

    /// <summary>
    /// Generates a weapon based on the supplied weapon template.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="weaponTpl">Weapon template to generate (use pickWeightedWeaponTplFromPool()).</param>
    /// <param name="slotName">Slot to fit into, primary/secondary/holster.</param>
    /// <param name="botTemplateInventory">e.g. assault.json.</param>
    /// <param name="weaponParentId">Parent ID of the weapon being generated.</param>
    /// <param name="modChances">Dictionary of item types and % chance weapon will have that mod.</param>
    /// <param name="botRole">e.g. assault/exusec.</param>
    /// <param name="isPmc">Is weapon being generated for a PMC.</param>
    /// <param name="botLevel">The level of the bot.</param>
    /// <returns>GenerateWeaponResult object.</returns>
    public GenerateWeaponResult? GenerateWeaponByTpl(string sessionId, string weaponTpl, EquipmentSlots slotName, BotTypeInventory botTemplateInventory,
        string weaponParentId, Dictionary<string, double> modChances, string botRole, bool isPmc, int botLevel)
    {
        var modPool = botTemplateInventory.Mods;
        var weaponItemTemplate = _itemHelper.GetItem(weaponTpl).Value;

        if (weaponItemTemplate is null)
        {
            _logger.Error(_localisationService.GetText("bot-missing_item_template", weaponTpl));
            _logger.Error($"WeaponSlot-> { slotName}");

            return null;
        }

        // Find ammo to use when filling magazines/chamber
        if (botTemplateInventory.Ammo is not null)
        {
            _logger.Error(_localisationService.GetText("bot-no_ammo_found_in_bot_json", botRole));
            _logger.Error(_localisationService.GetText("bot-generation_failed"));

            return null;
        }
        var ammoTpl = GetWeightedCompatibleAmmo(botTemplateInventory.Ammo, weaponItemTemplate);

        // Create with just base weapon item
        var weaponWithModsArray = ConstructWeaponBaseList(
            weaponTpl,
            weaponParentId,
            slotName,
            weaponItemTemplate,
            botRole);

        // Chance to add randomised weapon enhancement
        if (isPmc && _randomUtil.GetChance100(_pmcConfig.WeaponHasEnhancementChancePercent))
        {
            // Add buff to weapon root
            _repairService.AddBuff(_repairConfig.RepairKit.Weapon, weaponWithModsArray[0]);
        }
        

        throw new NotImplementedException();
    }

    /// <summary>
    /// Insert cartridge(s) into a weapon
    /// Handles all chambers - patron_in_weapon, patron_in_weapon_000 etc
    /// </summary>
    /// <param name="weaponWithModsList">Weapon and mods</param>
    /// <param name="ammoTemplate">Cartridge to add to weapon</param>
    /// <param name="chamberSlotIdentifiers">Name of slots to create or add ammo to</param>
    protected void AddCartridgeToChamber(List<Item> weaponWithModsList, string ammoTemplate, List<string> chamberSlotIdentifiers)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a list with weapon base as the only element and
    /// add additional properties based on weapon type
    /// </summary>
    /// <param name="weaponTemplate">Weapon template to create item with</param>
    /// <param name="weaponParentId">Weapons parent id</param>
    /// <param name="equipmentSlot">e.g. primary/secondary/holster</param>
    /// <param name="weaponItemTemplate">Database template for weapon</param>
    /// <param name="botRole">For durability values</param>
    /// <returns>Base weapon item in a list</returns>
    protected List<Item> ConstructWeaponBaseList(string weaponTemplate, string weaponParentId, EquipmentSlots equipmentSlot, TemplateItem weaponItemTemplate,
        string botRole)
    {
        return
        [
            new()
            {
                Id = _hashUtil.Generate(),
                Template = weaponTemplate,
                ParentId = weaponParentId,
                SlotId = equipmentSlot.ToString(),
                Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(weaponItemTemplate, botRole)
            }
        ];
    }

    /// <summary>
    /// Get the mods necessary to kit out a weapon to its preset level
    /// </summary>
    /// <param name="weaponTemplate">Weapon to find preset for</param>
    /// <param name="equipmentSlot">The slot the weapon will be placed in</param>
    /// <param name="weaponParentId">Value used for the parent id</param>
    /// <param name="itemTemplate">Item template</param>
    /// <param name="botRole">Bot role</param>
    /// <returns>List of weapon mods</returns>
    protected List<Item> GetPresetWeaponMods(string weaponTemplate, string equipmentSlot, string weaponParentId, TemplateItem itemTemplate, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if all required slots are occupied on a weapon and all its mods.
    /// </summary>
    /// <param name="weaponItemList">Weapon + mods</param>
    /// <param name="botRole">Role of bot weapon is for</param>
    /// <returns>True if valid</returns>
    protected bool IsWeaponValid(List<Item> weaponItemList, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Generates extra magazines or bullets (if magazine is internal) and adds them to TacticalVest and Pockets.
    /// Additionally, adds extra bullets to SecuredContainer
    /// </summary>
    /// <param name="generatedWeaponResult">Object with properties for generated weapon (weapon mods pool / weapon template / ammo tpl)</param>
    /// <param name="magWeights">Magazine weights for count to add to inventory</param>
    /// <param name="inventory">Inventory to add magazines to</param>
    /// <param name="botRole">The bot type we're generating extra mags for</param>
    public void AddExtraMagazinesToInventory(GenerateWeaponResult generatedWeaponResult, GenerationData magWeights, BotBaseInventory inventory, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add Grenades for UBGL to bot's vest and secure container
    /// </summary>
    /// <param name="weaponMods">Weapon list with mods</param>
    /// <param name="generatedWeaponResult">Result of weapon generation</param>
    /// <param name="inventory">Bot inventory to add grenades to</param>
    protected void AddUbglGrenadesToBotInventory(List<Item> weaponMods, GenerateWeaponResult generatedWeaponResult, BotBaseInventory inventory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add ammo to the secure container.
    /// </summary>
    /// <param name="stackCount">How many stacks of ammo to add.</param>
    /// <param name="ammoTpl">Ammo type to add.</param>
    /// <param name="stackSize">Size of the ammo stack to add.</param>
    /// <param name="inventory">Player inventory.</param>
    protected void AddAmmoToSecureContainer(int stackCount, string ammoTemplate, int stackSize, BotBaseInventory inventory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a weapons magazine template from a weapon template.
    /// </summary>
    /// <param name="weaponMods">Mods from a weapon template.</param>
    /// <param name="weaponTemplate">Weapon to get magazine template for.</param>
    /// <param name="botRole">The bot type we are getting the magazine for.</param>
    /// <returns>Magazine template string.</returns>
    protected string GetMagazineTemplateFromWeaponTemplate(List<Item> weaponMods, TemplateItem weaponTemplate, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Finds and returns a compatible ammo template based on the bots ammo weightings (x.json/inventory/equipment/ammo)
    /// </summary>
    /// <param name="cartridgePool">Dictionary of all cartridges keyed by type e.g. Caliber556x45NATO</param>
    /// <param name="weaponTemplate">Weapon details from database we want to pick ammo for</param>
    /// <returns>Ammo template that works with the desired gun</returns>
    protected string GetWeightedCompatibleAmmo(Dictionary<string, Dictionary<string, double>> cartridgePool, TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the cartridge ids from a weapon template that work with the weapon
    /// </summary>
    /// <param name="weaponTemplate">Weapon db template to get cartridges for</param>
    /// <returns>List of cartridge tpls</returns>
    protected List<string> GetCompatibleCartridgesFromWeaponTemplate(TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a weapons compatible cartridge caliber
    /// </summary>
    /// <param name="weaponTemplate">Weapon to look up caliber of</param>
    /// <returns>Caliber as string</returns>
    protected string GetWeaponCaliber(TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Fill existing magazines to full, while replacing their contents with specified ammo
    /// </summary>
    /// <param name="weaponMods">Weapon with children</param>
    /// <param name="magazine">Magazine item</param>
    /// <param name="cartridgeTemplate">Cartridge to insert into magazine</param>
    protected void FillExistingMagazines(List<Item> weaponMods, Item magazine, string cartridgeTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add desired ammo template as item to weapon modifications list, placed as child to UBGL.
    /// </summary>
    /// <param name="weaponMods">Weapon with children.</param>
    /// <param name="ubglMod">UBGL item.</param>
    /// <param name="ubglAmmoTpl">Grenade ammo template.</param>
    protected void FillUbgl(List<Item> weaponMods, Item ubglMod, string ubglAmmoTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add cartridge item to weapon item list, if it already exists, update
    /// </summary>
    /// <param name="weaponWithMods">Weapon items list to amend</param>
    /// <param name="magazine">Magazine item details we're adding cartridges to</param>
    /// <param name="chosenAmmoTpl">Cartridge to put into the magazine</param>
    /// <param name="newStackSize">How many cartridges should go into the magazine</param>
    /// <param name="magazineTemplate">Magazines db template</param>
    protected void AddOrUpdateMagazinesChildWithAmmo(List<Item> weaponWithMods, Item magazine, string chosenAmmoTpl, TemplateItem magazineTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Fill each Camora with a bullet
    /// </summary>
    /// <param name="weaponMods">Weapon mods to find and update camora mod(s) from</param>
    /// <param name="magazineId">Magazine id to find and add to</param>
    /// <param name="ammoTpl">Ammo template id to hydrate with</param>
    protected void FillCamorasWithAmmo(List<Item> weaponMods, string magazineId, string ammoTpl)
    {
        throw new NotImplementedException();
    }
}
