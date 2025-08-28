using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class ScavCaseRewardGenerator(
    ISptLogger<ScavCaseRewardGenerator> logger,
    RandomUtil randomUtil,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    DatabaseService databaseService,
    RagfairPriceService ragfairPriceService,
    SeasonalEventService seasonalEventService,
    ItemFilterService itemFilterService,
    ServerLocalisationService localisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected List<TemplateItem> _dbAmmoItemsCache = [];
    protected List<TemplateItem> _dbItemsCache = [];
    protected readonly ScavCaseConfig _scavCaseConfig = configServer.GetConfig<ScavCaseConfig>();

    /// <summary>
    ///     Create an array of rewards that will be given to the player upon completing their scav case build
    /// </summary>
    /// <param name="recipeId">recipe of the scav case craft</param>
    /// <returns>Product array</returns>
    public IEnumerable<List<Item>> Generate(MongoId recipeId)
    {
        CacheDbItems();

        // Get scavcase details from hideout/scavcase.json
        var scavCaseDetails = databaseService.GetHideout().Production.ScavRecipes.FirstOrDefault(r => r.Id == recipeId);
        var rewardItemCounts = GetScavCaseRewardCountsAndPrices(scavCaseDetails);

        // Get items that fit the price criteria as set by the scavCase config
        var commonPricedItems = GetFilteredItemsByPrice(_dbItemsCache, rewardItemCounts.Common);
        var rarePricedItems = GetFilteredItemsByPrice(_dbItemsCache, rewardItemCounts.Rare);
        var superRarePricedItems = GetFilteredItemsByPrice(_dbItemsCache, rewardItemCounts.Superrare);

        // Get randomly picked items from each item collection, the count range of which is defined in hideout/scavcase.json
        var randomlyPickedCommonRewards = PickRandomRewards(commonPricedItems, rewardItemCounts.Common, RewardRarity.Common);

        var randomlyPickedRareRewards = PickRandomRewards(rarePricedItems, rewardItemCounts.Rare, RewardRarity.Rare);

        var randomlyPickedSuperRareRewards = PickRandomRewards(superRarePricedItems, rewardItemCounts.Superrare, RewardRarity.SuperRare);

        // Add randomised stack sizes to ammo and money rewards
        var commonRewards = RandomiseContainerItemRewards(randomlyPickedCommonRewards, RewardRarity.Common);
        var rareRewards = RandomiseContainerItemRewards(randomlyPickedRareRewards, RewardRarity.Rare);
        var superRareRewards = RandomiseContainerItemRewards(randomlyPickedSuperRareRewards, RewardRarity.SuperRare);

        var result = commonRewards.Concat(rareRewards).Concat(superRareRewards);

        return result;
    }

    /// <summary>
    ///     Get all db items that are not blacklisted in scavcase config or global blacklist
    ///     Store in class field
    /// </summary>
    protected void CacheDbItems()
    {
        // Get an array of seasonal items that should not be shown right now as seasonal event is not active
        var inactiveSeasonalItems = seasonalEventService.GetInactiveSeasonalEventItems();
        if (!_dbItemsCache.Any())
        {
            _dbItemsCache = databaseService
                .GetItems()
                .Values.Where(item =>
                {
                    // Base "Item" item has no parent, ignore it
                    if (item.Parent == MongoId.Empty())
                    {
                        return false;
                    }

                    if (item.Type == "Node")
                    {
                        return false;
                    }

                    if (item.Properties.QuestItem ?? false)
                    {
                        return false;
                    }

                    // Skip item if item id is on blacklist
                    if (
                        item.Type != "Item"
                        || _scavCaseConfig.RewardItemBlacklist.Contains(item.Id)
                        || itemFilterService.IsItemBlacklisted(item.Id)
                    )
                    {
                        return false;
                    }

                    // Globally reward-blacklisted
                    if (itemFilterService.IsItemRewardBlacklisted(item.Id))
                    {
                        return false;
                    }

                    if (!_scavCaseConfig.AllowBossItemsAsRewards && itemFilterService.IsBossItem(item.Id))
                    {
                        return false;
                    }

                    // Skip item if parent id is blacklisted
                    if (itemHelper.IsOfBaseclasses(item.Id, _scavCaseConfig.RewardItemParentBlacklist))
                    {
                        return false;
                    }

                    if (inactiveSeasonalItems.Contains(item.Id))
                    {
                        return false;
                    }

                    return true;
                })
                .ToList();
        }

        if (!_dbAmmoItemsCache.Any())
        {
            _dbAmmoItemsCache = databaseService
                .GetItems()
                .Values.Where(item =>
                {
                    // Base "Item" item has no parent, ignore it
                    if (item.Parent == MongoId.Empty())
                    {
                        return false;
                    }

                    if (item.Type != "Item")
                    {
                        return false;
                    }

                    // Not ammo, skip
                    if (!itemHelper.IsOfBaseclass(item.Id, BaseClasses.AMMO))
                    {
                        return false;
                    }

                    // Skip item if item id is on blacklist
                    if (_scavCaseConfig.RewardItemBlacklist.Contains(item.Id) || itemFilterService.IsItemBlacklisted(item.Id))
                    {
                        return false;
                    }

                    // Globally reward-blacklisted
                    if (itemFilterService.IsItemRewardBlacklisted(item.Id))
                    {
                        return false;
                    }

                    if (!_scavCaseConfig.AllowBossItemsAsRewards && itemFilterService.IsBossItem(item.Id))
                    {
                        return false;
                    }

                    // Skip seasonal items
                    if (inactiveSeasonalItems.Contains(item.Id))
                    {
                        return false;
                    }

                    // Skip ammo that doesn't stack as high as value in config
                    if (item.Properties.StackMaxSize < _scavCaseConfig.AmmoRewards.MinStackSize)
                    {
                        return false;
                    }

                    return true;
                })
                .ToList();
        }
    }

    /// <summary>
    ///     Pick a number of items to be rewards, the count is defined by the values in `itemFilters` param
    /// </summary>
    /// <param name="items">item pool to pick rewards from</param>
    /// <param name="itemFilters">how the rewards should be filtered down (by item count)</param>
    /// <param name="rarity">Rarity of reward</param>
    /// <returns></returns>
    protected List<TemplateItem> PickRandomRewards(List<TemplateItem> items, RewardCountAndPriceDetails itemFilters, string rarity)
    {
        List<TemplateItem> result = [];

        var rewardWasMoney = false;
        var rewardWasAmmo = false;
        var randomCount = randomUtil.GetInt((int)itemFilters.MinCount, (int)itemFilters.MaxCount);
        for (var i = 0; i < randomCount; i++)
        {
            if (RewardShouldBeMoney() && !rewardWasMoney)
            {
                // Only allow one reward to be money
                result.Add(GetRandomMoney());
                if (!_scavCaseConfig.AllowMultipleMoneyRewardsPerRarity)
                {
                    rewardWasMoney = true;
                }
            }
            else if (RewardShouldBeAmmo() && !rewardWasAmmo)
            {
                // Only allow one reward to be ammo
                result.Add(GetRandomAmmo(rarity));
                if (!_scavCaseConfig.AllowMultipleAmmoRewardsPerRarity)
                {
                    rewardWasAmmo = true;
                }
            }
            else
            {
                result.Add(randomUtil.GetArrayValue(items));
            }
        }

        return result;
    }

    /// <summary>
    ///     Choose if money should be a reward based on the moneyRewardChancePercent config chance in scavCaseConfig
    /// </summary>
    /// <returns>true if reward should be money</returns>
    protected bool RewardShouldBeMoney()
    {
        return randomUtil.GetChance100(_scavCaseConfig.MoneyRewards.MoneyRewardChancePercent);
    }

    /// <summary>
    ///     Choose if ammo should be a reward based on the ammoRewardChancePercent config chance in scavCaseConfig
    /// </summary>
    /// <returns>true if reward should be ammo</returns>
    protected bool RewardShouldBeAmmo()
    {
        return randomUtil.GetChance100(_scavCaseConfig.AmmoRewards.AmmoRewardChancePercent);
    }

    /// <summary>
    ///     Choose from rouble/dollar/euro at random
    /// </summary>
    protected TemplateItem GetRandomMoney()
    {
        List<TemplateItem> money = [];
        var items = databaseService.GetItems();
        money.Add(items[Money.ROUBLES]);
        money.Add(items[Money.EUROS]);
        money.Add(items[Money.DOLLARS]);
        money.Add(items[Money.GP]);

        return randomUtil.GetArrayValue(money);
    }

    /// <summary>
    ///     Get a random ammo from items.json that is not in the ammo blacklist AND inside the price range defined in scavcase.json config
    /// </summary>
    /// <param name="rarity">The rarity desired ammo reward is for</param>
    /// <returns>random ammo item from items.json</returns>
    protected TemplateItem GetRandomAmmo(string rarity)
    {
        var possibleAmmoPool = _dbAmmoItemsCache.Where(ammo =>
        {
            // Is ammo handbook price between desired range
            var handbookPrice = ragfairPriceService.GetStaticPriceForItem(ammo.Id);
            if (
                _scavCaseConfig.AmmoRewards.AmmoRewardValueRangeRub.TryGetValue(rarity, out var matchingAmmoRewardForRarity)
                && handbookPrice >= matchingAmmoRewardForRarity.Min
                && handbookPrice <= matchingAmmoRewardForRarity.Max
            )
            {
                return true;
            }

            return false;
        });

        if (!possibleAmmoPool.Any())
        {
            // Filtered pool is empty
            logger.Warning(localisationService.GetText("scavcase-no_cartridges_found_matching_price"));
        }

        // Get a random ammo and return it
        return randomUtil.GetArrayValue(possibleAmmoPool);
    }

    /// <summary>
    ///     Take all the rewards picked create the Product object array ready to return to calling code.
    ///     Also add a stack count to ammo and money
    /// </summary>
    /// <param name="rewardItems">items to convert</param>
    /// <param name="rarity">The rarity desired ammo reward is for</param>
    /// <returns>Product array</returns>
    protected List<List<Item>> RandomiseContainerItemRewards(IEnumerable<TemplateItem> rewardItems, string rarity)
    {
        // Each array is an item + children
        List<List<Item>> result = [];
        foreach (var rewardItemDb in rewardItems)
        {
            List<Item> resultItem =
            [
                new()
                {
                    Id = new MongoId(),
                    Template = rewardItemDb.Id,
                    Upd = null,
                },
            ];
            var rootItem = resultItem.FirstOrDefault();

            if (itemHelper.IsOfBaseclass(rewardItemDb.Id, BaseClasses.AMMO_BOX))
            {
                itemHelper.AddCartridgesToAmmoBox(resultItem, rewardItemDb);
            }
            // Armor or weapon = use default preset from globals.json
            else if (
                itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(rewardItemDb.Id)
                || itemHelper.IsOfBaseclass(rewardItemDb.Id, BaseClasses.WEAPON)
            )
            {
                var preset = presetHelper.GetDefaultPreset(rewardItemDb.Id);
                if (preset is null)
                {
                    logger.Warning($"No preset for item: {rewardItemDb.Id} {rewardItemDb.Name}, skipping");

                    continue;
                }

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = cloner.Clone(preset.Items).ReplaceIDs().ToList();
                presetAndMods.RemapRootItemId();

                resultItem = presetAndMods;
            }
            else if (itemHelper.IsOfBaseclasses(rewardItemDb.Id, [BaseClasses.AMMO, BaseClasses.MONEY]))
            {
                rootItem.Upd = new Upd { StackObjectsCount = GetRandomAmountRewardForScavCase(rewardItemDb, rarity) };
            }

            result.Add(resultItem);
        }

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="dbItems">all items from the items.json</param>
    /// <param name="itemFilters">controls how the dbItems will be filtered and returned (handbook price)</param>
    /// <returns>filtered dbItems array</returns>
    protected List<TemplateItem> GetFilteredItemsByPrice(List<TemplateItem> dbItems, RewardCountAndPriceDetails itemFilters)
    {
        return dbItems
            .Where(item =>
            {
                var handbookPrice = ragfairPriceService.GetStaticPriceForItem(item.Id);
                if (handbookPrice >= itemFilters.MinPriceRub && handbookPrice <= itemFilters.MaxPriceRub)
                {
                    return true;
                }

                return false;
            })
            .ToList();
    }

    /// <summary>
    ///     Gathers the reward min and max count params for each reward quality level from config and scavcase.json into a single object
    /// </summary>
    /// <param name="scavCaseDetails">production.json/scavRecipes object</param>
    /// <returns>ScavCaseRewardCountsAndPrices object</returns>
    protected ScavCaseRewardCountsAndPrices GetScavCaseRewardCountsAndPrices(ScavRecipe scavCaseDetails)
    {
        return new ScavCaseRewardCountsAndPrices
        {
            // Create reward min/max counts for each type
            Common = new RewardCountAndPriceDetails
            {
                MinCount = scavCaseDetails.EndProducts.Common.Min,
                MaxCount = scavCaseDetails.EndProducts.Common.Max,
                MinPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Common].Min,
                MaxPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Common].Max,
            },
            Rare = new RewardCountAndPriceDetails
            {
                MinCount = scavCaseDetails.EndProducts.Rare.Min,
                MaxCount = scavCaseDetails.EndProducts.Rare.Max,
                MinPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Rare].Min,
                MaxPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Rare].Max,
            },
            Superrare = new RewardCountAndPriceDetails
            {
                MinCount = scavCaseDetails.EndProducts.Superrare.Min,
                MaxCount = scavCaseDetails.EndProducts.Superrare.Max,
                MinPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.SuperRare].Min,
                MaxPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.SuperRare].Max,
            },
        };
    }

    /// <summary>
    ///     Randomises the size of ammo and money stacks
    /// </summary>
    /// <param name="itemToCalculate">ammo or money item</param>
    /// <param name="rarity">rarity (common/rare/superrare)</param>
    /// <returns>value to set stack count to</returns>
    protected int GetRandomAmountRewardForScavCase(TemplateItem itemToCalculate, string rarity)
    {
        var parentId = itemToCalculate.Parent;

        if (parentId == BaseClasses.AMMO)
        {
            return GetRandomisedAmmoRewardStackSize(itemToCalculate);
        }
        else if (parentId == BaseClasses.MONEY)
        {
            return GetRandomisedMoneyRewardStackSize(itemToCalculate, rarity);
        }
        else
        {
            return 1;
        }
    }

    /// <summary>
    ///     Randomises the size of ammo stacks
    /// </summary>
    /// <param name="itemToCalculate">ammo or money item</param>
    /// <returns>value to set stack count to</returns>
    protected int GetRandomisedAmmoRewardStackSize(TemplateItem itemToCalculate)
    {
        return randomUtil.GetInt(_scavCaseConfig.AmmoRewards.MinStackSize, itemToCalculate.Properties.StackMaxSize ?? 0);
    }

    /// <summary>
    ///     Randomises the size of money stacks
    /// </summary>
    /// <param name="itemToCalculate">ammo or money item</param>
    /// <param name="rarity">rarity (common/rare/superrare)</param>
    /// <returns>value to set stack count to</returns>
    protected int GetRandomisedMoneyRewardStackSize(TemplateItem itemToCalculate, string rarity)
    {
        var id = itemToCalculate.Id;

        if (id == Money.ROUBLES)
        {
            return randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.RubCount.GetByJsonProperty<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.RubCount.GetByJsonProperty<MinMax<int>>(rarity).Max
            );
        }
        else if (id == Money.EUROS)
        {
            return randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.EurCount.GetByJsonProperty<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.EurCount.GetByJsonProperty<MinMax<int>>(rarity).Max
            );
        }
        else if (id == Money.DOLLARS)
        {
            return randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.UsdCount.GetByJsonProperty<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.UsdCount.GetByJsonProperty<MinMax<int>>(rarity).Max
            );
        }
        else if (id == Money.GP)
        {
            return randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.GpCount.GetByJsonProperty<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.GpCount.GetByJsonProperty<MinMax<int>>(rarity).Max
            );
        }
        else
        {
            return 1;
        }
    }
}

public record RewardRarity
{
    public const string Common = "common";
    public const string Rare = "rare";
    public const string SuperRare = "superrare";
}
