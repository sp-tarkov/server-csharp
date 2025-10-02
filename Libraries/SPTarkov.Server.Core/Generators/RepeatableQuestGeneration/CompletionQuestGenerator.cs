using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Repeatable;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;

namespace SPTarkov.Server.Core.Generators.RepeatableQuestGeneration;

[Injectable]
public class CompletionQuestGenerator(
    ISptLogger<CompletionQuestGenerator> logger,
    RepeatableQuestHelper repeatableQuestHelper,
    RepeatableQuestRewardGenerator repeatableQuestRewardGenerator,
    DatabaseService databaseService,
    SeasonalEventService seasonalEventService,
    ServerLocalisationService localisationService,
    RandomUtil randomUtil,
    MathUtil mathUtil,
    ItemHelper itemHelper
) : IRepeatableQuestGenerator
{
    protected const int MaxRandomNumberAttempts = 6;

    /// <summary>
    ///     Generates a valid Completion quest
    /// </summary>
    /// <param name="sessionId">session Id to generate the quest for</param>
    /// <param name="pmcLevel">player's level for requested items and reward generation</param>
    /// <param name="traderId">trader from which the quest will be provided</param>
    /// <param name="questTypePool"></param>
    /// <param name="repeatableConfig">
    ///     The configuration for the repeatably kind (daily, weekly) as configured in QuestConfig
    ///     for the requested quest
    /// </param>
    /// <returns>quest type format for "Completion" (see assets/database/templates/repeatableQuests.json)</returns>
    public RepeatableQuest? Generate(
        MongoId sessionId,
        int pmcLevel,
        MongoId traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig
    )
    {
        var completionConfig = repeatableQuestHelper.GetCompletionConfigByPmcLevel(pmcLevel, repeatableConfig);
        if (completionConfig is null)
        {
            logger.Warning(localisationService.GetText("repeatable-completion_config_no_template", new { pmcLevel }));
            return null;
        }

        var levelsConfig = repeatableConfig.RewardScaling.Levels;
        var roublesConfig = repeatableConfig.RewardScaling.Roubles;

        var quest = repeatableQuestHelper.GenerateRepeatableTemplate(
            RepeatableQuestType.Completion,
            traderId,
            repeatableConfig.Side,
            sessionId
        );

        if (quest is null)
        {
            logger.Error("Quest template null when attempting to create completion operational task.");
            return null;
        }

        // Filter the items.json items to items the player must retrieve to complete quest: shouldn't be a quest item or "non-existent"
        var itemsToRetrievePool = GetItemsToRetrievePool(completionConfig, repeatableConfig.RewardBlacklist);

        // Filter items within our budget
        var (hashSet, budget) = GetItemsWithinBudget(pmcLevel, levelsConfig, roublesConfig, itemsToRetrievePool);
        itemsToRetrievePool = hashSet;

        // We also have the option to use whitelist and/or blacklist which is defined in repeatableQuests.json as
        // [{"minPlayerLevel": 1, "itemIds": ["id1",...]}, {"minPlayerLevel": 15, "itemIds": ["id3",...]}]
        if (completionConfig.UseWhitelist)
        {
            itemsToRetrievePool = GetWhitelistedItemSelection(itemsToRetrievePool, pmcLevel);
        }

        if (completionConfig.UseBlacklist)
        {
            itemsToRetrievePool = GetBlacklistedItemSelection(itemsToRetrievePool, pmcLevel);
        }

        // Filtering too harsh
        if (itemsToRetrievePool.Count == 0)
        {
            logger.Error(localisationService.GetText("repeatable-completion_quest_whitelist_too_small_or_blacklist_too_restrictive"));

            return null;
        }

        var selectedItems = GenerateAvailableForFinish(quest, completionConfig, itemsToRetrievePool.ToList(), budget);

        quest.Rewards = repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            1,
            traderId,
            repeatableConfig,
            completionConfig,
            selectedItems.ToHashSet()
        );

        return quest;
    }

    /// <summary>
    /// Generate a pool of item tpls the player should reasonably be able to retrieve
    /// </summary>
    /// <param name="completionConfig">Completion quest type config</param>
    /// <param name="itemTplBlacklist">Item tpls to not add to pool</param>
    /// <returns>Set of item tpls</returns>
    protected HashSet<MongoId> GetItemsToRetrievePool(CompletionConfig completionConfig, HashSet<MongoId> itemTplBlacklist)
    {
        // Get seasonal items that should not be added to pool as seasonal event is not active
        var seasonalItems = seasonalEventService.GetInactiveSeasonalEventItems();

        // Check for specific base classes which don't make sense as reward item
        // also check if the price is greater than 0; there are some items whose price can not be found
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

                // Valid reward items share same logic as items to retrieve
                return repeatableQuestRewardGenerator.IsValidRewardItem(
                    itemTemplate.Id,
                    itemTplBlacklist,
                    completionConfig.RequiredItemTypeBlacklist
                );
            })
            .Select(item => item.Id)
            .ToHashSet();
    }

    /// <summary>
    ///     Filter item pool down to items we can afford on our budget
    /// </summary>
    /// <param name="pmcLevel">Level of pmc</param>
    /// <param name="levelsConfig">Levels config</param>
    /// <param name="roublesConfig">Roubles config</param>
    /// <param name="itemsToRetrievePool">Item pool</param>
    /// <returns>Filtered items and roubles budget</returns>
    protected (HashSet<MongoId>, double) GetItemsWithinBudget(
        int pmcLevel,
        List<double> levelsConfig,
        List<double> roublesConfig,
        HashSet<MongoId> itemsToRetrievePool
    )
    {
        // Be fair, don't value the items be more expensive than the reward
        var multiplier = randomUtil.GetDouble(0.5, 1);
        var roublesBudget = Math.Floor(mathUtil.Interp1(pmcLevel, levelsConfig, roublesConfig) * multiplier);

        // Make sure there is always a 5000 rouble budget available for selection
        roublesBudget = Math.Max(roublesBudget, 5000d);

        return (itemsToRetrievePool.Where(itemTpl => itemHelper.GetItemPrice(itemTpl) < roublesBudget).ToHashSet(), roublesBudget);
    }

    /// <summary>
    ///     Filter item selection to items in the whitelist
    /// </summary>
    /// <param name="itemSelection">Item selection to filter</param>
    /// <param name="pmcLevel">Level of pmc</param>
    /// <returns>Filtered selection, or original if null or empty</returns>
    protected HashSet<MongoId> GetWhitelistedItemSelection(HashSet<MongoId> itemSelection, int pmcLevel)
    {
        var itemWhitelist = databaseService.GetTemplates().RepeatableQuests.Data?.Completion?.ItemsWhitelist;

        // Whitelist doesn't exist or is empty, return original
        if (itemWhitelist is null || itemWhitelist.Count == 0)
        {
            return itemSelection;
        }

        // Filter and concatenate items according to current player level
        var itemIdsWhitelisted = itemWhitelist.Where(p => p.MinPlayerLevel <= pmcLevel).SelectMany(x => x.ItemIds ?? []).ToHashSet(); //.Aggregate((a, p) => a.Concat(p.ItemIds), []);

        var filteredSelection = itemSelection
            .Where(x =>
            {
                // Whitelist can contain item tpls and item base type ids
                return itemIdsWhitelisted.Any(v => itemHelper.IsOfBaseclass(x, v)) || itemIdsWhitelisted.Contains(x);
            })
            .ToHashSet();

        // check if items are missing
        // var flatList = itemSelection.reduce((a, il) => a.concat(il[0]), []);
        // var missing = itemIdsWhitelisted.filter(l => !flatList.includes(l));

        return filteredSelection;
    }

    /// <summary>
    ///     Filter item selection based on the blacklist
    /// </summary>
    /// <param name="itemSelection">Item selection to filter</param>
    /// <param name="pmcLevel">Level of pmc</param>
    /// <returns>Filtered selection, or original if null or empty</returns>
    protected HashSet<MongoId> GetBlacklistedItemSelection(HashSet<MongoId> itemSelection, int pmcLevel)
    {
        var itemBlacklist = databaseService.GetTemplates().RepeatableQuests.Data?.Completion?.ItemsBlacklist;

        // Blacklist doesn't exist or is empty, return original
        if (itemBlacklist is null || itemBlacklist.Count == 0)
        {
            return itemSelection;
        }

        // Filter and concatenate the arrays according to current player level
        var itemIdsBlacklisted = itemBlacklist
            .Where(blacklist => blacklist.MinPlayerLevel <= pmcLevel)
            .SelectMany(blacklist => blacklist.ItemIds ?? [])
            .ToHashSet(); //.Aggregate(List<ItemsBlacklist> , (a, p) => a.Concat(p.ItemIds) );

        var filteredSelection = itemSelection
            .Where(x =>
            {
                return itemIdsBlacklisted.All(v => !itemHelper.IsOfBaseclass(x, v)) || !itemIdsBlacklisted.Contains(x);
            })
            .ToHashSet();

        return filteredSelection;
    }

    /// <summary>
    ///     Generate the available for finish conditions for this quest
    /// </summary>
    /// <param name="quest">Quest to add the conditions to</param>
    /// <param name="completionConfig">Completion config</param>
    /// <param name="itemSelection">Filtered item selection</param>
    /// <param name="roublesBudget">Budget in roubles</param>
    /// <returns>Chosen item template Ids</returns>
    protected List<MongoId> GenerateAvailableForFinish(
        RepeatableQuest quest,
        CompletionConfig completionConfig,
        List<MongoId> itemSelection,
        double roublesBudget
    )
    {
        // Store the indexes of items we are asking player to supply
        var distinctItemsToRetrieveCount = randomUtil.GetInt(completionConfig.UniqueItemCount.Min, completionConfig.UniqueItemCount.Max);
        var chosenRequirementItemsTpls = new List<MongoId>();
        var usedItemIndexes = new HashSet<int>();

        for (var i = 0; i < distinctItemsToRetrieveCount; i++)
        {
            var chosenItemIndex = randomUtil.RandInt(itemSelection.Count);
            var found = false;

            for (var j = 0; j < MaxRandomNumberAttempts; j++)
            {
                if (usedItemIndexes.Contains(chosenItemIndex))
                {
                    chosenItemIndex = randomUtil.RandInt(itemSelection.Count);
                }
                else
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                logger.Error(
                    localisationService.GetText("repeatable-no_reward_item_found_in_price_range", new { minPrice = 0, roublesBudget })
                );

                return chosenRequirementItemsTpls;
            }

            // Store index of item we've already chosen for later checking
            usedItemIndexes.Add(chosenItemIndex);

            var tplChosen = itemSelection[chosenItemIndex];
            var itemPrice = itemHelper.GetItemPrice(tplChosen)!.Value;
            var minValue = completionConfig.RequestedItemCount.Min;
            var maxValue = completionConfig.RequestedItemCount.Max;

            var value = minValue;

            // Get the value range within budget
            var x = (int)Math.Floor(roublesBudget / itemPrice);
            maxValue = Math.Min(maxValue, x);
            if (maxValue > minValue)
            // If it doesn't blow the budget we have for the request, draw a random amount of the selected
            // Item type to be requested
            {
                value = randomUtil.RandInt(minValue, maxValue + 1);
            }

            roublesBudget -= value * itemPrice;

            // Push a CompletionCondition with the item and the amount of the item into quest
            chosenRequirementItemsTpls.Add(tplChosen);
            quest.Conditions.AvailableForFinish!.Add(GenerateCondition(tplChosen, value, completionConfig));

            // Is there budget left for more items
            if (roublesBudget > 0)
            {
                // Reduce item pool to fit budget
                itemSelection = itemSelection.Where(tpl => itemHelper.GetItemPrice(tpl) < roublesBudget).ToList();

                if (itemSelection.Count == 0)
                {
                    // Nothing fits new budget, exit
                    break;
                }

                continue;
            }

            break;
        }

        return chosenRequirementItemsTpls;
    }

    /// <summary>
    ///     A repeatable quest, besides some more or less static components, exists of reward and condition (see
    ///     assets/database/templates/repeatableQuests.json)
    ///     This is a helper method for GenerateCompletionQuest to create a completion condition (of which a completion quest
    ///     theoretically can have many)
    /// </summary>
    /// <param name="itemTpl">Id of the item to request</param>
    /// <param name="value">Amount of items of this specific type to request</param>
    /// <param name="completionConfig">Completion config from quest.json</param>
    /// <returns>object of "Completion"-condition</returns>
    protected QuestCondition GenerateCondition(MongoId itemTpl, double value, CompletionConfig completionConfig)
    {
        var onlyFoundInRaid = completionConfig.RequiredItemsAreFiR;
        var minDurability = itemHelper.IsOfBaseclasses(itemTpl, [BaseClasses.WEAPON, BaseClasses.ARMOR])
            ? randomUtil.GetArrayValue(
                [completionConfig.RequiredItemMinDurabilityMinMax.Min, completionConfig.RequiredItemMinDurabilityMinMax.Max]
            )
            : 0;

        // Dog tags MUST NOT be FiR for them to work
        if (itemHelper.IsDogtag(itemTpl) || itemHelper.IsOfBaseclass(itemTpl, BaseClasses.AMMO))
        {
            onlyFoundInRaid = false;
        }

        return new QuestCondition
        {
            Id = new MongoId(),
            Index = 0,
            ParentId = string.Empty,
            DynamicLocale = true,
            VisibilityConditions = [],
            Target = new ListOrT<string>([itemTpl], null),
            Value = value,
            MinDurability = minDurability,
            MaxDurability = 100,
            DogtagLevel = 0,
            OnlyFoundInRaid = onlyFoundInRaid,
            IsEncoded = false,
            ConditionType = "HandoverItem",
        };
    }
}
