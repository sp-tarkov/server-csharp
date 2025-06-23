using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
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
public class PickupQuestGenerator(
    ISptLogger<PickupQuestGenerator> logger,
    RepeatableQuestHelper repeatableQuestHelper,
    RepeatableQuestRewardGenerator repeatableQuestRewardGenerator,
    DatabaseService databaseService,
    LocalisationService localisationService,
    RandomUtil randomUtil,
    MathUtil mathUtil,
    HashUtil hashUtil
) : IRepeatableQuestGenerator
{

    // TODO: This isn't really implemented well at all, what even is this.
    public RepeatableQuest? Generate(
        string sessionId,
        int pmcLevel,
        string traderId,
        QuestTypePool questTypePool,
        RepeatableQuestConfig repeatableConfig)
    {
        var pickupConfig = repeatableConfig.QuestConfig.Pickup;

        var quest = repeatableQuestHelper.GenerateRepeatableTemplate(
            RepeatableQuestType.Pickup,
            traderId,
            repeatableConfig.Side,
            sessionId
        );

        var itemTypeToFetchWithCount = randomUtil.GetArrayValue(
            pickupConfig.ItemTypeToFetchWithMaxCount
        );

        var itemCountToFetch = randomUtil.RandInt(
            itemTypeToFetchWithCount.MinimumPickupCount.Value,
            itemTypeToFetchWithCount.MaximumPickupCount + 1
        );
        // Choose location - doesn't seem to work for anything other than 'any'
        // var locationKey: string = this.randomUtil.drawRandomFromDict(questTypePool.pool.Pickup.locations)[0];
        // var locationTarget = questTypePool.pool.Pickup.locations[locationKey];

        var findCondition = quest.Conditions.AvailableForFinish.FirstOrDefault(x =>
            x.ConditionType == "FindItem"
        );
        findCondition.Target = new ListOrT<string>([itemTypeToFetchWithCount.ItemType], null);
        findCondition.Value = itemCountToFetch;

        var counterCreatorCondition = quest.Conditions.AvailableForFinish.FirstOrDefault(x =>
            x.ConditionType == "CounterCreator"
        );
        // var locationCondition = counterCreatorCondition._props.counter.conditions.find(x => x._parent === "Location");
        // (locationCondition._props as ILocationConditionProps).target = [...locationTarget];

        var equipmentCondition = counterCreatorCondition.Counter.Conditions.FirstOrDefault(x =>
            x.ConditionType == "Equipment"
        );
        equipmentCondition.EquipmentInclusive =
        [
            [itemTypeToFetchWithCount.ItemType],
        ];

        // Add rewards
        quest.Rewards = repeatableQuestRewardGenerator.GenerateReward(
            pmcLevel,
            1,
            traderId,
            repeatableConfig,
            pickupConfig
        );

        return quest;
    }
}
