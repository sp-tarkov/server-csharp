using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RagfairServerHelper(
    ISptLogger<RagfairServerHelper> logger,
    RandomUtil randomUtil,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    WeightedRandomHelper weightedRandomHelper,
    MailSendService mailSendService,
    ServerLocalisationService localisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected const string goodsReturnedTemplate = "5bdabfe486f7743e1665df6e 0"; // Your item was not sold
    protected readonly RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /// <summary>
    /// Is item valid / not on blacklist / not a quest item
    /// </summary>
    /// <param name="itemDetails">Item to check</param>
    /// <returns></returns>
    public bool IsItemValidRagfairItem(KeyValuePair<bool, TemplateItem?> itemDetails)
    {
        var blacklistConfig = ragfairConfig.Dynamic.Blacklist;

        // Skip invalid items
        if (!itemDetails.Key)
        {
            return false;
        }

        if (!itemHelper.IsValidItem(itemDetails.Value.Id))
        {
            return false;
        }

        // Skip bsg blacklisted items
        if (
            blacklistConfig.EnableBsgList
            && !(itemDetails.Value?.Properties?.CanSellOnRagfair ?? false)
        )
        {
            return false;
        }

        // Skip custom blacklisted items and flag as unsellable by players
        if (IsItemOnCustomFleaBlacklist(itemDetails.Value.Id))
        {
            itemDetails.Value.Properties.CanSellOnRagfair = false;

            return false;
        }

        // Skip custom category blacklisted items
        if (
            blacklistConfig.EnableCustomItemCategoryList
            && IsItemCategoryOnCustomFleaBlacklist(itemDetails.Value.Parent)
        )
        {
            return false;
        }

        // Skip quest items
        if (blacklistConfig.EnableQuestList && itemDetails.Value.IsQuestItem())
        {
            return false;
        }

        // Don't include damaged ammo packs
        if (
            ragfairConfig.Dynamic.Blacklist.DamagedAmmoPacks
            && itemDetails.Value.Parent == BaseClasses.AMMO_BOX
            && itemDetails.Value.Name.Contains("_damaged")
        )
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Is supplied item tpl on the ragfair custom blacklist from configs/ragfair.json/dynamic
    /// </summary>
    /// <param name="itemTemplateId">Item tpl to check is blacklisted</param>
    /// <returns>True if its blacklisted</returns>
    protected bool IsItemOnCustomFleaBlacklist(MongoId itemTemplateId)
    {
        return ragfairConfig.Dynamic.Blacklist.Custom.Contains(itemTemplateId);
    }

    /// <summary>
    /// Is supplied parent id on the ragfair custom item category blacklist
    /// </summary>
    /// <param name="itemParentId">Parent Id to check is blacklisted</param>
    /// <returns>true if blacklisted</returns>
    protected bool IsItemCategoryOnCustomFleaBlacklist(MongoId itemParentId)
    {
        return ragfairConfig.Dynamic.Blacklist.CustomItemCategoryList.Contains(itemParentId);
    }

    /// <summary>
    /// Is supplied id a trader
    /// </summary>
    /// <param name="traderId">id to check</param>
    /// <returns>True if id was a trader</returns>
    public bool IsTrader(MongoId traderId)
    {
        return databaseService.GetTraders().ContainsKey(traderId);
    }

    /// <summary>
    /// Send items back to player
    /// </summary>
    /// <param name="sessionID">Player to send items to</param>
    /// <param name="returnedItems">Items to send to player</param>
    public void ReturnItems(MongoId sessionID, IEnumerable<Item> returnedItems)
    {
        mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionID,
            Traders.RAGMAN,
            MessageType.MessageWithItems,
            goodsReturnedTemplate,
            returnedItems,
            timeUtil.GetHoursAsSeconds(
                (int)
                    databaseService
                        .GetGlobals()
                        .Configuration.RagFair.YourOfferDidNotSellMaxStorageTimeInHour
            )
        );
    }

    public int CalculateDynamicStackCount(MongoId tplId, bool isPreset)
    {
        var config = ragfairConfig.Dynamic;

        // Lookup item details - check if item not found
        var itemDetails = itemHelper.GetItem(tplId);
        if (!itemDetails.Key)
        {
            throw new Exception(
                localisationService.GetText(
                    "ragfair-item_not_in_db_unable_to_generate_dynamic_stack_count",
                    tplId
                )
            );
        }

        // Item Types to return one of
        if (
            isPreset
            || itemHelper.IsOfBaseclasses(
                itemDetails.Value.Id,
                ragfairConfig.Dynamic.ShowAsSingleStack
            )
        )
        {
            return 1;
        }

        // Get max possible stack count
        var maxStackSize = itemDetails.Value?.Properties?.StackMaxSize ?? 1;

        // non-stackable - use different values to calculate stack size
        if (maxStackSize == 1)
        {
            return randomUtil.GetInt(config.NonStackableCount.Min, config.NonStackableCount.Max);
        }

        // Get a % to get of stack size
        var stackPercent = randomUtil.GetDouble(
            config.StackablePercent.Min,
            config.StackablePercent.Max
        );

        // Min value to return should be no less than 1
        return Math.Max((int)randomUtil.GetPercentOfValue(stackPercent, maxStackSize, 0), 1);
    }

    /// <summary>
    /// Choose a currency at random with bias
    /// </summary>
    /// <returns>Currency TPL</returns>
    public MongoId GetDynamicOfferCurrency()
    {
        return weightedRandomHelper.GetWeightedValue(
            ragfairConfig.Dynamic.OfferCurrencyChangePercent
        );
    }

    /// <summary>
    /// Given a preset id from globals.json, return an array of items[] with unique ids
    /// </summary>
    /// <param name="item">Preset item</param>
    /// <returns>Collection containing weapon and its children</returns>
    public List<Item> GetPresetItems(Item item)
    {
        if (!databaseService.GetGlobals().ItemPresets.TryGetValue(item.Id, out var presetToClone))
        {
            return [];
        }

        // Re-parent and clone the matching preset found
        return itemHelper.ReparentItemAndChildren(item, cloner.Clone(presetToClone.Items));
    }

    /// <summary>
    /// Possible bug, returns all items associated with an items tpl, could be multiple presets from globals.json
    /// </summary>
    /// <param name="item">Preset item</param>
    /// <returns>Collection of item objects</returns>
    public List<Item> GetPresetItemsByTpl(Item item)
    {
        var presets = new List<Item>();
        foreach (var itemId in databaseService.GetGlobals().ItemPresets.Keys)
        {
            if (
                databaseService.GetGlobals().ItemPresets.TryGetValue(itemId, out var presetsOfItem)
                && presetsOfItem.Items?.FirstOrDefault()?.Template == item.Template
            )
            {
                // Add a clone of the found preset into list above
                presets.AddRange(
                    itemHelper.ReparentItemAndChildren(item, cloner.Clone(presetsOfItem.Items))
                );
            }
        }

        return presets;
    }

    /// <summary>
    /// Get a randomised offer count for the provided item base type
    /// </summary>
    /// <param name="itemParentType">Parent type for the item</param>
    /// <returns>randomised number between min and max</returns>
    public int GetOfferCountByBaseType(MongoId itemParentType)
    {
        if (!ragfairConfig.Dynamic.OfferItemCount.TryGetValue(itemParentType, out var minMaxRange))
        {
            minMaxRange = ragfairConfig.Dynamic.OfferItemCount.GetValueOrDefault("default");
        }

        return randomUtil.GetInt(minMaxRange.Min, minMaxRange.Max);
    }
}
