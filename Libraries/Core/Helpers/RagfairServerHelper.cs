using System.Runtime.InteropServices.JavaScript;
using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Helpers;

[Injectable]
public class RagfairServerHelper(
    ISptLogger<RagfairServerHelper> logger,
    RandomUtil randomUtil,
    TimeUtil timeUtil,
    SaveServer saveServer,
    DatabaseService databaseService,
    ProfileHelper profileHelper,
    ItemHelper itemHelper,
    TraderHelper traderHelper,
    MailSendService mailSendService,
    LocalisationService localisationService,
    ItemFilterService itemFilterService,
    ConfigServer configServer,
    ICloner cloner
)
{
    protected RagfairConfig ragfairConfig = configServer.GetConfig<RagfairConfig>();
    protected QuestConfig questConfig = configServer.GetConfig<QuestConfig>();
    protected static string goodsReturnedTemplate = "5bdabfe486f7743e1665df6e 0"; // Your item was not sold
    
    /**
     * Is item valid / on blacklist / quest item
     * @param itemDetails
     * @returns boolean
     */
    public bool IsItemValidRagfairItem(KeyValuePair<bool, TemplateItem?> itemDetails)
    {
        var blacklistConfig = ragfairConfig.Dynamic.Blacklist;

        // Skip invalid items
        if (!itemDetails.Key) {
            return false;
        }

        if (!itemHelper.IsValidItem(itemDetails.Value.Id)) {
            return false;
        }

        // Skip bsg blacklisted items
        if (blacklistConfig.EnableBsgList && !(itemDetails.Value?.Properties?.CanSellOnRagfair ?? false)) {
            return false;
        }

        // Skip custom blacklisted items and flag as unsellable by players
        if (IsItemOnCustomFleaBlacklist(itemDetails.Value.Id)) {
            itemDetails.Value.Properties.CanSellOnRagfair = false;

            return false;
        }

        // Skip custom category blacklisted items
        if (
            blacklistConfig.EnableCustomItemCategoryList &&
            IsItemCategoryOnCustomFleaBlacklist(itemDetails.Value.Parent)
        ) {
            return false;
        }

        // Skip quest items
        if (blacklistConfig.EnableQuestList && itemHelper.IsQuestItem(itemDetails.Value.Id)) {
            return false;
        }

        // Don't include damaged ammo packs
        if (
            ragfairConfig.Dynamic.Blacklist.DamagedAmmoPacks &&
            itemDetails.Value.Parent == BaseClasses.AMMO_BOX &&
            itemDetails[1]._name.includes("_damaged")
        ) {
            return false;
        }

        return true;
    }

    /**
     * Is supplied item tpl on the ragfair custom blacklist from configs/ragfair.json/dynamic
     * @param itemTemplateId Item tpl to check is blacklisted
     * @returns True if its blacklsited
     */
    protected isItemOnCustomFleaBlacklist(itemTemplateId: string): boolean {
        return ragfairConfig.dynamic.blacklist.custom.includes(itemTemplateId);
    }

    /**
     * Is supplied parent id on the ragfair custom item category blacklist
     * @param parentId Parent Id to check is blacklisted
     * @returns true if blacklisted
     */
    protected isItemCategoryOnCustomFleaBlacklist(itemParentId: string): boolean {
        return ragfairConfig.dynamic.blacklist.customItemCategoryList.includes(itemParentId);
    }

    /**
     * is supplied id a trader
     * @param traderId
     * @returns True if id was a trader
     */
    public isTrader(traderId: string): boolean {
        return traderId in databaseService.getTraders();
    }

    /**
     * Send items back to player
     * @param sessionID Player to send items to
     * @param returnedItems Items to send to player
     */
    public returnItems(sessionID: string, returnedItems: IItem[]): void {
        mailSendService.sendLocalisedNpcMessageToPlayer(
            sessionID,
            traderHelper.getTraderById(Traders.RAGMAN),
            MessageType.MESSAGE_WITH_ITEMS,
            RagfairServerHelper.goodsReturnedTemplate,
            returnedItems,
            timeUtil.getHoursAsSeconds(
                databaseService.getGlobals().config.RagFair.yourOfferDidNotSellMaxStorageTimeInHour,
            ),
        );
    }

    public calculateDynamicStackCount(tplId: string, isWeaponPreset: boolean): number {
        var config = ragfairConfig.dynamic;

        // Lookup item details - check if item not found
        var itemDetails = itemHelper.getItem(tplId);
        if (!itemDetails[0]) {
            throw new JSType.Error(
                localisationService.getText(
                    "ragfair-item_not_in_db_unable_to_generate_dynamic_stack_count",
                    tplId,
                ),
            );
        }

        // Item Types to return one of
        if (
            isWeaponPreset ||
            itemHelper.isOfBaseclasses(itemDetails[1]._id, ragfairConfig.dynamic.showAsSingleStack)
        ) {
            return 1;
        }

        // Get max stack count
        var maxStackCount = itemDetails[1]._props.StackMaxSize;

        // non-stackable - use different values to calculate stack size
        if (!maxStackCount || maxStackCount === 1) {
            return Math.round(randomUtil.getInt(config.nonStackableCount.min, config.nonStackableCount.max));
        }

        var stackPercent = Math.round(
            randomUtil.getInt(config.stackablePercent.min, config.stackablePercent.max),
        );

        return Math.round((maxStackCount / 100) * stackPercent);
    }

    /**
     * Choose a currency at random with bias
     * @returns currency tpl
     */
    public getDynamicOfferCurrency(): string {
        var currencies = ragfairConfig.dynamic.currencies;
        var bias: string[] = [];

        for (var item in currencies) {
            for (let i = 0; i < currencies[item]; i++) {
                bias.push(item);
            }
        }

        return bias[Math.floor(Math.random() * bias.length)];
    }

    /**
     * Given a preset id from globals.json, return an array of items[] with unique ids
     * @param item Preset item
     * @returns Array of weapon and its children
     */
    public getPresetItems(item: IItem): IItem[] {
        var preset = cloner.clone(databaseService.getGlobals().ItemPresets[item._id]._items);
        return itemHelper.reparentItemAndChildren(item, preset);
    }

    /**
     * Possible bug, returns all items associated with an items tpl, could be multiple presets from globals.json
     * @param item Preset item
     * @returns
     */
    public getPresetItemsByTpl(item: IItem): IItem[] {
        var presets = [];
        for (var itemId in databaseService.getGlobals().ItemPresets) {
            if (databaseService.getGlobals().ItemPresets[itemId]._items[0]._tpl === item._tpl) {
                var presetItems = cloner.clone(databaseService.getGlobals().ItemPresets[itemId]._items);
                presets.push(itemHelper.reparentItemAndChildren(item, presetItems));
            }
        }

        return presets;
    }
}
