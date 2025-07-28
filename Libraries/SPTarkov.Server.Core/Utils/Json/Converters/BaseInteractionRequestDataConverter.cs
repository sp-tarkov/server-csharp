using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Request;
using SPTarkov.Server.Core.Models.Eft.Customization;
using SPTarkov.Server.Core.Models.Eft.Health;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.Inventory;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Models.Eft.Quests;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Eft.Repair;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Eft.Wishlist;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public class BaseInteractionRequestDataConverter : JsonConverter<BaseInteractionRequestData>
{
    private static readonly Dictionary<string, Func<string, BaseInteractionRequestData?>> _modHandlers = [];

    public override BaseInteractionRequestData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);

        // Get request as raw JSON
        var jsonText = jsonDocument.RootElement.GetRawText();

        // Get the underlying 'type' of action the client is requesting we do
        // Handle nullability here in case action's GetString is null
        var action = jsonDocument.RootElement.GetProperty("Action").GetString() ?? string.Empty;

        return ConvertToCorrectType(action, jsonDocument.RootElement, jsonText, options);
    }

    /// <summary>
    /// Handle the players action from received from client
    /// </summary>
    /// <param name="action">e.g. "Eat"</param>
    /// <param name="jsonDocumentRoot">Root json element of client request</param>
    /// <param name="jsonText">Raw JSON request text</param>
    /// <param name="options">Json parsing options</param>
    /// <returns>BaseInteractionRequestData</returns>
    private static BaseInteractionRequestData? ConvertToCorrectType(
        string action,
        JsonElement jsonDocumentRoot,
        string jsonText,
        JsonSerializerOptions options
    )
    {
        switch (action)
        {
            case ItemEventActions.CUSTOMIZATION_BUY:
                return JsonSerializer.Deserialize<BuyClothingRequestData>(jsonText, options);
            case ItemEventActions.CUSTOMIZATION_SET:
                return JsonSerializer.Deserialize<CustomizationSetRequest>(jsonText, options);
            case ItemEventActions.EAT:
                return JsonSerializer.Deserialize<OffraidEatRequestData>(jsonText, options);
            case ItemEventActions.HEAL:
                return JsonSerializer.Deserialize<OffraidHealRequestData>(jsonText, options);
            case ItemEventActions.RESTORE_HEALTH:
                return JsonSerializer.Deserialize<HealthTreatmentRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_UPGRADE:
                return JsonSerializer.Deserialize<HideoutUpgradeRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE:
                return JsonSerializer.Deserialize<HideoutUpgradeCompleteRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS:
                return JsonSerializer.Deserialize<HideoutPutItemInRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS:
                return JsonSerializer.Deserialize<HideoutTakeItemOutRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_TOGGLE_AREA:
                return JsonSerializer.Deserialize<HideoutToggleAreaRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutSingleProductionStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutScavCaseStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutContinuousProductionStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_TAKE_PRODUCTION:
                return JsonSerializer.Deserialize<HideoutTakeProductionRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS:
                return JsonSerializer.Deserialize<RecordShootingRangePoints>(jsonText, options);
            case HideoutEventActions.HIDEOUT_IMPROVE_AREA:
            case HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND:
                return JsonSerializer.Deserialize<HideoutImproveAreaRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutCircleOfCultistProductionStartRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND:
                return JsonSerializer.Deserialize<HideoutDeleteProductionRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND:
                return JsonSerializer.Deserialize<HideoutCustomizationApplyRequestData>(jsonText, options);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE:
                return JsonSerializer.Deserialize<HideoutCustomizationSetMannequinPoseRequest>(jsonText, options);
            case ItemEventActions.INSURE:
                return JsonSerializer.Deserialize<InsureRequestData>(jsonText, options);
            case ItemEventActions.ADD_TO_WISHLIST:
                return JsonSerializer.Deserialize<AddToWishlistRequest>(jsonText, options);
            case ItemEventActions.REMOVE_FROM_WISHLIST:
                return JsonSerializer.Deserialize<RemoveFromWishlistRequest>(jsonText, options);
            case ItemEventActions.CHANGE_WISHLIST_ITEM_CATEGORY:
                return JsonSerializer.Deserialize<ChangeWishlistItemCategoryRequest>(jsonText, options);
            case ItemEventActions.TRADING_CONFIRM:
            {
                switch (jsonDocumentRoot.GetProperty("type").GetString())
                {
                    case ItemEventActions.BUY_FROM_TRADER:
                        return JsonSerializer.Deserialize<ProcessBuyTradeRequestData>(jsonText, options);
                    case ItemEventActions.SELL_TO_TRADER:
                        return JsonSerializer.Deserialize<ProcessSellTradeRequestData>(jsonText, options);
                    default:
                        throw new Exception(
                            $"Unhandled action type: {action}, make sure BaseInteractionRequestDataConverter has deserialization for this action."
                        );
                }
            }
            case ItemEventActions.RAGFAIR_BUY_OFFER:
                return JsonSerializer.Deserialize<ProcessRagfairTradeRequestData>(jsonText, options);
            case ItemEventActions.SELL_ALL_FROM_SAVAGE:
                return JsonSerializer.Deserialize<SellScavItemsToFenceRequestData>(jsonText, options);
            case ItemEventActions.REPAIR:
                return JsonSerializer.Deserialize<RepairActionDataRequest>(jsonText, options);
            case ItemEventActions.TRADER_REPAIR:
                return JsonSerializer.Deserialize<TraderRepairActionDataRequest>(jsonText, options);
            case ItemEventActions.RAGFAIR_ADD_OFFER:
                return JsonSerializer.Deserialize<AddOfferRequestData>(jsonText, options);
            case ItemEventActions.RAGFAIR_REMOVE_OFFER:
                return JsonSerializer.Deserialize<RemoveOfferRequestData>(jsonText, options);
            case ItemEventActions.RAGFAIR_RENEW_OFFER:
                return JsonSerializer.Deserialize<ExtendOfferRequestData>(jsonText, options);
            case ItemEventActions.QUEST_ACCEPT:
                return JsonSerializer.Deserialize<AcceptQuestRequestData>(jsonText, options);
            case ItemEventActions.QUEST_COMPLETE:
                return JsonSerializer.Deserialize<CompleteQuestRequestData>(jsonText, options);
            case ItemEventActions.QUEST_HANDOVER:
                return JsonSerializer.Deserialize<HandoverQuestRequestData>(jsonText, options);
            case ItemEventActions.REPEATABLE_QUEST_CHANGE:
                return JsonSerializer.Deserialize<RepeatableQuestChangeRequest>(jsonText, options);
            case ItemEventActions.ADD_NOTE:
            case ItemEventActions.EDIT_NOTE:
            case ItemEventActions.DELETE_NOTE:
                return JsonSerializer.Deserialize<NoteActionRequest>(jsonText, options);
            case ItemEventActions.MOVE:
                return JsonSerializer.Deserialize<InventoryMoveRequestData>(jsonText, options);
            case ItemEventActions.REMOVE:
                return JsonSerializer.Deserialize<InventoryRemoveRequestData>(jsonText, options);
            case ItemEventActions.SPLIT:
                return JsonSerializer.Deserialize<InventorySplitRequestData>(jsonText, options);
            case ItemEventActions.MERGE:
                return JsonSerializer.Deserialize<InventoryMergeRequestData>(jsonText, options);
            case ItemEventActions.TRANSFER:
                return JsonSerializer.Deserialize<InventoryTransferRequestData>(jsonText, options);
            case ItemEventActions.SWAP:
                return JsonSerializer.Deserialize<InventorySwapRequestData>(jsonText, options);
            case ItemEventActions.FOLD:
                return JsonSerializer.Deserialize<InventoryFoldRequestData>(jsonText, options);
            case ItemEventActions.TOGGLE:
                return JsonSerializer.Deserialize<InventoryToggleRequestData>(jsonText, options);
            case ItemEventActions.TAG:
                return JsonSerializer.Deserialize<InventoryTagRequestData>(jsonText, options);
            case ItemEventActions.BIND:
            case ItemEventActions.UNBIND:
                return JsonSerializer.Deserialize<InventoryBindRequestData>(jsonText, options);
            case ItemEventActions.EXAMINE:
                return JsonSerializer.Deserialize<InventoryExamineRequestData>(jsonText, options);
            case ItemEventActions.READ_ENCYCLOPEDIA:
                return JsonSerializer.Deserialize<InventoryReadEncyclopediaRequestData>(jsonText, options);
            case ItemEventActions.APPLY_INVENTORY_CHANGES:
                return JsonSerializer.Deserialize<InventorySortRequestData>(jsonText, options);
            case ItemEventActions.CREATE_MAP_MARKER:
                return JsonSerializer.Deserialize<InventoryCreateMarkerRequestData>(jsonText, options);
            case ItemEventActions.DELETE_MAP_MARKER:
                return JsonSerializer.Deserialize<InventoryDeleteMarkerRequestData>(jsonText, options);
            case ItemEventActions.EDIT_MAP_MARKER:
                return JsonSerializer.Deserialize<InventoryEditMarkerRequestData>(jsonText, options);
            case ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER:
                return JsonSerializer.Deserialize<OpenRandomLootContainerRequestData>(jsonText, options);
            case ItemEventActions.HIDEOUT_QTE_EVENT:
                return JsonSerializer.Deserialize<HandleQTEEventRequestData>(jsonText, options);
            case ItemEventActions.REDEEM_PROFILE_REWARD:
                return JsonSerializer.Deserialize<RedeemProfileRequestData>(jsonText, options);
            case ItemEventActions.SET_FAVORITE_ITEMS:
                return JsonSerializer.Deserialize<SetFavoriteItems>(jsonText, options);
            case ItemEventActions.QUEST_FAIL:
                return JsonSerializer.Deserialize<FailQuestRequestData>(jsonText, options);
            case ItemEventActions.PIN_LOCK:
                return JsonSerializer.Deserialize<PinOrLockItemRequest>(jsonText, options);
            default:
                if (_modHandlers.TryGetValue(action, out var handler))
                {
                    return handler(jsonText);
                }
                throw new Exception(
                    $"Unhandled action type {action}, make sure the BaseInteractionRequestDataConverter has the deserialization for this action handled."
                );
        }
    }

    public static void RegisterModDataHandler(string action, Func<string, BaseInteractionRequestData?> handler)
    {
        if (!_modHandlers.TryAdd(action, handler))
        {
            throw new Exception($"Unable to register action {action} to BaseInteractionRequestDataConverter as it already exists.");
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseInteractionRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
