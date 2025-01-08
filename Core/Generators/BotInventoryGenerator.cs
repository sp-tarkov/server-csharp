using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Match;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Equipment = Core.Models.Eft.Common.Tables.Equipment;

namespace Core.Generators;

public class BotInventoryGenerator
{
    private BotConfig _botConfig;

    public BotInventoryGenerator()
    {
    }

    /// <summary>
    /// Add equipment/weapons/loot to bot
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <param name="botRole">Role bot has (assault/pmcBot)</param>
    /// <param name="isPmc">Is bot being converted into a pmc</param>
    /// <param name="botLevel">Level of bot being generated</param>
    /// <param name="chosenGameVersion">Game version for bot, only really applies for PMCs</param>
    /// <returns>PmcInventory object with equipment/weapons/loot</returns>
    public BotBaseInventory generateInventory(string sessionId, BotType botJsonTemplate, string botRole, bool isPmc, int botLevel, string chosenGameVersion)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a pmcInventory object with all the base/generic items needed
    /// </summary>
    /// <returns>PmcInventory object</returns>
    public BotBaseInventory GenerateInventoryBase()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add equipment to a bot
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="wornItemChances">Chances items will be added to bot</param>
    /// <param name="botRole">Role bot has (assault/pmcBot)</param>
    /// <param name="botInventory">Inventory to add equipment to</param>
    /// <param name="botLevel">Level of bot</param>
    /// <param name="chosenGameVersion">Game version for bot, only really applies for PMCs</param>
    /// <param name="raidConfig">RadiConfig</param>
    public void GenerateAndAddEquipmentToBot(string sessionId, BotBaseInventory templateInventory, Chances wornItemChances, string botRole,
        BotBaseInventory botInventory, int botLevel, string chosenGameVersion, GetRaidConfigurationRequestData raidConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove non-armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equpiment to filter TacticalVest of</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    public void FilterRigsToThoseWithProtection(Equipment templateEquipment, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove armored rigs from parameter data
    /// </summary>
    /// <param name="templateEquipment">Equpiment to filter TacticalVest of</param>
    /// <param name="botRole">Role of bot vests are being filtered for</param>
    /// <param name="allowEmptyRequest">Should the function return all rigs when 0 unarmored are found</param>
    public void FilterRigsTothoseWithoutProtection(Equipment templateEquipment, string botRole, bool allowEmptyRequest = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add a piece of equipment with mods to inventory from the provided pools
    /// </summary>
    /// <param name="settings">Values to adjust how item is chosen and added to bot</param>
    /// <returns>true when item added</returns>
    public bool GenerateEquipment(GenerateEquipmentProperties settings)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get all possible mods for item and filter down based on equipment blacklist from bot.json config
    /// </summary>
    /// <param name="itemTpl">Item mod pool is being retrieved and filtered</param>
    /// <param name="equipmentBlacklist">Blacklist to filter mod pool with</param>
    /// <returns>Filtered pool of mods</returns>
    public Dictionary<string, List<string>> GetFilteredDynamicModsForItem(string itemTpl, Dictionary<string, List<string>> equipmentBlacklist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Work out what weapons bot should have equipped and add them to bot inventory
    /// </summary>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="equipmentChances">Chances bot can have equipment equipped</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="botInventory">Inventory to add weapons to</param>
    /// <param name="botRole">assault/pmcBot/bossTagilla etc</param>
    /// <param name="isPmc">Is the bot being generated as a pmc</param>
    /// <param name="itemGenerationLimitsMinMax">Limits for items the bot can have</param>
    /// <param name="botLevel">level of bot having weapon generated</param>
    public void GenerateAndAddWeaponsToBot(BotBaseInventory templateInventory, Chances equipmentChances, string sessionId, BotBaseInventory botInventory,
        string botRole,
        bool isPmc, Generation itemGenerationLimitsMinMax, int botLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate if the bot should have weapons in Primary/Secondary/Holster slots
    /// </summary>
    /// <param name="equipmentChances">Chances bot has certain equipment</param>
    /// <returns>What slots bot should have weapons generated for</returns>
    public object GetDesiredWeaponsForBot(Chances equipmentChances) // TODO: Type fuckery { slot: EquipmentSlots; shouldSpawn: boolean }[]
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add weapon + spare mags/ammo to bots inventory
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="weaponSlot">Weapon slot being generated</param>
    /// <param name="templateInventory">bot/x.json data from db</param>
    /// <param name="botInventory">Inventory to add weapon+mags/ammo to</param>
    /// <param name="equipmentChances">Chances bot can have equipment equipped</param>
    /// <param name="botRole">assault/pmcBot/bossTagilla etc</param>
    /// <param name="isPmc">Is the bot being generated as a pmc</param>
    /// <param name="itemGenerationWeights"></param>
    /// <param name="botLevel"></param>
    public void AddWeaponAndMagazineToInventory(string sessionId, object weaponSlot, BotBaseInventory templateInventory, BotBaseInventory botInventory,
        Chances equipmentChances, string botRole,
        bool isPmc, Generation itemGenerationWeights, int botLevel)
    {
        throw new NotImplementedException();
    }
}