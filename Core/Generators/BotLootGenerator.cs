using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;

namespace Core.Generators;

[Injectable]
public class BotLootGenerator
{
    private BotConfig _botConfig;
    private PmcConfig _pmcConfig;

    public BotLootGenerator()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botRole"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ItemSpawnLimitSettings GetItemSpawnLimitsForBot(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add loot to bots containers
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Base json db file for the bot having its loot generated</param>
    /// <param name="isPmc">Will bot be a pmc</param>
    /// <param name="botRole">Role of bot, e.g. asssult</param>
    /// <param name="botInventory">Inventory to add loot to</param>
    /// <param name="botLevel">Level of bot</param>
    public void GenerateLoot(string sessionId, BotType botJsonTemplate, bool isPmc, string botRole, BotBaseInventory botInventory, int botLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the rouble cost total for loot in a bots backpack by the bots levl
    /// Will return 0 for non PMCs
    /// </summary>
    /// <param name="botLevel">Bots level</param>
    /// <param name="isPmc">Is the bot a PMC</param>
    /// <returns>int</returns>
    public int GetBackpackRoubleTotalByLevel(int botLevel, bool isPmc)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botInventory"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<EquipmentSlots> GetAvailableContainersBotCanStoreItemsIn(BotBaseInventory botInventory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Force healing items onto bot to ensure they can heal in-raid
    /// </summary>
    /// <param name="botInventory">Inventory to add items to</param>
    /// <param name="botRole">Role of bot (pmcBEAR/pmcUSEC)</param>
    public void AddForcedMedicalItemsToPmcSecure(BotBaseInventory botInventory, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Take random items from a pool and add to an inventory until totalItemCount or totalValueLimit or space limit is reached
    /// </summary>
    /// <param name="pool">Pool of items to pick from with weight</param>
    /// <param name="equipmentSlots">What equipment slot will the loot items be added to</param>
    /// <param name="totalItemCount">Max count of items to add</param>
    /// <param name="inventoryToAddItemsTo">Bot inventory loot will be added to</param>
    /// <param name="botRole">Role of the bot loot is being generated for (assault/pmcbot)</param>
    /// <param name="itemSpawnLimits">Item spawn limits the bot must adhere to</param>
    /// <param name="containersIdFull"></param>
    /// <param name="totalValueLimitRub">Total value of loot allowed in roubles</param>
    /// <param name="isPmc">Is bot being generated for a pmc</param>
    /// <exception cref="NotImplementedException"></exception>
    public void AddLootFromPool(Dictionary<string, int> pool, List<string> equipmentSlots, int totalItemCount,
        BotBaseInventory inventoryToAddItemsTo, // TODO: type for containersIdFull was Set<string>
        string botRole, ItemSpawnLimitSettings? itemSpawnLimits, List<string> containersIdFull, int totalValueLimitRub = 0, bool isPmc = false)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="walletId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<List<Item>> CrateWalletLoot(string walletId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Some items need child items to function, add them to the itemToAddChildrenTo array
    /// </summary>
    /// <param name="itemToAddTemplate">Db template of item to check</param>
    /// <param name="itemToAddChildrenTo">Item to add children to</param>
    /// <param name="isPmc">Is the item being generated for a pmc (affects money/ammo stack sizes)</param>
    /// <param name="botRole">role bot has that owns item</param>
    public void AddRequiredChildItemsToParent(TemplateItem itemToAddTemplate, List<Item> itemToAddChildrenTo, bool isPmc, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add generated weapons to inventory as loot
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="botInventory">inventory to add preset to</param>
    /// <param name="equipmentSlot">slot to place the preset in (backpack)</param>
    /// <param name="templateInventory">bots template, assault.json</param>
    /// <param name="modsChances">chances for mods to spawn on weapon</param>
    /// <param name="botRole">bots role .e.g. pmcBot</param>
    /// <param name="isPmc">are we generating for a pmc</param>
    /// <param name="botLevel"></param>
    /// <param name="containersIdFull"></param>
    public void AddLooseWeaponsToInventorySlot(string sessionId, BotBaseInventory botInventory, string equipmentSlot,
        BotBaseInventory templateInventory, // TODO: type for containersIdFull was Set<string>
        Dictionary<string, double> modsChances, string botRole, bool isPmc, int botLevel, List<string>? containersIdFull)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Hydrate item limit array to contain items that have a limit for a specific bot type
    /// All values are set to 0
    /// </summary>
    /// <param name="botRole">Role the bot has</param>
    /// <param name="limitCount"></param>
    public void InitItemLimitArray(string botRole, Dictionary<string, int> limitCount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if an item has reached its bot-specific spawn limit
    /// </summary>
    /// <param name="itemTemplate">Item we check to see if its reached spawn limit</param>
    /// <param name="botRole">Bot type</param>
    /// <param name="itemSpawnLimits"></param>
    /// <returns>true if item has reached spawn limit</returns>
    public bool ItemHasReachedSpawnLimit(TemplateItem itemTemplate, string botRole, ItemSpawnLimitSettings itemSpawnLimits)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the stack size of a money object, uses different values for pmc or scavs
    /// </summary>
    /// <param name="botRole">Role bot has that has money stack</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="moneyItem">Money item to randomise</param>
    public void RandomiseMoneyStackSize(string botRole, TemplateItem itemTemplate, Item moneyItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Randomise the size of an ammo stack
    /// </summary>
    /// <param name="isPmc">Is ammo on a PMC bot</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="ammoItem">Ammo item to randomise</param>
    public void RandomiseAmmoStackSize(bool isPmc, TemplateItem itemTemplate, Item ammoItem)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get spawn limits for a specific bot type from bot.json config
    /// If no limit found for a non pmc bot, fall back to defaults
    /// </summary>
    /// <param name="botRole">what role does the bot have</param>
    /// <returns>Dictionary of tplIds and limit</returns>
    public Dictionary<string, int> GetItemSpawnLimitsForBotType(string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the parentId or tplId of item inside spawnLimits object if it exists
    /// </summary>
    /// <param name="itemTemplate">item we want to look for in spawn limits</param>
    /// <param name="spawnLimits">Limits to check for item</param>
    /// <returns>id as string, otherwise undefined</returns>
    public string? GetMatchingIdFromSpawnLimits(TemplateItem itemTemplate, Dictionary<string, int> spawnLimits)
    {
        throw new NotImplementedException();
    }
}
