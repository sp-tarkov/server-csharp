using Core.Helpers;
using Core.Models.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Hideout;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using SptCommon.Extensions;

namespace Core.Generators;

[Injectable]
public class ScavCaseRewardGenerator(
    ISptLogger<ScavCaseRewardGenerator> _logger,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    PresetHelper _presetHelper,
    DatabaseService _databaseService,
    RagfairPriceService _ragfairPriceService,
    SeasonalEventService _seasonalEventService,
    ItemFilterService _itemFilterService,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected List<TemplateItem> _dbAmmoItemsCache = [];
    protected List<TemplateItem> _dbItemsCache = [];
    protected ScavCaseConfig _scavCaseConfig = _configServer.GetConfig<ScavCaseConfig>();

    /// <summary>
    ///     Create an array of rewards that will be given to the player upon completing their scav case build
    /// </summary>
    /// <param name="recipeId">recipe of the scav case craft</param>
    /// <returns>Product array</returns>
    public List<List<Item>> Generate(string recipeId)
    {
        CacheDbItems();

        // Get scavcase details from hideout/scavcase.json
        var scavCaseDetails = _databaseService
            .GetHideout()
            .Production.ScavRecipes.FirstOrDefault(r => r.Id == recipeId);
        var rewardItemCounts = GetScavCaseRewardCountsAndPrices(scavCaseDetails);

        // Get items that fit the price criteria as set by the scavCase config
        var commonPricedItems = GetFilteredItemsByPrice(_dbItemsCache, rewardItemCounts.Common);
        var rarePricedItems = GetFilteredItemsByPrice(_dbItemsCache, rewardItemCounts.Rare);
        var superRarePricedItems = GetFilteredItemsByPrice(_dbItemsCache, rewardItemCounts.Superrare);

        // Get randomly picked items from each item collection, the count range of which is defined in hideout/scavcase.json
        var randomlyPickedCommonRewards = PickRandomRewards(
            commonPricedItems,
            rewardItemCounts.Common,
            RewardRarity.Common
        );

        var randomlyPickedRareRewards = PickRandomRewards(
            rarePricedItems,
            rewardItemCounts.Rare,
            RewardRarity.Rare);

        var randomlyPickedSuperRareRewards = PickRandomRewards(
            superRarePricedItems,
            rewardItemCounts.Superrare,
            RewardRarity.SuperRare
        );

        // Add randomised stack sizes to ammo and money rewards
        var commonRewards = RandomiseContainerItemRewards(randomlyPickedCommonRewards, RewardRarity.Common);
        var rareRewards = RandomiseContainerItemRewards(randomlyPickedRareRewards, RewardRarity.Rare);
        var superRareRewards = RandomiseContainerItemRewards(randomlyPickedSuperRareRewards, RewardRarity.SuperRare);

        var result = new List<List<Item>>();
        result = result.Concat(commonRewards).ToList();
        result = result.Concat(rareRewards).ToList();
        result = result.Concat(superRareRewards).ToList();
        // TODO: please make this better, how merge 2d Lists

        return result.ToList();
    }

    /// <summary>
    ///     Get all db items that are not blacklisted in scavcase config or global blacklist
    ///     Store in class field
    /// </summary>
    protected void CacheDbItems()
    {
        // Get an array of seasonal items that should not be shown right now as seasonal event is not active
        var inactiveSeasonalItems = _seasonalEventService.GetInactiveSeasonalEventItems();
        if (!_dbItemsCache.Any())
        {
            _dbItemsCache = _databaseService.GetItems()
                .Values.Where(
                    item =>
                    {
                        // Base "Item" item has no parent, ignore it
                        if (item.Parent == "")
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
                            item.Type != "Item" ||
                            _scavCaseConfig.RewardItemBlacklist.Contains(item.Id) ||
                            _itemFilterService.IsItemBlacklisted(item.Id)
                        )
                        {
                            return false;
                        }

                        // Globally reward-blacklisted
                        if (_itemFilterService.IsItemRewardBlacklisted(item.Id))
                        {
                            return false;
                        }

                        if (!_scavCaseConfig.AllowBossItemsAsRewards && _itemFilterService.IsBossItem(item.Id))
                        {
                            return false;
                        }

                        // Skip item if parent id is blacklisted
                        if (_itemHelper.IsOfBaseclasses(item.Id, _scavCaseConfig.RewardItemParentBlacklist))
                        {
                            return false;
                        }

                        if (inactiveSeasonalItems.Contains(item.Id))
                        {
                            return false;
                        }

                        return true;
                    }
                )
                .ToList();
        }

        if (!_dbAmmoItemsCache.Any())
        {
            _dbAmmoItemsCache = _databaseService.GetItems()
                .Values.Where(
                    item =>
                    {
                        // Base "Item" item has no parent, ignore it
                        if (item.Parent == "")
                        {
                            return false;
                        }

                        if (item.Type != "Item")
                        {
                            return false;
                        }

                        // Not ammo, skip
                        if (!_itemHelper.IsOfBaseclass(item.Id, BaseClasses.AMMO))
                        {
                            return false;
                        }

                        // Skip item if item id is on blacklist
                        if (
                            _scavCaseConfig.RewardItemBlacklist.Contains(item.Id) ||
                            _itemFilterService.IsItemBlacklisted(item.Id)
                        )
                        {
                            return false;
                        }

                        // Globally reward-blacklisted
                        if (_itemFilterService.IsItemRewardBlacklisted(item.Id))
                        {
                            return false;
                        }

                        if (!_scavCaseConfig.AllowBossItemsAsRewards && _itemFilterService.IsBossItem(item.Id))
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
                    }
                )
                .ToList();
        }
    }

    /// <summary>
    ///     Pick a number of items to be rewards, the count is defined by the values in `itemFilters` param
    /// </summary>
    /// <param name="items">item pool to pick rewards from</param>
    /// <param name="itemFilters">how the rewards should be filtered down (by item count)</param>
    /// <returns></returns>
    protected List<TemplateItem> PickRandomRewards(
        List<TemplateItem> items,
        RewardCountAndPriceDetails itemFilters,
        string rarity)
    {
        List<TemplateItem> result = [];

        var rewardWasMoney = false;
        var rewardWasAmmo = false;
        var randomCount = _randomUtil.GetInt((int) itemFilters.MinCount, (int) itemFilters.MaxCount);
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
                result.Add(_randomUtil.GetArrayValue(items));
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
        return _randomUtil.GetChance100(_scavCaseConfig.MoneyRewards.MoneyRewardChancePercent);
    }

    /// <summary>
    ///     Choose if ammo should be a reward based on the ammoRewardChancePercent config chance in scavCaseConfig
    /// </summary>
    /// <returns>true if reward should be ammo</returns>
    protected bool RewardShouldBeAmmo()
    {
        return _randomUtil.GetChance100(_scavCaseConfig.AmmoRewards.AmmoRewardChancePercent);
    }

    /// <summary>
    ///     Choose from rouble/dollar/euro at random
    /// </summary>
    protected TemplateItem GetRandomMoney()
    {
        List<TemplateItem> money = [];
        var items = _databaseService.GetItems();
        money.Add(items[Money.ROUBLES]);
        money.Add(items[Money.EUROS]);
        money.Add(items[Money.DOLLARS]);
        money.Add(items[Money.GP]);

        return _randomUtil.GetArrayValue(money);
    }

    /// <summary>
    ///     Get a random ammo from items.json that is not in the ammo blacklist AND inside the price range defined in scavcase.json config
    /// </summary>
    /// <param name="rarity">The rarity this ammo reward is for</param>
    /// <returns>random ammo item from items.json</returns>
    protected TemplateItem GetRandomAmmo(string rarity)
    {
        var possibleAmmoPool = _dbAmmoItemsCache.Where(
            ammo =>
            {
                // Is ammo handbook price between desired range
                var handbookPrice = _ragfairPriceService.GetStaticPriceForItem(ammo.Id);
                if (
                    handbookPrice >= _scavCaseConfig.AmmoRewards.AmmoRewardValueRangeRub[rarity].Min &&
                    handbookPrice <= _scavCaseConfig.AmmoRewards.AmmoRewardValueRangeRub[rarity].Max
                )
                {
                    return true;
                }

                return false;
            }
        );

        if (!possibleAmmoPool.Any())
        {
            _logger.Warning("Unable to get a list of ammo that matches desired criteria for scav case reward");
        }

        // Get a random ammo and return it
        return _randomUtil.GetArrayValue(possibleAmmoPool);
    }

    /// <summary>
    ///     Take all the rewards picked create the Product object array ready to return to calling code.
    ///     Also add a stack count to ammo and money
    /// </summary>
    /// <param name="rewardItems">items to convert</param>
    /// <returns>Product array</returns>
    protected List<List<Item>> RandomiseContainerItemRewards(List<TemplateItem> rewardItems, string rarity)
    {
        // Each array is an item + children
        List<List<Item>> result = [];
        foreach (var rewardItemDb in rewardItems)
        {
            List<Item> resultItem =
            [
                new()
                {
                    Id = _hashUtil.Generate(),
                    Template = rewardItemDb.Id,
                    Upd = null
                }
            ];
            var rootItem = resultItem.FirstOrDefault();

            if (_itemHelper.IsOfBaseclass(rewardItemDb.Id, BaseClasses.AMMO_BOX))
            {
                _itemHelper.AddCartridgesToAmmoBox(resultItem, rewardItemDb);
            }
            // Armor or weapon = use default preset from globals.json
            else if (
                _itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(rewardItemDb.Id) ||
                _itemHelper.IsOfBaseclass(rewardItemDb.Id, BaseClasses.WEAPON)
            )
            {
                var preset = _presetHelper.GetDefaultPreset(rewardItemDb.Id);
                if (preset is null)
                {
                    _logger.Warning($"No preset for item: {rewardItemDb.Id} {rewardItemDb.Name}, skipping");

                    continue;
                }

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = _itemHelper.ReplaceIDs(_cloner.Clone(preset.Items));
                _itemHelper.RemapRootItemId(presetAndMods);

                resultItem = presetAndMods;
            }
            else if (_itemHelper.IsOfBaseclasses(rewardItemDb.Id, [BaseClasses.AMMO, BaseClasses.MONEY]))
            {
                rootItem.Upd = new Upd
                {
                    StackObjectsCount = GetRandomAmountRewardForScavCase(rewardItemDb, rarity)
                };
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
    protected List<TemplateItem> GetFilteredItemsByPrice(
        List<TemplateItem> dbItems,
        RewardCountAndPriceDetails itemFilters)
    {
        return dbItems.Where(
                item =>
                {
                    var handbookPrice = _ragfairPriceService.GetStaticPriceForItem(item.Id);
                    if (handbookPrice >= itemFilters.MinPriceRub && handbookPrice <= itemFilters.MaxPriceRub)
                    {
                        return true;
                    }

                    return false;
                }
            )
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
                MaxPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Common].Max
            },
            Rare = new RewardCountAndPriceDetails
            {
                MinCount = scavCaseDetails.EndProducts.Rare.Min,
                MaxCount = scavCaseDetails.EndProducts.Rare.Max,
                MinPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Rare].Min,
                MaxPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.Rare].Max
            },
            Superrare = new RewardCountAndPriceDetails
            {
                MinCount = scavCaseDetails.EndProducts.Superrare.Min,
                MaxCount = scavCaseDetails.EndProducts.Superrare.Max,
                MinPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.SuperRare].Min,
                MaxPriceRub = _scavCaseConfig.RewardItemValueRangeRub[RewardRarity.SuperRare].Max
            }
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
        return itemToCalculate.Parent switch
        {
            BaseClasses.AMMO => GetRandomisedAmmoRewardStackSize(itemToCalculate),
            BaseClasses.MONEY => GetRandomisedMoneyRewardStackSize(itemToCalculate, rarity),
            _ => 1
        };
    }
    /// <summary>
    ///     Randomises the size of ammo stacks
    /// </summary>
    /// <param name="itemToCalculate">ammo or money item</param>
    /// <returns>value to set stack count to</returns>
    protected int GetRandomisedAmmoRewardStackSize(TemplateItem itemToCalculate)
    {
        return _randomUtil.GetInt(
            _scavCaseConfig.AmmoRewards.MinStackSize,
            itemToCalculate.Properties.StackMaxSize ?? 0
        );
    }
    /// <summary>
    ///     Randomises the size of money stacks
    /// </summary>
    /// <param name="itemToCalculate">ammo or money item</param>
    /// <param name="rarity">rarity (common/rare/superrare)</param>
    /// <returns>value to set stack count to</returns>
    protected int GetRandomisedMoneyRewardStackSize(TemplateItem itemToCalculate, string rarity)
    {
        return itemToCalculate.Id switch
        {
            Money.ROUBLES => _randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.RubCount.GetByJsonProp<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.RubCount.GetByJsonProp<MinMax<int>>(rarity).Max
            ),
            Money.EUROS => _randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.EurCount.GetByJsonProp<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.EurCount.GetByJsonProp<MinMax<int>>(rarity).Max
            ),
            Money.DOLLARS => _randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.UsdCount.GetByJsonProp<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.UsdCount.GetByJsonProp<MinMax<int>>(rarity).Max
            ),
            Money.GP => _randomUtil.GetInt(
                _scavCaseConfig.MoneyRewards.GpCount.GetByJsonProp<MinMax<int>>(rarity).Min,
                _scavCaseConfig.MoneyRewards.GpCount.GetByJsonProp<MinMax<int>>(rarity).Max
            ),
            _ => 1
        };
    }
}

public record RewardRarity
{
    public const string Common = "common";
    public const string Rare = "rare";
    public const string SuperRare = "superrare";
}
