using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;

namespace Core.Generators;

[Injectable]
public class BotEquipmentModGenerator
{
    private BotConfig _botConfig;

    public BotEquipmentModGenerator()
    {
    }

    /// <summary>
    /// Check mods are compatible and add to array
    /// </summary>
    /// <param name="equipment">Equipment item to add mods to</param>
    /// <param name="parentId">Mod list to choose from</param>
    /// <param name="parentTemplate">parentid of item to add mod to</param>
    /// <param name="settings">Template object of item to add mods to</param>
    /// <param name="specificBlacklist">The relevant blacklist from bot.json equipment dictionary</param>
    /// <param name="shouldForceSpawn">should this mod be forced to spawn</param>
    /// <returns>Item + compatible mods as an array</returns>
    public Item GenerateModsForEquipment(List<Item> equipment, string parentId, TemplateItem parentTemplate, GenerateEquipmentProperties settings,
        EquipmentFilterDetails specificBlacklist, bool shouldForceSpawn = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter a bots plate pool based on its current level
    /// </summary>
    /// <param name="settings">Bot equipment generation settings</param>
    /// <param name="modSlot">Armor slot being filtered</param>
    /// <param name="existingPlateTplPool">Plates tpls to choose from</param>
    /// <param name="armorItem">The armor items db template</param>
    /// <returns>Array of plate tpls to choose from</returns>
    public FilterPlateModsForSlotByLevelResult FilterPlateModsForSlotByLevel(GenerateEquipmentProperties settings, string modSlot,
        List<string> existingPlateTplPool, TemplateItem armorItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add mods to a weapon using the provided mod pool
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="request">Data used to generate the weapon</param>
    /// <returns>Weapon + mods array</returns>
    public List<Item> GenerateModsForWeapon(string sessionId, GenerateWeaponRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Should the provided bot have its stock chance values altered to 100%
    /// </summary>
    /// <param name="modSlot">Slot to check</param>
    /// <param name="botEquipConfig">Bots equipment config/chance values</param>
    /// <param name="modToAddTemplate">Mod being added to bots weapon</param>
    /// <returns>True if it should</returns>
    public bool ShouldForceSubStockSlots(string modSlot, EquipmentFilters botEquipConfig, TemplateItem modToAddTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is this modslot a front or rear sight
    /// </summary>
    /// <param name="modSlot">Slot to check</param>
    /// <param name="tpl"></param>
    /// <returns>true if it's a front/rear sight</returns>
    public bool ModIsFrontOrRearSight(string modSlot, string tpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the provided mod details show the mod can hold a scope
    /// </summary>
    /// <param name="modSlot">e.g. mod_scope, mod_mount</param>
    /// <param name="ModsParentId">Parent id of mod item</param>
    /// <returns>true if it can hold a scope</returns>
    public bool ModSlotCanHoldScope(string modSlot, string ModsParentId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Set mod spawn chances to defined amount
    /// </summary>
    /// <param name="modSpawnChances">Chance dictionary to update</param>
    /// <param name="modSlotsToAdjust"></param>
    /// <param name="newChancePercent"></param>
    public void AdjustSlotSpawnChances(Dictionary<string, double> modSpawnChances, List<string> modSlotsToAdjust, double newChancePercent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does the provided modSlot allow muzzle-related items
    /// </summary>
    /// <param name="modSlot">Slot id to check</param>
    /// <param name="modsParentId">OPTIONAL: parent id of modslot being checked</param>
    /// <returns>True if modSlot can have muzzle-related items</returns>
    public bool AodSlotCanHoldMuzzleDevices(string modSlot, string? modsParentId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sort mod slots into an ordering that maximises chance of a successful weapon generation
    /// </summary>
    /// <param name="unsortedSlotKeys">Array of mod slot strings to sort</param>
    /// <param name="itemTplWithKeysToSort">The Tpl of the item with mod keys being sorted</param>
    /// <returns>Sorted array</returns>
    public List<string> SortModKeys(List<string> unsortedSlotKeys, string itemTplWithKeysToSort)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a Slot property for an item (chamber/cartridge/slot)
    /// </summary>
    /// <param name="modSlot">e.g patron_in_weapon</param>
    /// <param name="parentTemplate">item template</param>
    /// <returns>Slot item</returns>
    public Slot GetModItemSlotFromDb(string modSlot, TemplateItem parentTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomly choose if a mod should be spawned, 100% for required mods OR mod is ammo slot
    /// </summary>
    /// <param name="itemSlot">slot the item sits in from db</param>
    /// <param name="modSlotName">Name of slot the mod sits in</param>
    /// <param name="modSpawnChances">Chances for various mod spawns</param>
    /// <param name="botEquipConfig">Various config settings for generating this type of bot</param>
    /// <returns>ModSpawn.SPAWN when mod should be spawned, ModSpawn.DEFAULT_MOD when default mod should spawn, ModSpawn.SKIP when mod is skipped</returns>
    public ModSpawn ShouldModBeSpawned(Slot itemSlot, string modSlotName, Dictionary<string, double> modSpawnChances, EquipmentFilters botEquipConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a mod to fit into the desired slot
    /// </summary>
    /// <param name="request">Data used to choose an appropriate mod with</param>
    /// <returns>itemHelper.getItem() result</returns>
    public object? ChooseModToPutIntoSlot(ModToSpawnRequest request) // TODO: type fuckery: [boolean, ITemplateItem] | undefined
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Given the passed in array of magaizne tpls, look up the min size set in config and return only those that have that size or larger
    /// </summary>
    /// <param name="modSpawnRequest">Request data</param>
    /// <param name="modPool">Pool of magazine tpls to filter</param>
    /// <returns>Filtered pool of magazine tpls</returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<string> GetFilterdMagazinePoolByCapacity(ModToSpawnRequest modSpawnRequest, List<string> modPool)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Choose a weapon mod tpl for a given slot from a pool of choices
    /// Checks chosen tpl is compatible with all existing weapon items
    /// </summary>
    /// <param name="request"></param>
    /// <param name="modPool">Pool of mods that can be picked from</param>
    /// <param name="parentSlot">Slot the picked mod will have as a parent</param>
    /// <param name="choiceTypeEnum">How should chosen tpl be treated: DEFAULT_MOD/SPAWN/SKIP</param>
    /// <param name="weapon">Array of weapon items chosen item will be added to</param>
    /// <param name="modSlotName">Name of slot picked mod will be placed into</param>
    /// <returns>Chosen weapon details</returns>
    public ChooseRandomCompatibleModResult GetCompatibleWeaponModTplForSlotFromPool(ModToSpawnRequest request, List<string> modPool, Slot parentSlot,
        ModSpawn choiceTypeEnum, List<Item> weapon, string modSlotName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modPool">Pool of item Tpls to choose from</param>
    /// <param name="modSpawnType">How should the slot choice be handled - forced/normal etc</param>
    /// <param name="weapon">Weapon mods at current time</param>
    /// <returns>IChooseRandomCompatibleModResult</returns>
    public ChooseRandomCompatibleModResult GetCompatibleModFromPool(List<string> modPool, ModSpawn modSpawnType, List<Item> weapon)
    {
        throw new NotImplementedException();
    }

    public object CreateExhaustableArray<T>(T itemsToAddToArray) // TODO: this wont likely be needed, reimplement for C#
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a list of mod tpls that are compatible with the current weapon
    /// </summary>
    /// <param name="modPool"></param>
    /// <param name="tplBlacklist">Tpls that are incompatible and should not be used</param>
    /// <returns>string array of compatible mod tpls with weapon</returns>
    public List<string> GetFilteredModPool(List<string> modPool, List<string> tplBlacklist) // TODO: tplBlacklist was Set<string>
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter mod pool down based on various criteria:
    /// Is slot flagged as randomisable
    /// Is slot required
    /// Is slot flagged as default mod only
    /// </summary>
    /// <param name="request"></param>
    /// <param name="weaponTemplate">Mods root parent (weapon/equipment)</param>
    /// <returns>Array of mod tpls</returns>
    public List<string> GetModPoolForSlot(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }

    public List<string> GetModPoolForDefaultSlot(ModToSpawnRequest request, TemplateItem weaponTemplate)
    {
        throw new NotImplementedException();
    }

    public object GetMatchingModFromPreset(ModToSpawnRequest request, TemplateItem weaponTemplate) // TODO: no return type given in node server
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get default preset for weapon OR get specific weapon presets for edge cases (mp5/silenced dvl)
    /// </summary>
    /// <param name="weaponTemplate">Weapons db template</param>
    /// <param name="parentItemTpl">Tpl of the parent item</param>
    /// <returns>Default preset found</returns>
    public Preset? GetMatchingPreset(TemplateItem weaponTemplate, string parentItemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Temp fix to prevent certain combinations of weapons with mods that are known to be incompatible
    /// </summary>
    /// <param name="weapon">Array of items that make up a weapon</param>
    /// <param name="modTpl">Mod to check compatibility with weapon</param>
    /// <returns>True if incompatible</returns>
    public bool WeaponModComboIsIncompatible(List<Item> weapon, string modTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a mod item with provided parameters as properties + add upd property
    /// </summary>
    /// <param name="modId">_id</param>
    /// <param name="modTpl">_tpl</param>
    /// <param name="parentId">parentId</param>
    /// <param name="modSlot">slotId</param>
    /// <param name="modTemplate">Used to add additional properties in the upd object</param>
    /// <param name="botRole">The bots role mod is being created for</param>
    /// <returns>Item object</returns>
    public Item CreateModItem(string modId, string modTpl, string parentId, string modSlot, TemplateItem modTemplate, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a list of containers that hold ammo
    /// e.g. mod_magazine / patron_in_weapon_000
    /// </summary>
    /// <returns>string array</returns>
    public List<string> GetAmmoContainers()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a random mod from an items compatible mods Filter array
    /// </summary>
    /// <param name="fallbackModTpl">Default value to return if parentSlot Filter is empty</param>
    /// <param name="parentSlot">Item mod will go into, used to get compatible items</param>
    /// <param name="modSlot">Slot to get mod to fill</param>
    /// <param name="items">Items to ensure picked mod is compatible with</param>
    /// <returns>Item tpl</returns>
    public string? GetRandomModTplFromItemDb(string fallbackModTpl, Slot parentSlot, string modSlot, List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if mod exists in db + is for a required slot
    /// TODO: modToAdd type was [boolean, ITemplateItem] in node
    /// </summary>
    /// <param name="modtoAdd">Db template of mod to check</param>
    /// <param name="slotAddedToTemplate">Slot object the item will be placed as child into</param>
    /// <param name="modSlot">Slot the mod will fill</param>
    /// <param name="parentTemplate">Db template of the mods being added</param>
    /// <param name="botRole">Bots wildspawntype (assault/pmcBot/exUsec etc)</param>
    /// <returns>True if valid for slot</returns>
    public bool IsModValidForSlot(object modToAdd, Slot slotAddedToTemplate, string modSlot, TemplateItem parentTemplate, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Find mod tpls of a provided type and add to modPool
    /// </summary>
    /// <param name="desiredSlotName">Slot to look up and add we are adding tpls for (e.g mod_scope)</param>
    /// <param name="modTemplate">db object for modItem we get compatible mods from</param>
    /// <param name="modPool">Pool of mods we are adding to</param>
    /// <param name="botEquipBlacklist">A blacklist of items that cannot be picked</param>
    public void AddCompatibleModsForProvidedMod(string desiredSlotName, TemplateItem modTemplate, Dictionary<string, Dictionary<string, List<string>>> modPool,
        EquipmentFilterDetails botEquipBlacklist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the possible items that fit a slot
    /// </summary>
    /// <param name="parentItemId">item tpl to get compatible items for</param>
    /// <param name="modSlot">Slot item should fit in</param>
    /// <param name="botEquipBlacklist">Equipment that should not be picked</param>
    /// <returns>Array of compatible items for that slot</returns>
    public List<string> GetDynamicModPool(string parentItemId, string modSlot, EquipmentFilterDetails botEquipBlacklist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take a list of tpls and filter out blacklisted values using itemFilterService + botEquipmentBlacklist
    /// </summary>
    /// <param name="allowedMods">Base mods to filter</param>
    /// <param name="botEquipBlacklist">Equipment blacklist</param>
    /// <param name="modSlot">Slot mods belong to</param>
    /// <returns>Filtered array of mod tpls</returns>
    public List<string> FilterModsByBlacklist(List<string> allowedMods, EquipmentFilterDetails botEquipBlacklist, string modSlot)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// With the shotgun revolver (60db29ce99594040e04c4a27) 12.12 introduced CylinderMagazines.
    /// Those magazines (e.g. 60dc519adf4c47305f6d410d) have a "Cartridges" entry with a _max_count=0.
    /// Ammo is not put into the magazine directly but assigned to the magazine's slots: The "camora_xxx" slots.
    /// This function is a helper called by generateModsForItem for mods with parent type "CylinderMagazine"
    /// </summary>
    /// <param name="items">The items where the CylinderMagazine's camora are appended to</param>
    /// <param name="modPool">ModPool which should include available cartridges</param>
    /// <param name="cylinderMagParentId">The CylinderMagazine's UID</param>
    /// <param name="cylinderMagTemplate">The CylinderMagazine's template</param>
    public void FillCamora(List<Item> items, Dictionary<string, Dictionary<string, List<string>>> modPool, string cylinderMagParentId,
        TemplateItem cylinderMagTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take a record of camoras and merge the compatible shells into one array
    /// </summary>
    /// <param name="camorasWithShells">Dictionary of camoras we want to merge into one array</param>
    /// <returns>String array of shells for multiple camora sources</returns>
    public List<string> MergeCamoraPools(Dictionary<string, List<string>> camorasWithShells)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter out non-whitelisted weapon scopes
    /// Controlled by bot.json weaponSightWhitelist
    /// e.g. filter out rifle scopes from SMGs
    /// </summary>
    /// <param name="weapon">Weapon scopes will be added to</param>
    /// <param name="scopes">Full scope pool</param>
    /// <param name="botWeaponSightWhitelist">Whitelist of scope types by weapon base type</param>
    /// <returns>Array of scope tpls that have been filtered to just ones allowed for that weapon type</returns>
    public List<string> FilterSightsByWeaponType(Item weapon, List<string> scopes, Dictionary<string, List<string>> botWeaponSightWhitelist)
    {
        throw new NotImplementedException();
    }
}
