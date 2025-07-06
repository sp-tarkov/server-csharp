using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using Hideout = SPTarkov.Server.Core.Models.Spt.Hideout.Hideout;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class CircleOfCultistService(
    ISptLogger<CircleOfCultistService> logger,
    TimeUtil timeUtil,
    ICloner cloner,
    EventOutputHolder eventOutputHolder,
    RandomUtil randomUtil,
    HashUtil hashUtil,
    ItemHelper itemHelper,
    PresetHelper presetHelper,
    ProfileHelper profileHelper,
    InventoryHelper inventoryHelper,
    HideoutHelper hideoutHelper,
    QuestHelper questHelper,
    DatabaseService databaseService,
    ItemFilterService itemFilterService,
    SeasonalEventService seasonalEventService,
    ServerLocalisationService localisationService,
    ConfigServer configServer
)
{
    protected const string CircleOfCultistSlotId = "CircleOfCultistsGrid1";
    protected readonly HideoutConfig _hideoutConfig = configServer.GetConfig<HideoutConfig>();

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
        MongoId sessionId,
        PmcData pmcData,
        HideoutCircleOfCultistProductionStartRequestData request
    )
    {
        var output = eventOutputHolder.GetOutput(sessionId);

        var cultistCircleStashId = pmcData.Inventory?.HideoutAreaStashes?.GetValueOrDefault(
            ((int)HideoutAreas.CircleOfCultists).ToString()
        );
        if (cultistCircleStashId is null)
        {
            logger.Error(localisationService.GetText("cultistcircle-unable_to_find_stash_id"));

            return output;
        }

        // `cultistRecipes` just has single recipeId
        var cultistCraftData = databaseService
            .GetHideout()
            .Production.CultistRecipes.FirstOrDefault();
        var sacrificedItems = GetSacrificedItems(pmcData);
        var sacrificedItemCostRoubles = sacrificedItems.Aggregate(
            0D,
            (sum, curr) => sum + (itemHelper.GetItemPrice(curr.Template) ?? 0)
        );

        var rewardAmountMultiplier = GetRewardAmountMultiplier(
            pmcData,
            _hideoutConfig.CultistCircle
        );

        // Get the rouble amount we generate rewards with from cost of sacrificed items * above multiplier
        var rewardAmountRoubles = Math.Round(sacrificedItemCostRoubles * rewardAmountMultiplier);

        // Check if it matches any direct swap recipes
        var directRewardsCache = GenerateSacrificedItemsCache(
            _hideoutConfig.CultistCircle.DirectRewards
        );
        var directRewardSettings = CheckForDirectReward(
            sessionId,
            sacrificedItems,
            directRewardsCache
        );
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

        // Remove sacrificed items from circle inventory
        foreach (var item in sacrificedItems)
        {
            if (item.SlotId == CircleOfCultistSlotId)
            {
                inventoryHelper.RemoveItem(pmcData, item.Id, sessionId, output);
            }
        }

        var rewards = hasDirectReward
            ? GetDirectRewards(sessionId, directRewardSettings, cultistCircleStashId)
            : GetRewardsWithinBudget(
                GetCultistCircleRewardPool(
                    sessionId,
                    pmcData,
                    craftingInfo,
                    _hideoutConfig.CultistCircle
                ),
                rewardAmountRoubles,
                cultistCircleStashId.Value,
                _hideoutConfig.CultistCircle
            );

        // Get the container grid for cultist stash area
        var cultistStashDbItem = itemHelper.GetItem(
            ItemTpl.HIDEOUTAREACONTAINER_CIRCLEOFCULTISTS_STASH_1
        );

        // Ensure rewards fit into container
        var containerGrid = inventoryHelper.GetContainerSlotMap(cultistStashDbItem.Value.Id);
        AddRewardsToCircleContainer(
            sessionId,
            pmcData,
            rewards,
            containerGrid,
            cultistCircleStashId.Value,
            output
        );

        return output;
    }

    /// <summary>
    ///     Get the reward amount multiple value based on players hideout management skill + configs rewardPriceMultiplerMinMax values
    /// </summary>
    /// <param name="pmcData"> Player profile </param>
    /// <param name="cultistCircleSettings"> Circle config settings </param>
    /// <returns> Reward Amount Multiplier </returns>
    private double GetRewardAmountMultiplier(
        PmcData pmcData,
        CultistCircleSettings cultistCircleSettings
    )
    {
        // Get a randomised value to multiply the sacrificed rouble cost by
        var rewardAmountMultiplier = randomUtil.GetDouble(
            cultistCircleSettings.RewardPriceMultiplierMinMax.Min,
            cultistCircleSettings.RewardPriceMultiplierMinMax.Max
        );

        // Adjust value generated by the players hideout management skill
        var hideoutManagementSkill = pmcData.GetSkillFromProfile(SkillTypes.HideoutManagement);
        if (hideoutManagementSkill is not null)
        {
            rewardAmountMultiplier *= (float)(1 + hideoutManagementSkill.Progress / 10000); // 5100 becomes 0.51, add 1 to it, 1.51, multiply the bonus by it (e.g. 1.2 x 1.51)
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
        MongoId sessionId,
        PmcData pmcData,
        string recipeId,
        List<Item> sacrificedItems,
        double craftingTime
    )
    {
        // Create circle production/craft object to add to player profile
        var cultistProduction = hideoutHelper.InitProduction(recipeId, craftingTime, false);

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
            RewardAmountRoubles = (int)rewardAmountRoubles,
            RewardDetails = null,
        };

        // Direct reward edge case
        if (directRewardSettings is not null)
        {
            result.Time = directRewardSettings.CraftTimeSeconds;

            return result;
        }

        var random = new Random();

        // Get a threshold where sacrificed amount is between thresholds min and max
        var matchingThreshold = GetMatchingThreshold(
            circleConfig.CraftTimeThreshholds,
            rewardAmountRoubles
        );
        if (
            rewardAmountRoubles >= circleConfig.HideoutCraftSacrificeThresholdRub
            && random.Next(0, 1) <= circleConfig.BonusChanceMultiplier
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
            circleConfig.CraftTimeOverride != -1
                ? circleConfig.CraftTimeOverride
                : matchingThreshold.CraftTimeSeconds;

        result.RewardDetails = matchingThreshold;

        return result;
    }

    protected CraftTimeThreshold GetMatchingThreshold(
        List<CraftTimeThreshold> thresholds,
        double rewardAmountRoubles
    )
    {
        var matchingThreshold = thresholds.FirstOrDefault(craftThreshold =>
            craftThreshold.Min <= rewardAmountRoubles && craftThreshold.Max >= rewardAmountRoubles
        );

        // No matching threshold, make one
        if (matchingThreshold is null)
        {
            // None found, use a default
            logger.Warning(
                localisationService.GetText(
                    "cultistcircle-no_matching_threshhold_found",
                    new { rewardAmountRoubles = rewardAmountRoubles }
                )
            );

            // Use first threshold value (cheapest) from parameter array, otherwise use 12 hours
            var firstThreshold = thresholds.FirstOrDefault();
            var craftTime =
                firstThreshold?.CraftTimeSeconds > 0
                    ? firstThreshold.CraftTimeSeconds
                    : timeUtil.GetHoursAsSeconds(12);

            return new CraftTimeThreshold
            {
                Min = firstThreshold?.Min ?? 1,
                Max = firstThreshold?.Max ?? 34999,
                CraftTimeSeconds = craftTime,
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
        var inventoryRootItemsInCultistGrid = pmcData.Inventory.Items.Where(item =>
            item.SlotId == CircleOfCultistSlotId
        );

        // Get rootitem + its children
        List<Item> sacrificedItems = [];
        foreach (var rootItem in inventoryRootItemsInCultistGrid)
        {
            var rootItemWithChildren = pmcData.Inventory.Items.FindAndReturnChildrenAsItems(
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
        MongoId cultistCircleStashId,
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
            totalRewardCost < rewardBudget
            && rewardItemTplPool.Count > 0
            && rewardItemCount < circleConfig.MaxRewardItemCount
        )
        {
            if (failedAttempts > circleConfig.MaxAttemptsToPickRewardsWithinBudget)
            {
                logger.Warning($"Exiting reward generation after {failedAttempts} failed attempts");

                break;
            }

            // Choose a random tpl from pool
            var randomItemTplFromPool = randomUtil.GetArrayValue(rewardItemTplPool);

            // Is weapon/armor, handle differently
            if (
                itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(randomItemTplFromPool)
                || itemHelper.IsOfBaseclass(randomItemTplFromPool, BaseClasses.WEAPON)
            )
            {
                var defaultPreset = presetHelper.GetDefaultPreset(randomItemTplFromPool);
                if (defaultPreset is null)
                {
                    logger.Warning(
                        $"Reward tpl: {randomItemTplFromPool} lacks a default preset, skipping reward"
                    );
                    failedAttempts++;

                    continue;
                }

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = defaultPreset.Items.ReplaceIDs().ToList();
                presetAndMods.RemapRootItemId();

                // Set item as FiR
                itemHelper.SetFoundInRaid(presetAndMods);

                rewardItemCount++;
                totalRewardCost += (int)itemHelper.GetItemPrice(randomItemTplFromPool);
                rewards.Add(presetAndMods);

                continue;
            }

            // Some items can have variable stack size, e.g. ammo / currency
            var stackSize = GetRewardStackSize(
                randomItemTplFromPool,
                (int)(rewardBudget / (rewardItemCount == 0 ? 1 : rewardItemCount)) // Remaining rouble budget
            );

            // Not a weapon/armor, standard single item
            List<Item> rewardItem =
            [
                new()
                {
                    Id = new MongoId(),
                    Template = randomItemTplFromPool,
                    ParentId = cultistCircleStashId,
                    SlotId = CircleOfCultistSlotId,
                    Upd = new Upd { StackObjectsCount = stackSize },
                },
            ];

            itemHelper.SetFoundInRaid(rewardItem);

            // Edge case - item is ammo container and needs cartridges added
            if (itemHelper.IsOfBaseclass(randomItemTplFromPool, BaseClasses.AMMO_BOX))
            {
                var itemDetails = itemHelper.GetItem(randomItemTplFromPool).Value;
                itemHelper.AddCartridgesToAmmoBox(rewardItem, itemDetails);
            }

            // Increment price of rewards to give to player + add to reward array
            rewardItemCount++;
            var singleItemPrice = itemHelper.GetItemPrice(randomItemTplFromPool);
            var itemPrice = singleItemPrice * stackSize;
            totalRewardCost += (int)itemPrice;

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
        MongoId sessionId,
        DirectRewardSettings directReward,
        string cultistCircleStashId
    )
    {
        // Prep rewards array (reward can be item with children, hence array of arrays)
        List<List<Item>> rewards = [];

        // Handle special case of tagilla helmets - only one reward is allowed
        if (directReward.Reward.Contains(ItemTpl.FACECOVER_TAGILLAS_WELDING_MASK_GORILLA))
        {
            directReward.Reward = [randomUtil.GetArrayValue(directReward.Reward)];
        }

        // Loop because these can include multiple rewards
        foreach (var rewardTpl in directReward.Reward)
        {
            // Is weapon/armor, handle differently
            if (
                itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(rewardTpl)
                || itemHelper.IsOfBaseclass(rewardTpl, BaseClasses.WEAPON)
            )
            {
                var defaultPreset = presetHelper.GetDefaultPreset(rewardTpl);
                if (defaultPreset is null)
                {
                    logger.Warning(
                        $"Reward tpl: {rewardTpl} lacks a default preset, skipping reward"
                    );

                    continue;
                }

                // Ensure preset has unique ids and is cloned so we don't alter the preset data stored in memory
                var presetAndMods = defaultPreset.Items.ReplaceIDs().ToList();
                presetAndMods.RemapRootItemId();

                // Set item as FiR
                itemHelper.SetFoundInRaid(presetAndMods);

                rewards.Add(presetAndMods);

                continue;
            }

            // 'Normal' item, non-preset
            var stackSize = GetDirectRewardBaseTypeStackSize(rewardTpl);
            List<Item> rewardItem =
            [
                new()
                {
                    Id = new MongoId(),
                    Template = rewardTpl,
                    ParentId = cultistCircleStashId,
                    SlotId = CircleOfCultistSlotId,
                    Upd = new Upd { StackObjectsCount = stackSize },
                },
            ];

            itemHelper.SetFoundInRaid(rewardItem);

            // Edge case - item is ammo container and needs cartridges added
            if (itemHelper.IsOfBaseclass(rewardTpl, BaseClasses.AMMO_BOX))
            {
                var itemDetails = itemHelper.GetItem(rewardTpl).Value;
                itemHelper.AddCartridgesToAmmoBox(rewardItem, itemDetails);
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
    protected DirectRewardSettings? CheckForDirectReward(
        MongoId sessionId,
        List<Item> sacrificedItems,
        Dictionary<string, DirectRewardSettings> directRewardsCache
    )
    {
        // Get sacrificed tpls
        var sacrificedItemTpls = sacrificedItems
            .Select(item => item.Template)
            .Where(item => item != null);
        // Create md5 key of the items player sacrificed so we can compare against the direct reward cache
        var sacrificedItemsKey = CreateSacrificeCacheKey(sacrificedItemTpls);

        var matchingDirectReward = directRewardsCache.GetValueOrDefault(sacrificedItemsKey);
        if (matchingDirectReward is null)
        // No direct reward
        {
            return null;
        }

        var fullProfile = profileHelper.GetFullProfile(sessionId);
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

        return hashUtil.GenerateHashForData(HashingAlgorithm.MD5, key);
    }

    /// <summary>
    ///     Explicit rewards have their own stack sizes as they don't use a reward rouble pool
    /// </summary>
    /// <param name="rewardTpl">Item being rewarded to get stack size of</param>
    /// <returns>stack size of item</returns>
    protected int GetDirectRewardBaseTypeStackSize(string rewardTpl)
    {
        var itemDetails = itemHelper.GetItem(rewardTpl);
        if (!itemDetails.Key)
        {
            logger.Warning($"{rewardTpl} is not an item, setting stack size to 1");

            return 1;
        }

        // Look for parent in dict
        var settings = _hideoutConfig.CultistCircle.DirectRewardStackSize.GetValueOrDefault(
            itemDetails.Value.Parent
        );
        if (settings is null)
        {
            return 1;
        }

        return randomUtil.GetInt(settings.Min, settings.Max);
    }

    /// <summary>
    ///     Add a record to the player's profile to signal they have accepted a non-repeatable direct reward
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="directReward">Reward sent to player</param>
    protected void FlagDirectRewardAsAcceptedInProfile(
        MongoId sessionId,
        DirectRewardSettings directReward
    )
    {
        var fullProfile = profileHelper.GetFullProfile(sessionId);
        var dataToStoreInProfile = new AcceptedCultistReward
        {
            Timestamp = timeUtil.GetTimeStamp(),
            SacrificeItems = directReward.RequiredItems,
            RewardItems = directReward.Reward,
        };

        fullProfile.SptData.CultistRewards[GetDirectRewardHashKey(directReward)] =
            dataToStoreInProfile;
    }

    /// <summary>
    ///     Get the size of a reward item's stack
    ///     1 for everything except ammo, ammo can be between min stack and max stack
    /// </summary>
    /// <param name="itemTpl">Item chosen</param>
    /// <param name="rewardPoolRemaining">Rouble amount of pool remaining to fill</param>
    /// <returns>Size of stack</returns>
    protected int GetRewardStackSize(MongoId itemTpl, int rewardPoolRemaining)
    {
        if (itemHelper.IsOfBaseclass(itemTpl, BaseClasses.AMMO))
        {
            var ammoTemplate = itemHelper.GetItem(itemTpl).Value;
            return itemHelper.GetRandomisedAmmoStackSize(ammoTemplate);
        }

        if (itemHelper.IsOfBaseclass(itemTpl, BaseClasses.MONEY))
        {
            // Get currency-specific values from config
            var settings = _hideoutConfig.CultistCircle.CurrencyRewards[itemTpl];

            // What % of the pool remaining should be rewarded as chosen currency
            var percentOfPoolToUse = randomUtil.GetDouble(settings.Min, settings.Max);

            // Rouble amount of pool we want to reward as currency
            var roubleAmountToFill = randomUtil.GetPercentOfValue(
                percentOfPoolToUse,
                rewardPoolRemaining
            );

            // Convert currency to roubles
            var currencyPriceAsRouble = itemHelper.GetItemPrice(itemTpl);

            // How many items can we fit into chosen pool
            var itemCountToReward = Math.Round(roubleAmountToFill / currencyPriceAsRouble ?? 0);

            return (int)itemCountToReward;
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
        MongoId sessionId,
        PmcData pmcData,
        CircleCraftDetails craftingInfo,
        CultistCircleSettings cultistCircleConfig
    )
    {
        var rewardPool = new HashSet<string>();
        var hideoutDbData = databaseService.GetHideout();
        var itemsDb = databaseService.GetItems();

        // Get all items that match the blacklisted types and fold into item blacklist below
        var itemTypeBlacklist = itemFilterService.GetItemRewardBaseTypeBlacklist();
        var itemsMatchingTypeBlacklist = itemsDb
            .Where(templateItem => itemHelper.IsOfBaseclasses(templateItem.Key, itemTypeBlacklist))
            .Select(templateItem => templateItem.Key);

        // Create set of unique values to ignore
        var itemRewardBlacklist = new HashSet<MongoId>();
        itemRewardBlacklist.UnionWith(seasonalEventService.GetInactiveSeasonalEventItems());
        itemRewardBlacklist.UnionWith(itemFilterService.GetItemRewardBlacklist());
        itemRewardBlacklist.UnionWith(itemFilterService.GetBlacklistedItems());
        itemRewardBlacklist.UnionWith(cultistCircleConfig.RewardItemBlacklist);
        itemRewardBlacklist.UnionWith(itemsMatchingTypeBlacklist);

        // Hideout and task rewards are ONLY if the bonus is active
        switch (craftingInfo.RewardType)
        {
            case CircleRewardType.RANDOM:
            {
                // Does reward pass the high value threshold
                var isHighValueReward =
                    craftingInfo.RewardAmountRoubles >= cultistCircleConfig.HighValueThresholdRub;
                GenerateRandomisedItemsAndAddToRewardPool(
                    rewardPool,
                    itemRewardBlacklist,
                    isHighValueReward
                );

                break;
            }
            case CircleRewardType.HIDEOUT_TASK:
            {
                // Hideout/Task loot
                AddHideoutUpgradeRequirementsToRewardPool(
                    hideoutDbData,
                    pmcData,
                    itemRewardBlacklist,
                    rewardPool
                );
                AddTaskItemRequirementsToRewardPool(pmcData, itemRewardBlacklist, rewardPool);

                // If we have no tasks or hideout stuff left or need more loot to fill it out, default to high value
                if (rewardPool.Count < cultistCircleConfig.MaxRewardItemCount + 2)
                {
                    GenerateRandomisedItemsAndAddToRewardPool(
                        rewardPool,
                        itemRewardBlacklist,
                        true
                    );
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
        HashSet<MongoId> itemRewardBlacklist,
        HashSet<string> rewardPool
    )
    {
        var activeTasks = pmcData.Quests.Where(quest => quest.Status == QuestStatusEnum.Started);
        foreach (var task in activeTasks)
        {
            var questData = questHelper.GetQuestFromDb(task.QId, pmcData);
            var handoverConditions = questData.Conditions.AvailableForFinish.Where(condition =>
                condition.ConditionType == "HandoverItem"
            );
            foreach (var condition in handoverConditions)
            foreach (var neededItem in condition.Target.List)
            {
                if (itemRewardBlacklist.Contains(neededItem) || !itemHelper.IsValidItem(neededItem))
                {
                    continue;
                }

                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Added Task Loot: {itemHelper.GetItemName(neededItem)}");
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
        HashSet<MongoId> itemRewardBlacklist,
        HashSet<string> rewardPool
    )
    {
        var dbAreas = hideoutDbData.Areas;
        foreach (var profileArea in GetPlayerAccessibleHideoutAreas(pmcData.Hideout.Areas))
        {
            var currentStageLevel = profileArea.Level;
            var areaType = profileArea.Type;

            // Get next stage of area
            var dbArea = dbAreas?.FirstOrDefault(area => area.Type == areaType);
            var nextTargetStageLevel = (currentStageLevel + 1).ToString() ?? "";
            if (dbArea?.Stages?.TryGetValue(nextTargetStageLevel, out var nextStageDbData) ?? false)
            {
                // Next stage exists, gather up requirements and add to pool
                var itemRequirements = GetItemRequirements(nextStageDbData.Requirements);
                foreach (var rewardToAdd in itemRequirements)
                {
                    if (
                        itemRewardBlacklist.Contains(rewardToAdd.TemplateId)
                        || !itemHelper.IsValidItem(rewardToAdd.TemplateId)
                    )
                    // Dont reward items sacrificed
                    {
                        continue;
                    }

                    if (logger.IsLogEnabled(LogLevel.Debug))
                    {
                        logger.Debug(
                            $"Added Hideout Loot: {itemHelper.GetItemName(rewardToAdd.TemplateId)}"
                        );
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
        return areas
            .Where(area =>
            {
                if (
                    area.Type == HideoutAreas.ChristmasIllumination
                    && !seasonalEventService.ChristmasEventEnabled()
                )
                // Christmas tree area and not Christmas, skip
                {
                    return false;
                }

                return true;
            })
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
        HashSet<MongoId> itemRewardBlacklist,
        bool itemsShouldBeHighValue
    )
    {
        var allItems = itemHelper.GetItems();
        var currentItemCount = 0;
        var attempts = 0;
        // `currentItemCount` var will look for the correct number of items, `attempts` var will keep this from never stopping if the highValueThreshold is too high
        while (
            currentItemCount < _hideoutConfig.CultistCircle.MaxRewardItemCount + 2
            && attempts < allItems.Count
        )
        {
            attempts++;
            var randomItem = randomUtil.GetArrayValue(allItems);
            if (
                itemRewardBlacklist.Contains(randomItem.Id)
                || !itemHelper.IsValidItem(randomItem.Id)
            )
            {
                continue;
            }

            // Valuable check
            if (itemsShouldBeHighValue)
            {
                var itemValue = itemHelper.GetItemMaxPrice(randomItem.Id);
                if (itemValue < _hideoutConfig.CultistCircle.HighValueThresholdRub)
                {
                    continue;
                }
            }

            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Added: {itemHelper.GetItemName(randomItem.Id)}");
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
    /// Temporary until MongoId conversion is done, should be removed and the mongoid one kept in place
    /// </summary>
    /// <param name="requiredItems"></param>
    /// <returns></returns>
    protected string CreateSacrificeCacheKey(IEnumerable<string> requiredItems)
    {
        var concat = string.Join(",", requiredItems.OrderBy(item => item));
        return hashUtil.GenerateHashForData(HashingAlgorithm.MD5, concat);
    }

    protected string CreateSacrificeCacheKey(IEnumerable<MongoId> requiredItems)
    {
        var concat = string.Join(",", requiredItems.OrderBy(item => item));
        return hashUtil.GenerateHashForData(HashingAlgorithm.MD5, concat);
    }

    /// <summary>
    ///     Create a map of the possible direct rewards, keyed by the items needed to be sacrificed
    /// </summary>
    /// <param name="directRewards">Direct rewards array from hideout config</param>
    /// <returns>Dictionary</returns>
    protected Dictionary<string, DirectRewardSettings> GenerateSacrificedItemsCache(
        List<DirectRewardSettings> directRewards
    )
    {
        var result = new Dictionary<string, DirectRewardSettings>();
        foreach (var rewardSettings in directRewards)
        {
            string key = CreateSacrificeCacheKey(rewardSettings.RequiredItems);
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
        MongoId sessionId,
        PmcData pmcData,
        List<List<Item>> rewards,
        int[,] containerGrid,
        MongoId cultistCircleStashId,
        ItemEventRouterResponse output
    )
    {
        var canAddToContainer = false;
        while (!canAddToContainer && rewards.Count > 0)
        {
            canAddToContainer = inventoryHelper.CanPlaceItemsInContainer(
                cloner.Clone(containerGrid), // MUST clone grid before passing in as function modifies grid
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
            inventoryHelper.PlaceItemInContainer(
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
