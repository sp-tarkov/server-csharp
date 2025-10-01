using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Collections;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;

[Injectable]
public class RepeatableQuestRewardGenerator(
    ISptLogger<RepeatableQuestRewardGenerator> logger,
    RandomUtil randomUtil,
    MathUtil mathUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    HandbookHelper handbookHelper,
    ServerLocalisationService localisationService,
    ItemFilterService itemFilterService,
    SeasonalEventService seasonalEventService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected QuestConfig QuestConfig = configServer.GetConfig<QuestConfig>();

    /// <summary>
    ///     Generate the reward for a mission. A reward can consist of: <br />
    ///     - Experience <br />
    ///     - Money <br />
    ///     - GP coins <br />
    ///     - Weapon preset <br />
    ///     - Items <br />
    ///     - Trader Reputation <br />
    ///     - Skill level experience <br />
    ///     <br />
    ///     The reward is dependent on the player level as given by the wiki. The exact mapping of pmcLevel to <br />
    ///     experience / money / items / trader reputation can be defined in QuestConfig.js <br />
    ///     <br />
    ///     There's also a random variation of the reward the spread of which can be also defined in the config <br />
    ///     <br />
    ///     Additionally, a scaling factor w.r.t. quest difficulty going from 0.2...1 can be used
    /// </summary>
    /// <param name="pmcLevel"> Level of player reward is being generated for </param>
    /// <param name="difficulty"> Reward scaling factor from 0.2 to 1 </param>
    /// <param name="traderId"> Trader reward will be given by </param>
    /// <param name="repeatableConfig"> Config for quest type (daily, weekly) </param>
    /// <param name="eliminationConfig"> Base Quest config</param>
    /// <param name="rewardTplBlacklist"> Optional: list of tpls to NOT use when picking a reward </param>
    /// <returns> QuestRewards </returns>
    public Dictionary<string, List<Reward>>? GenerateReward(
        int pmcLevel,
        double difficulty,
        MongoId traderId,
        RepeatableQuestConfig repeatableConfig,
        BaseQuestConfig eliminationConfig,
        HashSet<MongoId>? rewardTplBlacklist = null
    )
    {
        // Get vars to configure rewards with
        var rewardParams = GetQuestRewardValues(repeatableConfig.RewardScaling, difficulty, pmcLevel);

        // Get budget to spend on item rewards (copy of raw roubles given)
        var itemRewardBudget = rewardParams.RewardRoubles;

        // Possible improvement -> draw trader-specific items e.g. with _itemHelper.isOfBaseclass(val._id, ItemHelper.BASECLASS.FoodDrink)
        var rewards = new Dictionary<string, List<Reward>>
        {
            { "Success", [] },
            { "Started", [] },
            { "Fail", [] },
        };

        // Start reward index to keep track
        var rewardIndex = -1;

        // Add xp reward
        if (rewardParams.RewardXP > 0)
        {
            rewards["Success"]
                .Add(
                    new Reward
                    {
                        Id = new MongoId(),
                        Unknown = false,
                        GameMode = [],
                        AvailableInGameEditions = [],
                        Index = rewardIndex,
                        Value = rewardParams.RewardXP,
                        Type = RewardType.Experience,
                    }
                );
            rewardIndex++;
        }

        // Add money reward
        rewards["Success"].Add(GetMoneyReward(traderId, rewardParams.RewardRoubles, rewardIndex));
        rewardIndex++;

        // Add GP coin reward
        rewards["Success"].Add(GenerateItemReward(Money.GP, rewardParams.GpCoinRewardCount, rewardIndex));
        rewardIndex++;

        // Add preset weapon to reward if checks pass
        var traderWhitelistDetails = repeatableConfig.TraderWhitelist.FirstOrDefault(traderWhitelist =>
            traderWhitelist.TraderId == traderId
        );

        if (traderWhitelistDetails is null)
        {
            logger.Error($"Unable to find trader id: {traderId.ToString()} in whitelist");
            return null;
        }

        if (traderWhitelistDetails.RewardCanBeWeapon && randomUtil.GetChance100(traderWhitelistDetails.WeaponRewardChancePercent))
        {
            var chosenWeapon = GetRandomWeaponPresetWithinBudget(itemRewardBudget, rewardIndex);
            if (chosenWeapon is not null)
            {
                rewards["Success"].Add(chosenWeapon.Value.Key);

                // Subtract price of preset from item budget so we don't give player too much stuff
                itemRewardBudget -= chosenWeapon.Value.Value;
                rewardIndex++;
            }
        }

        var inBudgetRewardItemPool = ChooseRewardItemsWithinBudget(repeatableConfig, itemRewardBudget, traderId);
        if (rewardTplBlacklist is not null)
        {
            // Filter reward pool of items from blacklist, only use if there's at least 1 item remaining
            var filteredRewardItemPool = inBudgetRewardItemPool.Where(item => !rewardTplBlacklist.Contains(item.Id)).ToList();

            if (filteredRewardItemPool.Count != 0)
            {
                inBudgetRewardItemPool = filteredRewardItemPool.ToList();
            }
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            logger.Debug(
                $"Generating: {repeatableConfig.Name} quest for: {traderId.ToString()} with budget: {itemRewardBudget} totalling: {rewardParams.RewardNumItems} items"
            );
        }

        if (inBudgetRewardItemPool.Count > 0)
        {
            var itemsToReward = GetRewardableItemsFromPoolWithinBudget(
                inBudgetRewardItemPool,
                rewardParams.RewardNumItems,
                itemRewardBudget,
                repeatableConfig
            );

            // Add item rewards
            foreach (var itemReward in itemsToReward)
            {
                rewards["Success"].Add(GenerateItemReward(itemReward.Key.Id, itemReward.Value, rewardIndex));
                rewardIndex++;
            }
        }

        // Add rep reward to rewards array
        if (rewardParams.RewardReputation > 0)
        {
            Reward reward = new()
            {
                Id = new MongoId(),
                Unknown = false,
                GameMode = [],
                AvailableInGameEditions = [],
                Target = traderId,
                Value = rewardParams.RewardReputation,
                Type = RewardType.TraderStanding,
                Index = rewardIndex,
            };
            rewards["Success"].Add(reward);
            rewardIndex++;

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Adding: {rewardParams.RewardReputation} {traderId.ToString()} trader reputation reward");
            }
        }

        // Chance of adding skill reward
        if (randomUtil.GetChance100(rewardParams.SkillRewardChance))
        {
            var targetSkill = randomUtil.GetArrayValue(eliminationConfig.PossibleSkillRewards);
            Reward reward = new()
            {
                Id = new MongoId(),
                Unknown = false,
                GameMode = [],
                AvailableInGameEditions = [],
                Target = targetSkill,
                Value = rewardParams.SkillPointReward,
                Type = RewardType.Skill,
                Index = rewardIndex,
            };
            rewards["Success"].Add(reward);

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Adding {rewardParams.SkillPointReward} skill points to {targetSkill}");
            }
        }

        return rewards;
    }

    protected QuestRewardValues GetQuestRewardValues(RewardScaling rewardScaling, double effectiveDifficulty, int pmcLevel)
    {
        // difficulty could go from 0.2 ... -> for lowest difficulty receive 0.2*nominal reward
        var levelsConfig = rewardScaling.Levels;
        var roublesConfig = rewardScaling.Roubles;
        var gpCoinConfig = rewardScaling.GpCoins;
        var xpConfig = rewardScaling.Experience;
        var itemsConfig = rewardScaling.Items;
        var rewardSpreadConfig = rewardScaling.RewardSpread;
        var skillRewardChanceConfig = rewardScaling.SkillRewardChance;
        var skillPointRewardConfig = rewardScaling.SkillPointReward;
        var reputationConfig = rewardScaling.Reputation;

        return new QuestRewardValues
        {
            SkillPointReward = mathUtil.Interp1(pmcLevel, levelsConfig, skillPointRewardConfig),
            SkillRewardChance = mathUtil.Interp1(pmcLevel, levelsConfig, skillRewardChanceConfig),
            RewardReputation = GetRewardRep(effectiveDifficulty, pmcLevel, levelsConfig, reputationConfig, rewardSpreadConfig),
            RewardNumItems = GetRewardNumItems(pmcLevel, levelsConfig, itemsConfig),
            RewardRoubles = GetRewardRoubles(effectiveDifficulty, pmcLevel, levelsConfig, roublesConfig, rewardSpreadConfig),
            GpCoinRewardCount = GetGpCoinRewardCount(effectiveDifficulty, pmcLevel, levelsConfig, gpCoinConfig, rewardSpreadConfig),
            RewardXP = GetRewardXp(effectiveDifficulty, pmcLevel, levelsConfig, xpConfig, rewardSpreadConfig),
        };
    }

    protected double GetRewardXp(
        double effectiveDifficulty,
        int pmcLevel,
        List<double> levelsConfig,
        List<double> xpConfig,
        double rewardSpreadConfig
    )
    {
        var interpolatedXp = mathUtil.Interp1(pmcLevel, levelsConfig, xpConfig);
        var randomSpread = randomUtil.GetDouble(1 - rewardSpreadConfig, 1 + rewardSpreadConfig);

        return Math.Floor(effectiveDifficulty * interpolatedXp * randomSpread);
    }

    protected double GetGpCoinRewardCount(
        double effectiveDifficulty,
        int pmcLevel,
        List<double> levelsConfig,
        List<double> gpCoinConfig,
        double rewardSpreadConfig
    )
    {
        var interpolatedGpCoins = mathUtil.Interp1(pmcLevel, levelsConfig, gpCoinConfig);
        var randomSpread = randomUtil.GetDouble(1 - rewardSpreadConfig, 1 + rewardSpreadConfig);

        return Math.Ceiling(effectiveDifficulty * interpolatedGpCoins * randomSpread);
    }

    protected double GetRewardRep(
        double effectiveDifficulty,
        int pmcLevel,
        List<double> levelsConfig,
        List<double> reputationConfig,
        double rewardSpreadConfig
    )
    {
        var difficultyMod = 100 * effectiveDifficulty;
        var interpolatedRep = mathUtil.Interp1(pmcLevel, levelsConfig, reputationConfig);
        var randomSpread = randomUtil.GetDouble(1 - rewardSpreadConfig, 1 + rewardSpreadConfig);
        var multiplier = difficultyMod * interpolatedRep * randomSpread;

        return Math.Round(multiplier) / 100;
    }

    protected int GetRewardNumItems(int pmcLevel, List<double> levelsConfig, List<double> itemsConfig)
    {
        var interpolatedNumItems = mathUtil.Interp1(pmcLevel, levelsConfig, itemsConfig);

        return randomUtil.RandInt(1, (int)Math.Round(interpolatedNumItems) + 1);
    }

    protected double GetRewardRoubles(
        double effectiveDifficulty,
        int pmcLevel,
        List<double> levelsConfig,
        List<double> roublesConfig,
        double rewardSpreadConfig
    )
    {
        var interpolatedRoubles = mathUtil.Interp1(pmcLevel, levelsConfig, roublesConfig);
        var randomSpread = randomUtil.GetDouble(1d - rewardSpreadConfig, 1d + rewardSpreadConfig);

        return Math.Floor(effectiveDifficulty * interpolatedRoubles * randomSpread);
    }

    /// <summary>
    ///     Get an array of items + stack size to give to player as reward that fit inside a rouble budget.
    /// </summary>
    /// <param name="itemPool"> All possible items to choose rewards from </param>
    /// <param name="maxItemCount"> Total number of items to reward </param>
    /// <param name="itemRewardBudget"> Rouble budget all item rewards must fit in </param>
    /// <param name="repeatableConfig"> Config for quest type </param>
    /// <returns> Dictionary of items and stack size</returns>
    protected Dictionary<TemplateItem, int> GetRewardableItemsFromPoolWithinBudget(
        List<TemplateItem> itemPool,
        int maxItemCount,
        double itemRewardBudget,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var itemsToReturn = new Dictionary<TemplateItem, int>();
        var exhaustibleItemPool = new ExhaustableArray<TemplateItem>(itemPool, randomUtil, cloner);

        for (var i = 0; i < maxItemCount; i++)
        {
            // Default stack size to 1
            var rewardItemStackCount = 1;

            // Get a random item
            var chosenItemFromPool = exhaustibleItemPool.GetRandomValue();
            if (chosenItemFromPool is null || !exhaustibleItemPool.HasValues())
            {
                break;
            }

            // Handle edge case - ammo
            if (itemHelper.IsOfBaseclass(chosenItemFromPool.Id, BaseClasses.AMMO))
            {
                // Don't reward ammo that stacks to less than what's allowed in config
                if (chosenItemFromPool.Properties?.StackMaxSize < repeatableConfig.RewardAmmoStackMinSize)
                {
                    i--;
                    continue;
                }

                // Choose the smallest value between budget, fitting size and stack max
                rewardItemStackCount = CalculateAmmoStackSizeThatFitsBudget(chosenItemFromPool, itemRewardBudget, maxItemCount);
            }

            // 25% chance to double, triple or quadruple reward stack
            // (Only occurs when item is stackable and not weapon, armor or ammo)
            if (CanIncreaseRewardItemStackSize(chosenItemFromPool, 70000, 25))
            {
                rewardItemStackCount = GetRandomisedRewardItemStackSizeByPrice(chosenItemFromPool);
            }

            itemsToReturn.Add(chosenItemFromPool, rewardItemStackCount);

            var itemCost = presetHelper.GetDefaultPresetOrItemPrice(chosenItemFromPool.Id);
            var calculatedItemRewardBudget = itemRewardBudget - rewardItemStackCount * itemCost;
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Added item: {chosenItemFromPool.Id} with price: {rewardItemStackCount * itemCost}");
            }

            // If we still have budget narrow down possible items
            if (calculatedItemRewardBudget > 0)
            {
                // Filter possible reward items to only items with a price below the remaining budget
                exhaustibleItemPool = new ExhaustableArray<TemplateItem>(
                    FilterRewardPoolWithinBudget(itemPool, calculatedItemRewardBudget, 0),
                    randomUtil,
                    cloner
                );

                if (!exhaustibleItemPool.HasValues())
                {
                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug($"Reward pool empty with: {calculatedItemRewardBudget} roubles of budget remaining");
                    }
                }
            }

            // No budget for more items, end loop
            break;
        }

        return itemsToReturn;
    }

    /// <summary>
    ///     Get a count of cartridges that fits the rouble budget amount provided.<br />
    ///     e.g. how many M80s for 50,000 roubles.
    /// </summary>
    /// <param name="itemSelected"> Cartridge template </param>
    /// <param name="roublesBudget"> Rouble budget </param>
    /// <param name="rewardNumItems"> Count of rewarded items </param>
    /// <returns> Count that fits budget (min 1) </returns>
    protected int CalculateAmmoStackSizeThatFitsBudget(TemplateItem itemSelected, double roublesBudget, int rewardNumItems)
    {
        // Calculate budget per reward item
        var stackRoubleBudget = roublesBudget / rewardNumItems;

        var singleCartridgePrice = handbookHelper.GetTemplatePrice(itemSelected.Id);

        // Get a stack size of ammo that fits rouble budget
        var stackSizeThatFitsBudget = Math.Round(stackRoubleBudget / singleCartridgePrice);

        // Get itemDbs max stack size for ammo - don't go above 100 (some mods mess around with stack sizes)
        var stackMaxCount = Math.Min(itemSelected.Properties.StackMaxSize.Value, 100);

        // Ensure stack size is at least 1 + is no larger than the max possible stack size
        return (int)Math.Clamp(stackSizeThatFitsBudget, 1, stackMaxCount);
    }

    protected bool CanIncreaseRewardItemStackSize(TemplateItem item, int maxRoublePriceToStack, int randomChanceToPass = 100)
    {
        var isEligibleForStackSizeIncrease =
            presetHelper.GetDefaultPresetOrItemPrice(item.Id) < maxRoublePriceToStack
            && !itemHelper.IsOfBaseclasses(item.Id, [BaseClasses.WEAPON, BaseClasses.ARMORED_EQUIPMENT, BaseClasses.AMMO])
            && !itemHelper.ItemRequiresSoftInserts(item.Id);

        return isEligibleForStackSizeIncrease && randomUtil.GetChance100(randomChanceToPass);
    }

    /// <summary>
    ///     Get a randomised number a reward items stack size should be based on its handbook price
    /// </summary>
    /// <param name="item"> Reward item to get stack size for </param>
    /// <returns> Matching stack size for the passed in items price </returns>
    protected int GetRandomisedRewardItemStackSizeByPrice(TemplateItem item)
    {
        var rewardItemPrice = presetHelper.GetDefaultPresetOrItemPrice(item.Id);

        // Define price tiers and corresponding stack size options
        var priceTiers = new List<Tuple<int, List<int>>>
        {
            new(3000, [2, 3, 4]),
            new(10000, [2, 3]),
            new(int.MaxValue, [2, 3, 4]), // Default for prices 10001+ RUB
        };

        // Find the appropriate price tier and return a random stack size from its options
        var tier = priceTiers.FirstOrDefault(tier => rewardItemPrice < tier.Item1);
        if (tier is null)
        {
            return 4; // Default to 4 if no tier matches
        }

        return randomUtil.GetArrayValue(tier.Item2);
    }

    /// <summary>
    ///     Select a number of items that have a collective value of the passed in parameter
    /// </summary>
    /// <param name="repeatableConfig"> Config </param>
    /// <param name="roublesBudget"> Total value of items to return </param>
    /// <param name="traderId"> ID of the trader who will give player reward </param>
    /// <returns> List of reward items that fit budget </returns>
    protected List<TemplateItem> ChooseRewardItemsWithinBudget(
        RepeatableQuestConfig repeatableConfig,
        double roublesBudget,
        MongoId traderId
    )
    {
        // First filter for type and baseclass to avoid lookup in handbook for non-available items
        var rewardableItemPool = GetRewardableItems(repeatableConfig, traderId);
        var minPrice = Math.Min(25000, 0.5 * roublesBudget);

        var rewardableItemPoolWithinBudget = FilterRewardPoolWithinBudget(rewardableItemPool, roublesBudget, minPrice);

        if (rewardableItemPoolWithinBudget.Count == 0)
        {
            logger.Warning(localisationService.GetText("repeatable-no_reward_item_found_in_price_range", new { minPrice, roublesBudget }));

            // In case we don't find any items in the price range
            rewardableItemPoolWithinBudget = rewardableItemPool.Where(x => itemHelper.GetItemPrice(x.Id) < roublesBudget).ToList();
        }

        return rewardableItemPoolWithinBudget;
    }

    /// <summary>
    ///     Filters a list of reward Items within a budget.
    /// </summary>
    /// <param name="rewardItems"> List of reward items to filter </param>
    /// <param name="roublesBudget"> The budget remaining for rewards </param>
    /// <param name="minPrice"> The minimum priced item to include </param>
    /// <returns> List of Items </returns>
    protected List<TemplateItem> FilterRewardPoolWithinBudget(List<TemplateItem> rewardItems, double roublesBudget, double minPrice)
    {
        return rewardItems
            .Where(item =>
            {
                var itemPrice = presetHelper.GetDefaultPresetOrItemPrice(item.Id);
                return itemPrice < roublesBudget && itemPrice > minPrice;
            })
            .ToList();
    }

    /// <summary>
    ///     Choose a random Weapon preset that fits inside a rouble amount limit
    /// </summary>
    /// <param name="roublesBudget"> Budget in roubles </param>
    /// <param name="rewardIndex"> Index of the reward </param>
    /// <returns> Dictionary of the reward and it's price, can return null. </returns>
    protected KeyValuePair<Reward, double>? GetRandomWeaponPresetWithinBudget(double roublesBudget, int rewardIndex)
    {
        // Add a random default preset weapon as reward
        var defaultPresetPool = new ExhaustableArray<Preset>(presetHelper.GetDefaultWeaponPresets().Values.ToList(), randomUtil, cloner);

        while (defaultPresetPool.HasValues())
        {
            var randomPreset = defaultPresetPool.GetRandomValue();
            if (randomPreset is null)
            {
                continue;
            }

            // Gather all tpls so we can get prices of them
            var tpls = randomPreset.Items.Select(item => item.Template).ToList();

            // Does preset items fit our budget
            var presetPrice = itemHelper.GetItemAndChildrenPrice(tpls);
            if (presetPrice <= roublesBudget)
            {
                logger.Debug($"Added weapon: {tpls[0]}with price: {presetPrice}");
                var chosenPreset = cloner.Clone(randomPreset)!;

                return new KeyValuePair<Reward, double>(
                    GeneratePresetReward(chosenPreset.Encyclopedia.Value, 1, rewardIndex, chosenPreset.Items),
                    presetPrice
                );
            }
        }

        return null;
    }

    /// <summary>
    ///     Helper to create a reward item structured as required by the client
    /// </summary>
    /// <param name="tpl"> ItemId of the rewarded item </param>
    /// <param name="count"> Amount of items to give </param>
    /// <param name="index"> All rewards will be appended to a list, for unknown reasons the client wants the index </param>
    /// <param name="preset"> Optional list of preset items </param>
    /// <param name="foundInRaid"> If generated Item is found in raid, default True </param>
    /// <returns> Object of "Reward"-item-type </returns>
    protected Reward GeneratePresetReward(MongoId tpl, int count, int index, List<Item>? preset, bool foundInRaid = true)
    {
        var id = new MongoId();
        var questRewardItem = new Reward
        {
            Id = new MongoId(),
            Unknown = false,
            GameMode = [],
            AvailableInGameEditions = [],
            Index = index,
            Target = id,
            Value = count,
            IsEncoded = false,
            FindInRaid = foundInRaid,
            Type = RewardType.Item,
            Items = [],
        };

        // Get presets root item
        var rootItem = preset?.FirstOrDefault(item => item.Template == tpl);
        if (rootItem is null)
        {
            logger.Warning($"Root item of preset: {tpl} not found");
        }

        if (rootItem?.Upd is not null)
        {
            rootItem.Upd.SpawnedInSession = foundInRaid;
        }

        questRewardItem.Items = itemHelper.ReparentItemAndChildren(rootItem, preset);
        questRewardItem.Target = rootItem.Id; // Target property and root items id must match

        return questRewardItem;
    }

    /// <summary>
    ///     Helper to create a reward item structured as required by the client
    /// </summary>
    /// <param name="tpl"> ItemId of the rewarded item </param>
    /// <param name="count"> Amount of items to give</param>
    /// <param name="index"> All rewards will be appended to a list, for unknown reasons the client wants the index</param>
    /// <param name="foundInRaid"> If generated Item is found in raid, default True </param>
    /// <returns> Object of "Reward"-item-type </returns>
    protected Reward GenerateItemReward(MongoId tpl, double count, int index, bool foundInRaid = true)
    {
        var id = new MongoId();
        var questRewardItem = new Reward
        {
            Id = new MongoId(),
            Unknown = false,
            GameMode = [],
            AvailableInGameEditions = [],
            Index = index,
            Target = id,
            Value = count,
            IsEncoded = false,
            FindInRaid = foundInRaid,
            Type = RewardType.Item,
            Items = [],
        };

        var rootItem = new Item
        {
            Id = id,
            Template = tpl,
            Upd = new Upd { StackObjectsCount = count, SpawnedInSession = foundInRaid },
        };
        questRewardItem.Items = [rootItem];

        return questRewardItem;
    }

    protected Reward GetMoneyReward(MongoId traderId, double rewardRoubles, int rewardIndex)
    {
        // Determine currency based on trader
        // PK and Fence use Euros, everyone else is Roubles
        var currency = traderId == Traders.PEACEKEEPER || traderId == Traders.FENCE ? Money.EUROS : Money.ROUBLES;

        // Convert reward amount to Euros if necessary
        var rewardAmountToGivePlayer = currency == Money.EUROS ? handbookHelper.FromRoubles(rewardRoubles, Money.EUROS) : rewardRoubles;

        // Get chosen currency + amount and return
        return GenerateItemReward(currency, rewardAmountToGivePlayer, rewardIndex, false);
    }

    /// <summary>
    ///     Picks rewardable items from items.json <br />
    ///     This means they must: <br />
    ///     - Fit into the inventory <br />
    ///     - Shouldn't be keys <br />
    ///     - Have a price greater than 0
    /// </summary>
    /// <param name="repeatableQuestConfig"> Config </param>
    /// <param name="traderId"> ID of trader who will give reward to player </param>
    /// <returns> List of rewardable items [[_tpl, itemTemplate],...] </returns>
    public List<TemplateItem> GetRewardableItems(RepeatableQuestConfig repeatableQuestConfig, MongoId traderId)
    {
        // Get an array of seasonal items that should not be shown right now as seasonal event is not active
        var seasonalItems = seasonalEventService.GetInactiveSeasonalEventItems();

        // Check for specific base classes which don't make sense as reward item
        // also check if the price is greater than 0; there are some items whose price can not be found
        // those are not in the game yet (e.g. AGS grenade launcher)
        return databaseService
            .GetItems()
            .Values.Where(itemTemplate =>
            {
                // Base "Item" item has no parent, ignore it
                if (itemTemplate.Parent == MongoId.Empty())
                {
                    return false;
                }

                if (seasonalItems.Contains(itemTemplate.Id))
                {
                    return false;
                }

                var traderWhitelist = repeatableQuestConfig.TraderWhitelist.FirstOrDefault(trader => trader.TraderId == traderId);

                return IsValidRewardItem(
                    itemTemplate.Id,
                    repeatableQuestConfig.RewardBlacklist,
                    repeatableQuestConfig.RewardBaseTypeBlacklist,
                    traderWhitelist?.RewardBaseWhitelist
                );
            })
            .ToList();
    }

    /// <summary>
    ///     Checks if an id is a valid item. Valid meaning that it's an item that may be a reward
    ///     or content of bot loot. Items that are tested as valid may be in a player backpack or stash.
    /// </summary>
    /// <param name="tpl"> Template id of item to check</param>
    /// <param name="itemTplBlacklist"> Specific item tpls to ignore </param>
    /// <param name="itemTypeBlacklist"> Specific item base types to ignore </param>
    /// <param name="itemBaseWhitelist"> Default null, specific trader item base classes</param>
    /// <returns> True if item is valid reward </returns>
    public bool IsValidRewardItem(
        MongoId tpl,
        HashSet<MongoId> itemTplBlacklist,
        HashSet<MongoId> itemTypeBlacklist,
        IEnumerable<MongoId>? itemBaseWhitelist = null
    )
    {
        // Return early if not valid item to give as reward
        if (!itemHelper.IsValidItem(tpl))
        {
            return false;
        }

        // Check item is not blacklisted
        if (
            itemFilterService.IsItemBlacklisted(tpl)
            || itemFilterService.IsItemRewardBlacklisted(tpl)
            || itemTplBlacklist.Contains(tpl)
            || itemFilterService.IsItemBlacklisted(tpl)
        )
        {
            return false;
        }

        // Item has blacklisted base types
        if (itemHelper.IsOfBaseclasses(tpl, itemTypeBlacklist))
        {
            return false;
        }

        // Skip boss items
        if (itemFilterService.IsBossItem(tpl))
        {
            return false;
        }

        // Trader has specific item base types they can give as rewards to player
        if (itemBaseWhitelist is not null && !itemHelper.IsOfBaseclasses(tpl, itemBaseWhitelist))
        {
            return false;
        }

        return true;
    }
}
