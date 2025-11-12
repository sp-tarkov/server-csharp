using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Common.Models.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class BotLootGenerator(
    ISptLogger<BotLootGenerator> logger,
    RandomUtil randomUtil,
    ItemHelper itemHelper,
    InventoryHelper inventoryHelper,
    HandbookHelper handbookHelper,
    BotGeneratorHelper botGeneratorHelper,
    BotWeaponGenerator botWeaponGenerator,
    WeightedRandomHelper weightedRandomHelper,
    BotHelper botHelper,
    BotLootCacheService botLootCacheService,
    ServerLocalisationService serverLocalisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly PmcConfig PMCConfig = configServer.GetConfig<PmcConfig>();

    /// <summary>
    /// Get a dictionary of item tpls and the number of times they can be spawned on a single bot
    /// Keyed by bot type
    /// </summary>
    /// <param name="botRole">Role of bot to get limits for</param>
    /// <returns>Item spawn limits</returns>
    protected ItemSpawnLimitSettings GetItemSpawnLimitsForBot(string botRole)
    {
        // Clone limits and set all values to 0 to use as a running total
        var limitsForBotDictClone = cloner.Clone(GetItemSpawnLimitsForBotType(botRole));
        // Init current count of items we want to limit
        foreach (var limit in limitsForBotDictClone)
        {
            limitsForBotDictClone[limit.Key] = 0;
        }

        return new ItemSpawnLimitSettings { CurrentLimits = limitsForBotDictClone, GlobalLimits = GetItemSpawnLimitsForBotType(botRole) };
    }

    /// <summary>
    ///     Add loot to bots containers
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="sessionId">Session id</param>
    /// <param name="botJsonTemplate">Clone of Base JSON db file for the bot having its loot generated</param>
    /// <param name="botGenerationDetails">Details relating to generating a bot</param>
    /// <param name="botInventory">Inventory to add loot to</param>
    public void GenerateLoot(
        MongoId botId,
        MongoId sessionId,
        BotType botJsonTemplate,
        BotGenerationDetails botGenerationDetails,
        BotBaseInventory botInventory
    )
    {
        // Limits on item types to be added as loot
        var itemCounts = botJsonTemplate.BotGeneration?.Items;

        if (
            itemCounts?.BackpackLoot.Weights is null
            || itemCounts.PocketLoot.Weights is null
            || itemCounts.VestLoot.Weights is null
            || itemCounts.SpecialItems.Weights is null
            || itemCounts.Healing.Weights is null
            || itemCounts.Drugs.Weights is null
            || itemCounts.Food.Weights is null
            || itemCounts.Drink.Weights is null
            || itemCounts.Currency.Weights is null
            || itemCounts.Stims.Weights is null
            || itemCounts.Grenades.Weights is null
        )
        {
            logger.Warning(serverLocalisationService.GetText("bot-unable_to_generate_bot_loot", botGenerationDetails.RoleLowercase));
            return;
        }

        var backpackLootCount = weightedRandomHelper.GetWeightedValue(itemCounts.BackpackLoot.Weights);
        var pocketLootCount = weightedRandomHelper.GetWeightedValue(itemCounts.PocketLoot.Weights);
        var vestLootCount = weightedRandomHelper.GetWeightedValue(itemCounts.VestLoot.Weights);
        var specialLootItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.SpecialItems.Weights);
        var healingItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Healing.Weights);
        var drugItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Drugs.Weights);
        var foodItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Food.Weights);
        var drinkItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Drink.Weights);
        var currencyItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Currency.Weights);
        var stimItemCount = weightedRandomHelper.GetWeightedValue(itemCounts.Stims.Weights);
        var grenadeCount = weightedRandomHelper.GetWeightedValue(itemCounts.Grenades.Weights);

        // If bot has been flagged as not having loot, set below counts to 0
        if (BotConfig.DisableLootOnBotTypes.Contains(botGenerationDetails.RoleLowercase))
        {
            backpackLootCount = 0;
            pocketLootCount = 0;
            vestLootCount = 0;
            currencyItemCount = 0;
        }

        // Forced pmc healing loot into secure container
        if (botGenerationDetails.IsPmc && PMCConfig.ForceHealingItemsIntoSecure)
        {
            AddForcedMedicalItemsToPmcSecure(botInventory, botGenerationDetails.RoleLowercase, botId);
        }

        var botItemLimits = GetItemSpawnLimitsForBot(botGenerationDetails.RoleLowercase);

        var containersBotHasAvailable = GetAvailableContainersBotCanStoreItemsIn(botInventory);

        // Special items
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.Special,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            specialLootItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            botItemLimits
        );

        // Healing items / Meds
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.HealingItems,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            healingItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            null,
            0,
            botGenerationDetails.IsPmc
        );

        // Drugs
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.DrugItems,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            drugItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            null,
            0,
            botGenerationDetails.IsPmc
        );

        // Food
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.FoodItems,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            foodItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            null,
            0,
            botGenerationDetails.IsPmc
        );

        // Drink
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.DrinkItems,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            drinkItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            null,
            0,
            botGenerationDetails.IsPmc
        );

        // Currency
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.CurrencyItems,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            currencyItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            null,
            0,
            botGenerationDetails.IsPmc
        );

        // Stims
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.StimItems,
                botJsonTemplate
            ),
            containersBotHasAvailable,
            stimItemCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            botItemLimits,
            0,
            botGenerationDetails.IsPmc
        );

        // Grenades
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.GrenadeItems,
                botJsonTemplate
            ),
            [EquipmentSlots.Pockets, EquipmentSlots.TacticalVest], // Can't use containersBotHasEquipped as we don't want grenades added to backpack
            grenadeCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            null,
            0,
            botGenerationDetails.IsPmc
        );

        var itemPriceLimits = GetSingleItemLootPriceLimits(botGenerationDetails.BotLevel, botGenerationDetails.IsPmc);

        // Backpack - generate loot if they have one
        if (containersBotHasAvailable.Contains(EquipmentSlots.Backpack) && backpackLootCount > 0)
        {
            // Add randomly generated weapon to PMC backpacks
            if (botGenerationDetails.IsPmc && randomUtil.GetChance100(PMCConfig.LooseWeaponInBackpackChancePercent))
            {
                AddLooseWeaponsToInventorySlot(
                    botId,
                    sessionId,
                    botInventory,
                    EquipmentSlots.Backpack,
                    botGenerationDetails,
                    botJsonTemplate.BotInventory,
                    botJsonTemplate.BotChances?.WeaponModsChances
                );
            }

            var backpackLootRoubleTotal = botGenerationDetails.IsPmc
                ? PMCConfig.LootSettings.Backpack.GetRoubleValue(botGenerationDetails.BotLevel, botGenerationDetails.Location)
                : 0;

            AddLootFromPool(
                botId,
                botLootCacheService.GetLootFromCache(
                    botGenerationDetails.RoleLowercase,
                    botGenerationDetails.IsPmc,
                    LootCacheType.Backpack,
                    botJsonTemplate,
                    itemPriceLimits?.Backpack
                ),
                [EquipmentSlots.Backpack],
                backpackLootCount,
                botInventory,
                botGenerationDetails.RoleLowercase,
                botItemLimits,
                backpackLootRoubleTotal,
                botGenerationDetails.IsPmc
            );
        }

        var vestLootRoubleTotal = botGenerationDetails.IsPmc
            ? PMCConfig.LootSettings.Vest.GetRoubleValue(botGenerationDetails.BotLevel, botGenerationDetails.Location)
            : 0;

        // TacticalVest - generate loot if they have one
        if (containersBotHasAvailable.Contains(EquipmentSlots.TacticalVest))
        // Vest
        {
            AddLootFromPool(
                botId,
                botLootCacheService.GetLootFromCache(
                    botGenerationDetails.RoleLowercase,
                    botGenerationDetails.IsPmc,
                    LootCacheType.Vest,
                    botJsonTemplate,
                    itemPriceLimits?.Vest
                ),
                [EquipmentSlots.TacticalVest],
                vestLootCount,
                botInventory,
                botGenerationDetails.RoleLowercase,
                botItemLimits,
                vestLootRoubleTotal,
                botGenerationDetails.IsPmc
            );
        }

        var pocketLootRoubleTotal = botGenerationDetails.IsPmc
            ? PMCConfig.LootSettings.Pocket.GetRoubleValue(botGenerationDetails.BotLevel, botGenerationDetails.Location)
            : 0;

        // Pockets
        AddLootFromPool(
            botId,
            botLootCacheService.GetLootFromCache(
                botGenerationDetails.RoleLowercase,
                botGenerationDetails.IsPmc,
                LootCacheType.Pocket,
                botJsonTemplate,
                itemPriceLimits?.Pocket
            ),
            [EquipmentSlots.Pockets],
            pocketLootCount,
            botInventory,
            botGenerationDetails.RoleLowercase,
            botItemLimits,
            pocketLootRoubleTotal,
            botGenerationDetails.IsPmc
        );

        // Secure

        // only add if not a pmc or is pmc and flag is true
        if (!botGenerationDetails.IsPmc || (botGenerationDetails.IsPmc && PMCConfig.AddSecureContainerLootFromBotConfig))
        {
            AddLootFromPool(
                botId,
                botLootCacheService.GetLootFromCache(
                    botGenerationDetails.RoleLowercase,
                    botGenerationDetails.IsPmc,
                    LootCacheType.Secure,
                    botJsonTemplate
                ),
                [EquipmentSlots.SecuredContainer],
                50,
                botInventory,
                botGenerationDetails.RoleLowercase,
                null,
                -1,
                botGenerationDetails.IsPmc
            );
        }
    }

    protected MinMaxLootItemValue? GetSingleItemLootPriceLimits(int botLevel, bool isPmc)
    {
        // TODO - extend to other bot types
        if (!isPmc)
        {
            return null;
        }

        var matchingValue = PMCConfig?.LootItemLimitsRub.FirstOrDefault(minMaxValue =>
            botLevel >= minMaxValue.Min && botLevel <= minMaxValue.Max
        );

        return matchingValue;
    }

    /// <summary>
    ///     Get an array of the containers a bot has on them (pockets/backpack/vest)
    /// </summary>
    /// <param name="botInventory">Bot to check</param>
    /// <returns>Array of available slots</returns>
    protected HashSet<EquipmentSlots> GetAvailableContainersBotCanStoreItemsIn(BotBaseInventory botInventory)
    {
        HashSet<EquipmentSlots> result = [EquipmentSlots.Pockets];

        if ((botInventory.Items ?? []).Any(item => item.SlotId == nameof(EquipmentSlots.TacticalVest)))
        {
            result.Add(EquipmentSlots.TacticalVest);
        }

        if ((botInventory.Items ?? []).Any(item => item.SlotId == nameof(EquipmentSlots.Backpack)))
        {
            result.Add(EquipmentSlots.Backpack);
        }

        return result;
    }

    /// <summary>
    ///     Force healing items onto bot to ensure they can heal in-raid
    /// </summary>
    /// <param name="botInventory">Inventory to add items to</param>
    /// <param name="botRole">Role of bot (pmcBEAR/pmcUSEC)</param>
    /// <param name="botId">Bots unique identifier</param>
    protected void AddForcedMedicalItemsToPmcSecure(BotBaseInventory botInventory, string botRole, MongoId botId)
    {
        // surv12
        AddLootFromPool(
            botId,
            new Dictionary<MongoId, double> { { ItemTpl.MEDICAL_SURV12_FIELD_SURGICAL_KIT, 1 } },
            [EquipmentSlots.SecuredContainer],
            1,
            botInventory,
            botRole,
            null,
            0,
            true
        );

        // AFAK
        var afaks = new Dictionary<MongoId, double> { { ItemTpl.MEDKIT_AFAK_TACTICAL_INDIVIDUAL_FIRST_AID_KIT, 1 } };
        AddLootFromPool(botId, afaks, [EquipmentSlots.SecuredContainer], 10, botInventory, botRole, null, 0, true);
    }

    /// <summary>
    ///     Take random items from a pool and add to an inventory until totalItemCount or totalValueLimit or space limit is reached
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="pool">Pool of items to pick from with weight</param>
    /// <param name="equipmentSlots">What equipment slot will the loot items be added to</param>
    /// <param name="totalItemCount">Max count of items to add</param>
    /// <param name="inventoryToAddItemsTo">Bot inventory loot will be added to</param>
    /// <param name="botRole">Role of the bot loot is being generated for (assault/pmcbot)</param>
    /// <param name="itemSpawnLimits">Item spawn limits the bot must adhere to</param>
    /// <param name="totalValueLimitRub">Total value of loot allowed in roubles</param>
    /// <param name="isPmc">Is bot being generated for a pmc</param>
    protected internal void AddLootFromPool(
        MongoId botId,
        Dictionary<MongoId, double> pool,
        HashSet<EquipmentSlots> equipmentSlots,
        double totalItemCount,
        BotBaseInventory inventoryToAddItemsTo,
        string botRole,
        ItemSpawnLimitSettings? itemSpawnLimits,
        double totalValueLimitRub = 0,
        bool isPmc = false
    )
    {
        // Loot pool has items
        if (pool.Count <= 0)
        {
            return;
        }

        double currentTotalRub = 0;

        var fitItemIntoContainerAttempts = 0;
        for (var i = 0; i < totalItemCount; i++)
        {
            // Pool can become empty if item spawn limits keep removing items
            if (pool.Count == 0)
            {
                return;
            }

            var weightedItemTpl = weightedRandomHelper.GetWeightedValue(pool);
            var (key, itemToAddTemplate) = itemHelper.GetItem(weightedItemTpl);

            if (!key)
            {
                logger.Warning($"Unable to process item tpl: {weightedItemTpl} for slots: {equipmentSlots} on bot: {botRole}");

                continue;
            }

            if (itemSpawnLimits is not null && ItemHasReachedSpawnLimit(itemToAddTemplate, botRole, itemSpawnLimits))
            {
                // Remove item from pool to prevent it being picked again
                pool.Remove(weightedItemTpl);

                i--;
                continue;
            }

            var newRootItemId = new MongoId();
            List<Item> itemWithChildrenToAdd =
            [
                new()
                {
                    Id = newRootItemId,
                    Template = itemToAddTemplate?.Id ?? MongoId.Empty(),
                    Upd = botGeneratorHelper.GenerateExtraPropertiesForItem(itemToAddTemplate, botRole, true),
                },
            ];

            // Is Simple-Wallet / WZ wallet
            if (BotConfig.WalletLoot.WalletTplPool.Contains(weightedItemTpl))
            {
                var addCurrencyToWallet = randomUtil.GetChance100(BotConfig.WalletLoot.ChancePercent);
                if (addCurrencyToWallet)
                {
                    // Create the currency items we want to add to wallet
                    var itemsToAdd = CreateWalletLoot(newRootItemId);

                    // Get the container grid for the wallet
                    var containerGrid = inventoryHelper.GetContainerSlotMap(weightedItemTpl);

                    // Check if all the chosen currency items fit into wallet
                    var canAddToContainer = inventoryHelper.CanPlaceItemsInContainer(
                        cloner.Clone(containerGrid), // MUST clone grid before passing in as function modifies grid
                        itemsToAdd
                    );
                    if (canAddToContainer)
                    {
                        // Add each currency to wallet
                        foreach (var itemToAdd in itemsToAdd)
                        {
                            inventoryHelper.PlaceItemInContainer(containerGrid, itemToAdd, itemWithChildrenToAdd[0].Id, "main");
                        }

                        itemWithChildrenToAdd.AddRange(itemsToAdd.SelectMany(x => x));
                    }
                }
            }

            // Some items (ammoBox/ammo) need extra changes
            AddRequiredChildItemsToParent(itemToAddTemplate, itemWithChildrenToAdd, isPmc, botRole);

            // Attempt to add item to container(s)
            var itemAddedResult = botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                botId,
                equipmentSlots,
                newRootItemId,
                itemToAddTemplate.Id,
                itemWithChildrenToAdd,
                inventoryToAddItemsTo
            );

            // Handle when fitting item fails
            if (itemAddedResult != ItemAddedResult.SUCCESS)
            {
                if (itemAddedResult == ItemAddedResult.NO_CONTAINERS)
                {
                    // Bot has no container to put item in, exit
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Unable to add: {totalItemCount} items to bot as it lacks a container to include them");
                    }

                    break;
                }

                fitItemIntoContainerAttempts++;
                if (fitItemIntoContainerAttempts >= 4)
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            $"Failed placing item: {itemToAddTemplate.Id} - {itemToAddTemplate.Name}: {i} of: {totalItemCount} items into: {botRole} "
                                + $"containers: {string.Join(",", equipmentSlots)}. Tried: {fitItemIntoContainerAttempts} "
                                + $"times, reason: {itemAddedResult}, skipping"
                        );
                    }

                    break;
                }

                // Try again, failed but still under attempt limit
                continue;
            }

            // Item added okay, reset counter for next item
            fitItemIntoContainerAttempts = 0;

            // Stop adding items to bots pool if rolling total is over total limit
            if (totalValueLimitRub > 0)
            {
                currentTotalRub += handbookHelper.GetTemplatePrice(itemToAddTemplate.Id);
                if (currentTotalRub > totalValueLimitRub)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    ///     Adds loot to the specified Wallet
    /// </summary>
    /// <param name="walletId"> Wallet to add loot to</param>
    /// <returns>Generated list of currency stacks with the wallet as their parent</returns>
    public List<List<Item>> CreateWalletLoot(MongoId walletId)
    {
        List<List<Item>> result = [];

        // Choose how many stacks of currency will be added to wallet
        var itemCount = randomUtil.GetInt(BotConfig.WalletLoot.ItemCount.Min, BotConfig.WalletLoot.ItemCount.Max);
        for (var index = 0; index < itemCount; index++)
        {
            // Choose the size of the currency stack - default is 5k, 10k, 15k, 20k, 25k
            var chosenStackCount = weightedRandomHelper.GetWeightedValue(BotConfig.WalletLoot.StackSizeWeight);
            List<Item> items =
            [
                new()
                {
                    Id = new MongoId(),
                    Template = weightedRandomHelper.GetWeightedValue(BotConfig.WalletLoot.CurrencyWeight),
                    ParentId = walletId,
                    Upd = new Upd { StackObjectsCount = int.Parse(chosenStackCount) },
                },
            ];
            result.Add(items);
        }

        return result;
    }

    /// <summary>
    ///     Some items need child items to function, add them to the itemToAddChildrenTo array
    /// </summary>
    /// <param name="itemToAddTemplate">Db template of item to check</param>
    /// <param name="itemToAddChildrenTo">Item to add children to</param>
    /// <param name="isPmc">Is the item being generated for a pmc (affects money/ammo stack sizes)</param>
    /// <param name="botRole">role bot has that owns item</param>
    public void AddRequiredChildItemsToParent(TemplateItem? itemToAddTemplate, List<Item> itemToAddChildrenTo, bool isPmc, string botRole)
    {
        // Fill ammo box
        if (itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.AMMO_BOX))
        {
            itemHelper.AddCartridgesToAmmoBox(itemToAddChildrenTo, itemToAddTemplate);
        }
        // Make money a stack
        else if (itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.MONEY))
        {
            RandomiseMoneyStackSize(botRole, itemToAddTemplate, itemToAddChildrenTo[0]);
        }
        // Make ammo a stack
        else if (itemHelper.IsOfBaseclass(itemToAddTemplate.Id, BaseClasses.AMMO))
        {
            RandomiseAmmoStackSize(isPmc, itemToAddTemplate, itemToAddChildrenTo[0]);
        }
        // Must add soft inserts/plates
        else if (itemHelper.ItemRequiresSoftInserts(itemToAddTemplate.Id))
        {
            itemHelper.AddChildSlotItems(itemToAddChildrenTo, itemToAddTemplate);
        }
    }

    /// <summary>
    ///     Add generated weapons to inventory as loot
    /// </summary>
    /// <param name="botId">Bots unique identifier</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="botInventory">Inventory to add preset to</param>
    /// <param name="equipmentSlot">Slot to place the preset in (backpack)</param>
    /// <param name="botGenerationDetails"></param>
    /// <param name="templateInventory">Bots template, assault.json</param>
    /// <param name="modChances">Chances for mods to spawn on weapon</param>
    public void AddLooseWeaponsToInventorySlot(
        MongoId botId,
        MongoId sessionId,
        BotBaseInventory botInventory,
        EquipmentSlots equipmentSlot,
        BotGenerationDetails botGenerationDetails,
        BotTypeInventory? templateInventory,
        Dictionary<string, double> modChances
    )
    {
        var chosenWeaponType = randomUtil.GetArrayValue<string>(
            [
                nameof(EquipmentSlots.FirstPrimaryWeapon),
                nameof(EquipmentSlots.FirstPrimaryWeapon),
                nameof(EquipmentSlots.FirstPrimaryWeapon),
                nameof(EquipmentSlots.Holster),
            ]
        );
        var randomisedWeaponCount = randomUtil.GetInt(
            PMCConfig.LooseWeaponInBackpackLootMinMax.Min,
            PMCConfig.LooseWeaponInBackpackLootMinMax.Max
        );

        if (randomisedWeaponCount <= 0)
        {
            return;
        }

        for (var i = 0; i < randomisedWeaponCount; i++)
        {
            var generatedWeapon = botWeaponGenerator.GenerateRandomWeapon(
                sessionId,
                chosenWeaponType,
                templateInventory,
                botGenerationDetails,
                botInventory.Equipment.Value,
                modChances
            );

            var weaponRootItem = generatedWeapon.Weapon?.FirstOrDefault();
            if (weaponRootItem is null)
            {
                logger.Error(
                    $"Generated null loose weapon: {chosenWeaponType} for: {botGenerationDetails.RoleLowercase} level: {botGenerationDetails.BotLevel}, skipping"
                );

                continue;
            }
            var result = botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                botId,
                [equipmentSlot],
                weaponRootItem.Id,
                weaponRootItem.Template,
                generatedWeapon.Weapon,
                botInventory
            );

            if (result != ItemAddedResult.SUCCESS)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Failed to add additional weapon: {weaponRootItem.Id} to bot backpack, reason: {result.ToString()}");
                }
            }
        }
    }

    /// <summary>
    ///     Check if an item has reached its bot-specific spawn limit
    /// </summary>
    /// <param name="itemTemplate">Item we check to see if its reached spawn limit</param>
    /// <param name="botRole">Bot type</param>
    /// <param name="itemSpawnLimits"></param>
    /// <returns>true if item has reached spawn limit</returns>
    protected bool ItemHasReachedSpawnLimit(TemplateItem? itemTemplate, string botRole, ItemSpawnLimitSettings? itemSpawnLimits)
    {
        // PMCs and scavs have different sections of bot config for spawn limits
        if (itemSpawnLimits is not null && itemSpawnLimits.GlobalLimits?.Count == 0)
        // No items found in spawn limit, drop out
        {
            return false;
        }

        // No spawn limits, skipping
        if (itemSpawnLimits is null)
        {
            return false;
        }

        var idToCheckFor = GetMatchingIdFromSpawnLimits(itemTemplate, itemSpawnLimits.GlobalLimits);
        if (idToCheckFor is null)
        // ParentId or tplid not found in spawnLimits, not a spawn limited item, skip
        {
            return false;
        }

        // Use tryAdd to see if it exists, and automatically add 1
        if (!itemSpawnLimits.CurrentLimits.TryAdd(idToCheckFor.Value, 1))
        // if it does exist, come in here and increment item count with this bot type
        {
            itemSpawnLimits.CurrentLimits[idToCheckFor.Value]++;
        }

        // Check if over limit
        var currentLimitCount = itemSpawnLimits.CurrentLimits[idToCheckFor.Value];
        if (itemSpawnLimits.CurrentLimits[idToCheckFor.Value] > itemSpawnLimits.GlobalLimits[idToCheckFor.Value])
        {
            // Prevent edge-case of small loot pools + code trying to add limited item over and over infinitely
            if (currentLimitCount > currentLimitCount * 10)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug(
                        serverLocalisationService.GetText(
                            "bot-item_spawn_limit_reached_skipping_item",
                            new
                            {
                                botRole,
                                itemName = itemTemplate.Name,
                                attempts = currentLimitCount,
                            }
                        )
                    );
                }

                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Randomise the stack size of a money object, uses different values for pmc or scavs
    /// </summary>
    /// <param name="botRole">Role bot has that has money stack</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="moneyItem">Money item to randomise</param>
    public void RandomiseMoneyStackSize(string botRole, TemplateItem itemTemplate, Item moneyItem)
    {
        // Get all currency weights for this bot type
        if (!BotConfig.CurrencyStackSize.TryGetValue(botRole, out var currencyWeights))
        {
            currencyWeights = BotConfig.CurrencyStackSize["default"];
        }

        var currencyWeight = currencyWeights[moneyItem.Template];

        moneyItem.AddUpd();

        moneyItem.Upd.StackObjectsCount = int.Parse(weightedRandomHelper.GetWeightedValue(currencyWeight));
    }

    /// <summary>
    ///     Randomise the size of an ammo stack
    /// </summary>
    /// <param name="isPmc">Is ammo on a PMC bot</param>
    /// <param name="itemTemplate">item details from db</param>
    /// <param name="ammoItem">Ammo item to randomise</param>
    public void RandomiseAmmoStackSize(bool isPmc, TemplateItem itemTemplate, Item ammoItem)
    {
        var randomSize = itemHelper.GetRandomisedAmmoStackSize(itemTemplate);
        ammoItem.AddUpd();

        ammoItem.Upd.StackObjectsCount = randomSize;
    }

    /// <summary>
    ///     Get spawn limits for a specific bot type from bot.json config
    ///     If no limit found for a non pmc bot, fall back to defaults
    /// </summary>
    /// <param name="botRole">what role does the bot have</param>
    /// <returns>Dictionary of tplIds and limit</returns>
    public Dictionary<MongoId, double> GetItemSpawnLimitsForBotType(string botRole)
    {
        if (botHelper.IsBotPmc(botRole))
        {
            return BotConfig.ItemSpawnLimits["pmc"];
        }

        if (BotConfig.ItemSpawnLimits.ContainsKey(botRole.ToLowerInvariant()))
        {
            return BotConfig.ItemSpawnLimits[botRole.ToLowerInvariant()];
        }

        logger.Warning(serverLocalisationService.GetText("bot-unable_to_find_spawn_limits_fallback_to_defaults", botRole));

        return [];
    }

    /// <summary>
    ///     Get the parentId or tplId of item inside spawnLimits object if it exists
    /// </summary>
    /// <param name="itemTemplate">item we want to look for in spawn limits</param>
    /// <param name="spawnLimits">Limits to check for item</param>
    /// <returns>id as string, otherwise undefined</returns>
    public MongoId? GetMatchingIdFromSpawnLimits(TemplateItem itemTemplate, Dictionary<MongoId, double> spawnLimits)
    {
        if (spawnLimits.ContainsKey(itemTemplate.Id))
        {
            return itemTemplate.Id;
        }

        // tplId not found in spawnLimits, check if parentId is
        if (spawnLimits.ContainsKey(itemTemplate.Parent))
        {
            return itemTemplate.Parent;
        }

        // parentId and tplId not found
        return null;
    }
}
