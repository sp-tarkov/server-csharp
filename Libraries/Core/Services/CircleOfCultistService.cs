using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Enums.Hideout;
using Core.Models.Spt.Config;
using Core.Models.Spt.Hideout;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Utils;
using Core.Utils.Cloners;
using SptCommon.Annotations;
using SptCommon.Extensions;
using Hideout = Core.Models.Spt.Hideout.Hideout;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class CircleOfCultistService(
    ISptLogger<CircleOfCultistService> _logger,
    TimeUtil _timeUtil,
    ICloner _cloner,
    EventOutputHolder _eventOutputHolder,
    RandomUtil _randomUtil,
    HashUtil _hashUtil,
    ItemHelper _itemHelper,
    PresetHelper _presetHelper,
    ProfileHelper _profileHelper,
    InventoryHelper _inventoryHelper,
    HideoutHelper _hideoutHelper,
    QuestHelper _questHelper,
    DatabaseService _databaseService,
    ItemFilterService _itemFilterService,
    SeasonalEventService _seasonalEventService,
    ConfigServer _configServer
)
{
    protected const string CircleOfCultistSlotId = "CircleOfCultistsGrid1";
    protected HideoutConfig _hideoutConfig = _configServer.GetConfig<HideoutConfig>();

    /// <summary>
    ///     Start a sacrifice event
    ///     Generate rewards
    ///     Delete sacrificed items
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile doing sacrifice</param>
    /// <param name="request">Client request</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse StartSacrifice(
        string sessionId,
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request
    )
    {
        var cultistCircleStashId = pmcData.Inventory.HideoutAreaStashes.GetValueOrDefault(HideoutAreas.CIRCLE_OF_CULTISTS.ToString());

        // `cultistRecipes` just has single recipeId
        var cultistCraftData = _databaseService.GetHideout().Production.CultistRecipes.FirstOrDefault();
        var sacrificedItems = GetSacrificedItems(pmcData);
        var sacrificedItemCostRoubles = sacrificedItems.Aggregate(
            0D,
            (sum, curr) => sum + (_itemHelper.GetItemPrice(curr.Template) ?? 0)
        );

        var rewardAmountMultiplier = GetRewardAmountMultiplier(pmcData, _hideoutConfig.CultistCircle);

        // Get the rouble amount we generate rewards with from cost of sacrificed items * above multiplier
        var rewardAmountRoubles = Math.Round(sacrificedItemCostRoubles * rewardAmountMultiplier);

        // Check if it matches any direct swap recipes
        var directRewardsCache = GenerateSacrificedItemsCache(_hideoutConfig.CultistCircle.DirectRewards);
        var directRewardSettings = CheckForDirectReward(sessionId, sacrificedItems, directRewardsCache);
        var hasDirectReward = directRewardSettings?.Reward.Count > 0;

        // Get craft time and bonus status
        var craftingInfo = GetCircleCraftingInfo(
            rewardAmountRoubles,
            _hideoutConfig.CultistCircle,
            directRewardSettings
        );

        // Create production in pmc profile
        RegisterCircleOfCultistProduction(
            sessionId,
            pmcData,
            cultistCraftData.Id,
            sacrificedItems,
            craftingInfo.Time
        );

        var output = _eventOutputHolder.GetOutput(sessionId);

        // Remove sacrificed items from circle inventory
        foreach (var item in sacrificedItems)
        {
            if (item.SlotId == CircleOfCultistSlotId)
            {
                _inventoryHelper.RemoveItem(pmcData, item.Id, sessionId, output);
            }
        }

        var rewards = hasDirectReward
            ? GetDirectRewards(sessionId, directRewardSettings, cultistCircleStashId)
            : GetRewardsWithinBudget(
                GetCultistCircleRewardPool(sessionId, pmcData, craftingInfo, _hideoutConfig.CultistCircle),
                rewardAmountRoubles,
                cultistCircleStashId,
                _hideoutConfig.CultistCircle
            );

        // Get the container grid for cultist stash area
        var cultistStashDbItem = _itemHelper.GetItem(ItemTpl.HIDEOUTAREACONTAINER_CIRCLEOFCULTISTS_STASH_1);

        // Ensure rewards fit into container
        var containerGrid = _inventoryHelper.GetContainerSlotMap(cultistStashDbItem.Value.Id);
        AddRewardsToCircleContainer(sessionId, pmcData, rewards, containerGrid, cultistCircleStashId, output);

        return output;
    }

    private double GetRewardAmountMultiplier(PmcData pmcData, CultistCircleSettings cultistCircleSettings)
    {
        // Get a randomised value to multiply the sacrificed rouble cost by
        var rewardAmountMultiplier = _randomUtil.GetDouble(
            (double) cultistCircleSettings.RewardPriceMultiplerMinMax.Min,
            (double) cultistCircleSettings.RewardPriceMultiplerMinMax.Max
        );

        // Adjust value generated by the players hideout management skill
        var hideoutManagementSkill = _profileHelper.GetSkillFromProfile(pmcData, SkillTypes.HideoutManagement);
        if (hideoutManagementSkill is not null)
        {
            rewardAmountMultiplier *=
                (float) (1 + hideoutManagementSkill.Progress / 10000); // 5100 becomes 0.51, add 1 to it, 1.51, multiply the bonus by it (e.g. 1.2 x 1.51)
        }

        return rewardAmountMultiplier;
    }

    /// <summary>
    ///     Register production inside player profile
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="recipeId">Recipe id</param>
    /// <param name="sacrificedItems">Items player sacrificed</param>
    /// <param name="craftingTime">How long the ritual should take</param>
    protected void RegisterCircleOfCultistProduction(
        string sessionId,
        PmcData pmcData,
        string recipeId,
        List<Item> sacrificedItems,
        double craftingTime
    )
    {
        // Create circle production/craft object to add to player profile
        var cultistProduction = _hideoutHelper.InitProduction(recipeId, craftingTime, false);

        // Flag as cultist circle for code to pick up later
        cultistProduction.SptIsCultistCircle = true;

        // Add items player sacrificed
        cultistProduction.GivenItemsInStart = sacrificedItems;

        // Add circle production to profile keyed to recipe id
        pmcData.Hideout.Production[recipeId] = cultistProduction;
    }

    /// <summary>
    ///     Get the circle craft time as seconds, value is based on reward item value
    ///     And get the bonus status to determine what tier of reward is given
    /// </summary>
    /// <param name="rewardAmountRoubles">Value of rewards in roubles</param>
    /// <param name="circleConfig">Circle config values</param>
    /// <param name="directRewardSettings">OPTIONAL - Values related to direct reward being given</param>
    /// <returns>craft time + type of reward + reward details</returns>
    protected CircleCraftDetails GetCircleCraftingInfo(
        double rewardAmountRoubles,
        CultistCircleSettings circleConfig,
        DirectRewardSettings? directRewardSettings = null
    )
    {
        var result = new CircleCraftDetails
        {
            Time = -1,
            RewardType = CircleRewardType.RANDOM,
            RewardAmountRoubles = (int) rewardAmountRoubles,
            RewardDetails = null
        };

        // Direct reward edge case
        if (directRewardSettings is not null)
        {
            result.Time = directRewardSettings.CraftTimeSeconds;

            return result;
        }

        var random = new Random();

        // Get a threshold where sacrificed amount is between thresholds min and max
        var matchingThreshold = GetMatchingThreshold(circleConfig.CraftTimeThreshholds, rewardAmountRoubles);
        if (
            rewardAmountRoubles >= circleConfig.HideoutCraftSacrificeThresholdRub &&
            random.Next(0, 1) <= circleConfig.BonusChanceMultiplier
        )
        {
            // Sacrifice amount is enough + passed 25% check to get hideout/task rewards
            result.Time =
                circleConfig.CraftTimeOverride != -1
                    ? circleConfig.CraftTimeOverride
                    : circleConfig.HideoutTaskRewardTimeSeconds;
            result.RewardType = CircleRewardType.HIDEOUT_TASK;

            return result;
        }

        // Edge case, check if override exists, Otherwise use matching threshold craft time
        result.Time =
            circleConfig.CraftTimeOverride != -1 ? circleConfig.CraftTimeOverride : matchingThreshold.CraftTimeSeconds;

        result.RewardDetails = matchingThreshold;

        return result;
    }

    protected CraftTimeThreshold GetMatchingThreshold(
        List<CraftTimeThreshold> thresholds,
        double rewardAmountRoubles
    )
    {
        var matchingThreshold = thresholds.FirstOrDefault(
            craftThreshold => craftThreshold.Min <= rewardAmountRoubles && craftThreshold.Max >= rewardAmountRoubles
        );

        // No matching threshold, make one
        if (matchingThreshold is null)
        {
            // None found, use a defalt
            _logger.Warning("Unable to find a matching cultist circle threshold, using fallback of 12 hours");

            // Use first threshold value (cheapest) from parameter array, otherwise use 12 hours
            var firstThreshold = thresholds.FirstOrDefault();
            var craftTime = firstThreshold?.CraftTimeSeconds is not null && firstThreshold.CraftTimeSeconds > 0
                ? firstThreshold.CraftTimeSeconds
                : _timeUtil.GetHoursAsSeconds(12);

            return new CraftTimeThreshold
            {
                Min = firstThreshold?.Min ?? 1,
                Max = firstThreshold?.Max ?? 34999,
                CraftTimeSeconds = craftTime
            };
        }

        return matchingThreshold;
    }

    /// <summary>
    ///     Get the items player sacrificed in circle
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Array of items from player inventory</returns>
    protected List<Item> GetSacrificedItems(PmcData pmcData)
    {
        // Get root items that are in the cultist sacrifice window
        var inventoryRootItemsInCultistGrid = pmcData.Inventory.Items.Where(
            item => item.SlotId == CircleOfCultistSlotId
        );

        // Get rootitem + its children
        List<Item> sacrificedItems = [];
        foreach (var rootItem in inventoryRootItemsInCultistGrid)
        {
            var rootItemWithChildren = _itemHelper.FindAndReturnChildrenAsItems(
                pmcData.Inventory.Items,
                rootItem.Id
            );
            sacrificedItems.AddRange(rootItemWithChildren);
        }

        return sacrificedItems;
    }

    /// <summary>
    ///     Given a pool of items + rouble budget, pick items until the budget is reached
    /// </summary>
    /// <param name="rewardItemTplPool">Items that can be picked</param>
    /// <param name="rewardBudget">Rouble budget to reach</param>
    /// <param name="cultistCircleStashId">Id of stash item</param>
    /// <returns>Array of item arrays</returns>
    protected List<List<Item>> GetRewardsWithinBudget(
        List<string> rewardItemTplPool,
        double rewardBudget,
        string cultistCircleStashId,
        CultistCircleSettings circleConfig
    )
    {
        // Prep rewards array (reward can be item with children, hence array of arrays)
        List<List<Item>> rewards = [];

        // Pick random rewards until we have exhausted the sacrificed items budget
        var totalRewardCost = 0;
        var rewardItemCount = 0;
        var failedAttempts = 0;
        while (
            totalRewardCost < rewardBudget &&
            rewardItemTplPool.Count > 0 &&
            rewardItemCount < circleConfig.MaxRewardItemCount
        )
        {
            if (failedAttempts > circleConfig.MaxAttemptsToPickRewardsWithinBudget)
            {
                _logger.Warning($"Exiting reward generation after {failedAttempts} failed attempts");

                break;
            }

            // Choose a random tpl from pool
            var randomItemTplFromPool = _randomUtil.GetArrayValue(rewardItemTplPool);

            // Is weapon/armor, handle differently
            if (
                _itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(randomItemTplFromPool) ||
                _itemHelper.IsOfBaseclass(randomItemTplFromPool, BaseClasses.WEAPON)
            )
            {
                var defaultPreset = _presetHelper.GetDefaultPreset(randomItemTplFromPool);
                if (defaultPreset is null)
                {
                    _logger.Warning($"Reward tpl: {randomItemTplFromPool} lacks a default preset, skipping reward");
                    failedAttempts++;

                    continue;
                }

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = _itemHelper.ReplaceIDs(defaultPreset.Items);
                _itemHelper.RemapRootItemId(presetAndMods);

                // Set item as FiR
                _itemHelper.SetFoundInRaid(presetAndMods);

                rewardItemCount++;
                totalRewardCost += (int) _itemHelper.GetItemPrice(randomItemTplFromPool);
                rewards.Add(presetAndMods);

                continue;
            }

            // Some items can have variable stack size, e.g. ammo / currency
            var stackSize = GetRewardStackSize(
                randomItemTplFromPool,
                (int) (rewardBudget / (rewardItemCount == 0 ? 1 : rewardItemCount)) // Remaining rouble budget
            );

            // Not a weapon/armor, standard single item
            List<Item> rewardItem =
            [
                new()
                {
                    Id = _hashUtil.Generate(),
                    Template = randomItemTplFromPool,
                    ParentId = cultistCircleStashId,
                    SlotId = CircleOfCultistSlotId,
                    Upd = new Upd
                    {
                        StackObjectsCount = stackSize
                    }
                }
            ];

            _itemHelper.SetFoundInRaid(rewardItem);

            // Edge case - item is ammo container and needs cartridges added
            if (_itemHelper.IsOfBaseclass(randomItemTplFromPool, BaseClasses.AMMO_BOX))
            {
                var itemDetails = _itemHelper.GetItem(randomItemTplFromPool).Value;
                _itemHelper.AddCartridgesToAmmoBox(rewardItem, itemDetails);
            }

            // Increment price of rewards to give to player + add to reward array
            rewardItemCount++;
            var singleItemPrice = _itemHelper.GetItemPrice(randomItemTplFromPool);
            var itemPrice = singleItemPrice * stackSize;
            totalRewardCost += (int) itemPrice;

            rewards.Add(rewardItem);
        }

        return rewards;
    }

    /// <summary>
    ///     Get direct rewards
    /// </summary>
    /// <param name="sessionId">sessionId</param>
    /// <param name="directReward">Items sacrificed</param>
    /// <param name="cultistCircleStashId">Id of stash item</param>
    /// <returns>The reward object</returns>
    protected List<List<Item>> GetDirectRewards(
        string sessionId,
        DirectRewardSettings directReward,
        string cultistCircleStashId
    )
    {
        // Prep rewards array (reward can be item with children, hence array of arrays)
        List<List<Item>> rewards = [];

        // Handle special case of tagilla helmets - only one reward is allowed
        if (directReward.Reward.Contains(ItemTpl.FACECOVER_TAGILLAS_WELDING_MASK_GORILLA))
        {
            directReward.Reward = [_randomUtil.GetArrayValue(directReward.Reward)];
        }

        // Loop because these can include multiple rewards
        foreach (var rewardTpl in directReward.Reward)
        {
            // Is weapon/armor, handle differently
            if (
                _itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(rewardTpl) ||
                _itemHelper.IsOfBaseclass(rewardTpl, BaseClasses.WEAPON)
            )
            {
                var defaultPreset = _presetHelper.GetDefaultPreset(rewardTpl);
                if (defaultPreset is null)
                {
                    _logger.Warning($"Reward tpl: {rewardTpl} lacks a default preset, skipping reward");

                    continue;
                }

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = _itemHelper.ReplaceIDs(defaultPreset.Items);
                _itemHelper.RemapRootItemId(presetAndMods);

                // Set item as FiR
                _itemHelper.SetFoundInRaid(presetAndMods);

                rewards.Add(presetAndMods);

                continue;
            }

            // 'Normal' item, non-preset
            var stackSize = GetDirectRewardBaseTypeStackSize(rewardTpl);
            List<Item> rewardItem =
            [
                new()
                {
                    Id = _hashUtil.Generate(),
                    Template = rewardTpl,
                    ParentId = cultistCircleStashId,
                    SlotId = CircleOfCultistSlotId,
                    Upd = new Upd
                    {
                        StackObjectsCount = stackSize
                    }
                }
            ];

            _itemHelper.SetFoundInRaid(rewardItem);

            // Edge case - item is ammo container and needs cartridges added
            if (_itemHelper.IsOfBaseclass(rewardTpl, BaseClasses.AMMO_BOX))
            {
                var itemDetails = _itemHelper.GetItem(rewardTpl).Value;
                _itemHelper.AddCartridgesToAmmoBox(rewardItem, itemDetails);
            }

            rewards.Add(rewardItem);
        }

        // Direct reward is not repeatable, flag collected in profile
        if (!directReward.Repeatable)
        {
            FlagDirectRewardAsAcceptedInProfile(sessionId, directReward);
        }

        return rewards;
    }

    /// <summary>
    ///     Check for direct rewards from what player sacrificed
    /// </summary>
    /// <param name="sessionId">sessionId</param>
    /// <param name="sacrificedItems">Items sacrificed</param>
    /// <returns>Direct reward items to send to player</returns>
    protected DirectRewardSettings CheckForDirectReward(
        string sessionId,
        List<Item> sacrificedItems,
        Dictionary<string, DirectRewardSettings> directRewardsCache
    )
    {
        // Get sacrificed tpls
        var sacrificedItemTpls = sacrificedItems.Select(item => item.Template).ToList();
        sacrificedItemTpls.Sort();
        // Create md5 key of the items player sacrificed so we can compare against the direct reward cache
        var sacrificedItemsKey = _hashUtil.GenerateMd5ForData(string.Concat(sacrificedItemTpls, ","));

        var matchingDirectReward = directRewardsCache.GetValueOrDefault(sacrificedItemsKey);
        if (matchingDirectReward is null)
            // No direct reward
        {
            return null;
        }

        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var directRewardHash = GetDirectRewardHashKey(matchingDirectReward);
        if (fullProfile.SptData.CultistRewards?.ContainsKey(directRewardHash) ?? false)
            // Player has already received this direct reward
        {
            return null;
        }

        return matchingDirectReward;
    }

    /// <summary>
    ///     Create an md5 key of the sacrificed + reward items
    /// </summary>
    /// <param name="directReward">Direct reward to create key for</param>
    /// <returns>Key</returns>
    protected string GetDirectRewardHashKey(DirectRewardSettings directReward)
    {
        directReward.RequiredItems.Sort();
        directReward.Reward.Sort();

        var required = string.Concat(directReward.RequiredItems, ",");
        var reward = string.Concat(directReward.Reward, ",");
        // Key is sacrificed items separated by commas, a dash, then the rewards separated by commas
        var key = $"{{{required}-{reward}}}";

        return _hashUtil.GenerateMd5ForData(key);
    }

    /// <summary>
    ///     Explicit rewards have their own stack sizes as they don't use a reward rouble pool
    /// </summary>
    /// <param name="rewardTpl">Item being rewarded to get stack size of</param>
    /// <returns>stack size of item</returns>
    protected int GetDirectRewardBaseTypeStackSize(string rewardTpl)
    {
        var itemDetails = _itemHelper.GetItem(rewardTpl);
        if (!itemDetails.Key)
        {
            _logger.Warning($"{rewardTpl} is not an item, setting stack size to 1");

            return 1;
        }

        // Look for parent in dict
        var settings = _hideoutConfig.CultistCircle.DirectRewardStackSize.GetValueOrDefault(itemDetails.Value.Parent);
        if (settings is null)
        {
            return 1;
        }

        return _randomUtil.GetInt((int) settings.Min, (int) settings.Max);
    }

    /// <summary>
    ///     Add a record to the player's profile to signal they have accepted a non-repeatable direct reward
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="directReward">Reward sent to player</param>
    protected void FlagDirectRewardAsAcceptedInProfile(string sessionId, DirectRewardSettings directReward)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var dataToStoreInProfile = new AcceptedCultistReward
        {
            Timestamp = _timeUtil.GetTimeStamp(),
            SacrificeItems = directReward.RequiredItems,
            RewardItems = directReward.Reward
        };

        fullProfile.SptData.CultistRewards[GetDirectRewardHashKey(directReward)] = dataToStoreInProfile;
    }

    /// <summary>
    ///     Get the size of a reward item's stack
    ///     1 for everything except ammo, ammo can be between min stack and max stack
    /// </summary>
    /// <param name="itemTpl">Item chosen</param>
    /// <param name="rewardPoolRemaining">Rouble amount of pool remaining to fill</param>
    /// <returns>Size of stack</returns>
    protected int GetRewardStackSize(string itemTpl, int rewardPoolRemaining)
    {
        if (_itemHelper.IsOfBaseclass(itemTpl, BaseClasses.AMMO))
        {
            var ammoTemplate = _itemHelper.GetItem(itemTpl).Value;
            return _itemHelper.GetRandomisedAmmoStackSize(ammoTemplate);
        }

        if (_itemHelper.IsOfBaseclass(itemTpl, BaseClasses.MONEY))
        {
            // Get currency-specific values from config
            var settings = _hideoutConfig.CultistCircle.CurrencyRewards[itemTpl];

            // What % of the pool remaining should be rewarded as chosen currency
            var percentOfPoolToUse = _randomUtil.GetDouble(settings.Min.Value, settings.Max.Value);

            // Rouble amount of pool we want to reward as currency
            var roubleAmountToFill = _randomUtil.GetPercentOfValue(percentOfPoolToUse, rewardPoolRemaining);

            // Convert currency to roubles
            var currencyPriceAsRouble = _itemHelper.GetItemPrice(itemTpl);

            // How many items can we fit into chosen pool
            var itemCountToReward = Math.Round(roubleAmountToFill / currencyPriceAsRouble ?? 0);

            return (int) itemCountToReward;
        }

        return 1;
    }

    /// <summary>
    ///     Get a pool of tpl IDs of items the player needs to complete hideout crafts/upgrade areas
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile of player who will be getting the rewards</param>
    /// <param name="craftingInfo">Do we return bonus items (hideout/task items)</param>
    /// <param name="cultistCircleConfig">Circle config</param>
    /// <returns>Array of tpls</returns>
    protected List<string> GetCultistCircleRewardPool(
        string sessionId,
        PmcData pmcData,
        CircleCraftDetails craftingInfo,
        CultistCircleSettings cultistCircleConfig)
    {
        var rewardPool = new HashSet<string>();
        var hideoutDbData = _databaseService.GetHideout();
        var itemsDb = _databaseService.GetItems();

        // Get all items that match the blacklisted types and fold into item blacklist below
        var itemTypeBlacklist = _itemFilterService.GetItemRewardBaseTypeBlacklist();
        var itemsMatchingTypeBlacklist = itemsDb
            .Where(templateItem => _itemHelper.IsOfBaseclasses(templateItem.Key, itemTypeBlacklist))
            .Select(templateItem => templateItem.Key);

        // Create set of unique values to ignore
        var itemRewardBlacklist = new HashSet<string>();
        itemRewardBlacklist.UnionWith(_seasonalEventService.GetInactiveSeasonalEventItems());
        itemRewardBlacklist.UnionWith(_itemFilterService.GetItemRewardBlacklist());
        itemRewardBlacklist.UnionWith(cultistCircleConfig.RewardItemBlacklist);
        itemRewardBlacklist.UnionWith(itemsMatchingTypeBlacklist);

        // Hideout and task rewards are ONLY if the bonus is active
        switch (craftingInfo.RewardType)
        {
            case CircleRewardType.RANDOM:
                {
                    // Does reward pass the high value threshold
                    var isHighValueReward = craftingInfo.RewardAmountRoubles >= cultistCircleConfig.HighValueThresholdRub;
                    GenerateRandomisedItemsAndAddToRewardPool(rewardPool, itemRewardBlacklist, isHighValueReward);

                    break;
                }
            case CircleRewardType.HIDEOUT_TASK:
                {
                    // Hideout/Task loot
                    AddHideoutUpgradeRequirementsToRewardPool(hideoutDbData, pmcData, itemRewardBlacklist, rewardPool);
                    AddTaskItemRequirementsToRewardPool(pmcData, itemRewardBlacklist, rewardPool);

                    // If we have no tasks or hideout stuff left or need more loot to fill it out, default to high value
                    if (rewardPool.Count < cultistCircleConfig.MaxRewardItemCount + 2)
                    {
                        GenerateRandomisedItemsAndAddToRewardPool(rewardPool, itemRewardBlacklist, true);
                    }

                    break;
                }
        }

        // Add custom rewards from config
        if (cultistCircleConfig.AdditionalRewardItemPool.Count > 0)
        {
            foreach (var additionalReward in cultistCircleConfig.AdditionalRewardItemPool)
            {
                if (itemRewardBlacklist.Contains(additionalReward))
                {
                    continue;
                }

                // Add tpl to reward pool
                rewardPool.Add(additionalReward);
            }
        }

        return rewardPool.ToList();
    }

    /// <summary>
    ///     Check player's profile for quests with hand-in requirements and add those required items to the pool
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemRewardBlacklist">Items not to add to pool</param>
    /// <param name="rewardPool">Pool to add items to</param>
    protected void AddTaskItemRequirementsToRewardPool(
        PmcData pmcData,
        HashSet<string> itemRewardBlacklist,
        HashSet<string> rewardPool)
    {
        var activeTasks = pmcData.Quests.Where(quest => quest.Status == QuestStatusEnum.Started);
        foreach (var task in activeTasks)
        {
            var questData = _questHelper.GetQuestFromDb(task.QId, pmcData);
            var handoverConditions = questData.Conditions.AvailableForFinish.Where(
                condition => condition.ConditionType == "HandoverItem"
            );
            foreach (var condition in handoverConditions)
            foreach (var neededItem in condition.Target.List)
            {
                if (itemRewardBlacklist.Contains(neededItem) || !_itemHelper.IsValidItem(neededItem))
                {
                    continue;
                }

                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug($"Added Task Loot: {_itemHelper.GetItemName(neededItem)}");
                }

                rewardPool.Add(neededItem);
            }
        }
    }

    /// <summary>
    ///     Adds items the player needs to complete hideout crafts/upgrades to the reward pool
    /// </summary>
    /// <param name="hideoutDbData">Hideout area data</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemRewardBlacklist">Items not to add to pool</param>
    /// <param name="rewardPool">Pool to add items to</param>
    protected void AddHideoutUpgradeRequirementsToRewardPool(
        Hideout hideoutDbData,
        PmcData pmcData,
        HashSet<string> itemRewardBlacklist,
        HashSet<string> rewardPool)
    {
        var dbAreas = hideoutDbData.Areas;
        foreach (var profileArea in GetPlayerAccessibleHideoutAreas(pmcData.Hideout.Areas))
        {
            var currentStageLevel = profileArea.Level;
            var areaType = profileArea.Type;

            // Get next stage of area
            var dbArea = dbAreas.FirstOrDefault(area => area.Type == areaType);
            var nextStageDbData = dbArea?.Stages[(currentStageLevel + 1).ToString()];
            if (nextStageDbData is not null)
            {
                // Next stage exists, gather up requirements and add to pool
                var itemRequirements = GetItemRequirements(nextStageDbData.Requirements);
                foreach (var rewardToAdd in itemRequirements)
                {
                    if (
                            itemRewardBlacklist.Contains(rewardToAdd.TemplateId) ||
                            !_itemHelper.IsValidItem(rewardToAdd.TemplateId)
                        )
                        // Dont reward items sacrificed
                    {
                        continue;
                    }

                    if (_logger.IsLogEnabled(LogLevel.Debug))
                    {
                        _logger.Debug($"Added Hideout Loot: {_itemHelper.GetItemName(rewardToAdd.TemplateId)}");
                    }

                    rewardPool.Add(rewardToAdd.TemplateId);
                }
            }
        }
    }

    /// <summary>
    ///     Get all active hideout areas
    /// </summary>
    /// <param name="areas">Hideout areas to iterate over</param>
    /// <returns>Active area array</returns>
    protected List<BotHideoutArea> GetPlayerAccessibleHideoutAreas(List<BotHideoutArea> areas)
    {
        return areas.Where(
                area =>
                {
                    if (area.Type == HideoutAreas.CHRISTMAS_TREE && !_seasonalEventService.ChristmasEventEnabled())
                        // Christmas tree area and not Christmas, skip
                    {
                        return false;
                    }

                    return true;
                }
            )
            .ToList();
    }

    /// <summary>
    ///     Get array of random reward items
    /// </summary>
    /// <param name="rewardPool">Reward pool to add to</param>
    /// <param name="itemRewardBlacklist">Item tpls to ignore</param>
    /// <param name="itemsShouldBeHighValue">Should these items meet the valuable threshold</param>
    /// <returns>Set of item tpls</returns>
    protected HashSet<string> GenerateRandomisedItemsAndAddToRewardPool(
        HashSet<string> rewardPool,
        HashSet<string> itemRewardBlacklist,
        bool itemsShouldBeHighValue)
    {
        var allItems = _itemHelper.GetItems();
        var currentItemCount = 0;
        var attempts = 0;
        // `currentItemCount` var will look for the correct number of items, `attempts` var will keep this from never stopping if the highValueThreshold is too high
        while (
            currentItemCount < _hideoutConfig.CultistCircle.MaxRewardItemCount + 2 &&
            attempts < allItems.Count
        )
        {
            attempts++;
            var randomItem = _randomUtil.GetArrayValue(allItems);
            if (itemRewardBlacklist.Contains(randomItem.Id) || !_itemHelper.IsValidItem(randomItem.Id))
            {
                continue;
            }

            // Valuable check
            if (itemsShouldBeHighValue)
            {
                var itemValue = _itemHelper.GetItemMaxPrice(randomItem.Id);
                if (itemValue < _hideoutConfig.CultistCircle.HighValueThresholdRub)
                {
                    continue;
                }
            }

            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Added: {_itemHelper.GetItemName(randomItem.Id)}");
            }

            rewardPool.Add(randomItem.Id);
            currentItemCount++;
        }

        return rewardPool;
    }

    /// <summary>
    ///     Iterate over passed in hideout requirements and return the Item
    /// </summary>
    /// <param name="requirements">Requirements to iterate over</param>
    /// <returns>Array of item requirements</returns>
    protected List<StageRequirement> GetItemRequirements(List<StageRequirement> requirements)
    {
        return requirements.Where(requirement => requirement.Type == "Item").ToList();
    }

    /// <summary>
    ///     Iterate over passed in hideout requirements and return the Item
    /// </summary>
    /// <param name="requirements">Requirements to iterate over</param>
    /// <returns>Array of item requirements</returns>
    protected List<Requirement> GetItemRequirements(List<Requirement> requirements)
    {
        return requirements.Where(requirement => requirement.Type == "Item").ToList();
    }

    /// <summary>
    ///     Create a map of the possible direct rewards, keyed by the items needed to be sacrificed
    /// </summary>
    /// <param name="directRewards">Direct rewards array from hideout config</param>
    /// <returns>Dictionary</returns>
    protected Dictionary<string, DirectRewardSettings> GenerateSacrificedItemsCache(List<DirectRewardSettings> directRewards)
    {
        var result = new Dictionary<string, DirectRewardSettings>();
        foreach (var rewardSettings in directRewards)
        {
            rewardSettings.RequiredItems.Sort();
            var concat = string.Concat(rewardSettings.RequiredItems, ",");

            var key = _hashUtil.GenerateMd5ForData(concat);
            result[key] = rewardSettings;
        }

        return result;
    }

    /// <summary>
    ///     Attempt to add all rewards to cultist circle, if they don't fit remove one and try again until they fit
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="rewards">Rewards to send to player</param>
    /// <param name="containerGrid">Cultist grid to add rewards to</param>
    /// <param name="cultistCircleStashId">Stash id</param>
    /// <param name="output">Client output</param>
    protected void AddRewardsToCircleContainer(
        string sessionId,
        PmcData pmcData,
        List<List<Item>> rewards,
        int[][] containerGrid,
        string cultistCircleStashId,
        ItemEventRouterResponse output
    )
    {
        var canAddToContainer = false;
        while (!canAddToContainer && rewards.Count > 0)
        {
            canAddToContainer = _inventoryHelper.CanPlaceItemsInContainer(
                _cloner.Clone(containerGrid), // MUST clone grid before passing in as function modifies grid
                rewards
            );

            // Doesn't fit, remove one item
            if (!canAddToContainer)
            {
                rewards.PopLast();
            }
        }

        foreach (var itemToAdd in rewards)
        {
            _inventoryHelper.PlaceItemInContainer(
                containerGrid,
                itemToAdd,
                cultistCircleStashId,
                CircleOfCultistSlotId
            );
            // Add item + mods to output and profile inventory
            pmcData.Inventory.Items.AddRange(itemToAdd);
        }
    }
}
