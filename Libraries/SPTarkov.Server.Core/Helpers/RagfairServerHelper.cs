using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class RagfairServerHelper(
    ISptLogger<RagfairServerHelper> logger,
    RandomUtil randomUtil,
    TimeUtil timeUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    TraderHelper traderHelper,
    WeightedRandomHelper weightedRandomHelper,
    MailSendService mailSendService,
    LocalisationService localisationService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected const string goodsReturnedTemplate = "5bdabfe486f7743e1665df6e 0"; // Your item was not sold
    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();

    /**
     * Is item valid / on blacklist / quest item
     * @param itemDetails
     * @returns boolean
     */
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
        if (blacklistConfig.EnableBsgList && !(itemDetails.Value?.Properties?.CanSellOnRagfair ?? false))
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
            blacklistConfig.EnableCustomItemCategoryList &&
            IsItemCategoryOnCustomFleaBlacklist(itemDetails.Value.Parent)
        )
        {
            return false;
        }

        // Skip quest items
        if (blacklistConfig.EnableQuestList && itemHelper.IsQuestItem(itemDetails.Value.Id))
        {
            return false;
        }

        // Don't include damaged ammo packs
        if (
            ragfairConfig.Dynamic.Blacklist.DamagedAmmoPacks &&
            itemDetails.Value.Parent == BaseClasses.AMMO_BOX &&
            itemDetails.Value.Name.Contains("_damaged")
        )
        {
            return false;
        }

        return true;
    }

    /**
     * Is supplied item tpl on the ragfair custom blacklist from configs/ragfair.json/dynamic
     * @param itemTemplateId Item tpl to check is blacklisted
     * @returns True if its blacklisted
     */
    protected bool IsItemOnCustomFleaBlacklist(string itemTemplateId)
    {
        return ragfairConfig.Dynamic.Blacklist.Custom.Contains(itemTemplateId);
    }

    /**
     * Is supplied parent id on the ragfair custom item category blacklist
     * @param parentId Parent Id to check is blacklisted
     * @returns true if blacklisted
     */
    protected bool IsItemCategoryOnCustomFleaBlacklist(string itemParentId)
    {
        return ragfairConfig.Dynamic.Blacklist.CustomItemCategoryList.Contains(itemParentId);
    }

    /**
     * is supplied id a trader
     * @param traderId
     * @returns True if id was a trader
     */
    public bool IsTrader(string traderId)
    {
        return databaseService.GetTraders().ContainsKey(traderId);
    }

    /**
     * Send items back to player
     * @param sessionID Player to send items to
     * @param returnedItems Items to send to player
     */
    public void ReturnItems(string sessionID, List<Item> returnedItems)
    {
        mailSendService.SendLocalisedNpcMessageToPlayer(
            sessionID,
            traderHelper.GetTraderById(Traders.RAGMAN).ToString(),
            MessageType.MESSAGE_WITH_ITEMS,
            goodsReturnedTemplate,
            returnedItems,
            timeUtil.GetHoursAsSeconds((int) databaseService.GetGlobals().Configuration.RagFair.YourOfferDidNotSellMaxStorageTimeInHour)
        );
    }

    public int CalculateDynamicStackCount(string tplId, bool isWeaponPreset)
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
        if (isWeaponPreset ||
            itemHelper.IsOfBaseclasses((MongoId) itemDetails.Value.Id, ragfairConfig.Dynamic.ShowAsSingleStack)
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
        var stackPercent = randomUtil.GetDouble(config.StackablePercent.Min, config.StackablePercent.Max);

        // Min value to return should be no less than 1
        return Math.Max((int) randomUtil.GetPercentOfValue(stackPercent, maxStackSize, 0), 1);
    }

    /**
     * Choose a currency at random with bias
     * @returns currency tpl
     */
    public string GetDynamicOfferCurrency()
    {
        return weightedRandomHelper.GetWeightedValue(ragfairConfig.Dynamic.Currencies);
    }

    /**
     * Given a preset id from globals.json, return an array of items[] with unique ids
     * @param item Preset item
     * @returns Array of weapon and its children
     */
    public List<Item> GetPresetItems(Item item)
    {
        var preset = cloner.Clone(databaseService.GetGlobals().ItemPresets[item.Id].Items);
        return itemHelper.ReparentItemAndChildren(item, preset);
    }

    /**
     * Possible bug, returns all items associated with an items tpl, could be multiple presets from globals.json
     * @param item Preset item
     * @returns
     */
    public List<Item> GetPresetItemsByTpl(Item item)
    {
        var presets = new List<Item>();
        foreach (var itemId in databaseService.GetGlobals().ItemPresets.Keys)
        {
            if (databaseService.GetGlobals().ItemPresets[itemId].Items[0].Template == item.Template)
            {
                var presetItems = cloner.Clone(databaseService.GetGlobals().ItemPresets[itemId].Items);
                presets.AddRange(itemHelper.ReparentItemAndChildren(item, presetItems));
            }
        }

        return presets;
    }
}
