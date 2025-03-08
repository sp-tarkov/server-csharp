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
    public override BaseInteractionRequestData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        // Need to read the actual JSON text here so we can convert to the correct type
        var jsonText = jsonDocument.RootElement.GetRawText();
        var value = JsonSerializer.Deserialize<BaseInteractionRequestData>(jsonText);
        return ConvertToCorrectType(value, jsonText);
    }

    private BaseInteractionRequestData? ConvertToCorrectType(BaseInteractionRequestData? value, string jsonText)
    {
        switch (value.Action)
        {
            case ItemEventActions.CUSTOMIZATION_BUY:
                return JsonSerializer.Deserialize<BuyClothingRequestData>(jsonText);
            case ItemEventActions.CUSTOMIZATION_SET:
                return JsonSerializer.Deserialize<CustomizationSetRequest>(jsonText);
            case ItemEventActions.EAT:
                return JsonSerializer.Deserialize<OffraidEatRequestData>(jsonText);
            case ItemEventActions.HEAL:
                return JsonSerializer.Deserialize<OffraidHealRequestData>(jsonText);
            case ItemEventActions.RESTORE_HEALTH:
                return JsonSerializer.Deserialize<HealthTreatmentRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_UPGRADE:
                return JsonSerializer.Deserialize<HideoutUpgradeRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_UPGRADE_COMPLETE:
                return JsonSerializer.Deserialize<HideoutUpgradeCompleteRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_PUT_ITEMS_IN_AREA_SLOTS:
                return JsonSerializer.Deserialize<HideoutPutItemInRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_TAKE_ITEMS_FROM_AREA_SLOTS:
                return JsonSerializer.Deserialize<HideoutTakeItemOutRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_TOGGLE_AREA:
                return JsonSerializer.Deserialize<HideoutToggleAreaRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_SINGLE_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutSingleProductionStartRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_SCAV_CASE_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutScavCaseStartRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_CONTINUOUS_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutContinuousProductionStartRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_TAKE_PRODUCTION:
                return JsonSerializer.Deserialize<HideoutTakeProductionRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_RECORD_SHOOTING_RANGE_POINTS:
                return JsonSerializer.Deserialize<RecordShootingRangePoints>(jsonText);
            case HideoutEventActions.HIDEOUT_IMPROVE_AREA:
            case HideoutEventActions.HIDEOUT_CANCEL_PRODUCTION_COMMAND:
                return JsonSerializer.Deserialize<HideoutImproveAreaRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_CIRCLE_OF_CULTIST_PRODUCTION_START:
                return JsonSerializer.Deserialize<HideoutCircleOfCultistProductionStartRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_DELETE_PRODUCTION_COMMAND:
                return JsonSerializer.Deserialize<HideoutDeleteProductionRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_APPLY_COMMAND:
                return JsonSerializer.Deserialize<HideoutCustomizationApplyRequestData>(jsonText);
            case HideoutEventActions.HIDEOUT_CUSTOMIZATION_SET_MANNEQUIN_POSE:
                return JsonSerializer.Deserialize<HideoutCustomizationSetMannequinPoseRequest>(jsonText);
            case ItemEventActions.INSURE:
                return JsonSerializer.Deserialize<InsureRequestData>(jsonText);
            case ItemEventActions.ADD_TO_WISHLIST:
                return JsonSerializer.Deserialize<AddToWishlistRequest>(jsonText);
            case ItemEventActions.REMOVE_FROM_WISHLIST:
                return JsonSerializer.Deserialize<RemoveFromWishlistRequest>(jsonText);
            case ItemEventActions.CHANGE_WISHLIST_ITEM_CATEGORY:
                return JsonSerializer.Deserialize<ChangeWishlistItemCategoryRequest>(jsonText);
            case ItemEventActions.TRADING_CONFIRM:
                {
                    var json = JsonSerializer.Deserialize<ProcessBaseTradeRequestData>(jsonText);

                    switch (json.Type)
                    {
                        case ItemEventActions.BUY_FROM_TRADER:
                            return JsonSerializer.Deserialize<ProcessBuyTradeRequestData>(jsonText);
                        case ItemEventActions.SELL_TO_TRADER:
                            return JsonSerializer.Deserialize<ProcessSellTradeRequestData>(jsonText);
                        default:
                            throw new Exception(
                                $"Unhandled action type {value.Action}, make sure the BaseInteractionRequestDataConverter has the deserialization for this action handled."
                            );
                    }
                }
            case ItemEventActions.RAGFAIR_BUY_OFFER:
                return JsonSerializer.Deserialize<ProcessRagfairTradeRequestData>(jsonText);
            case ItemEventActions.SELL_ALL_FROM_SAVAGE:
                return JsonSerializer.Deserialize<SellScavItemsToFenceRequestData>(jsonText);
            case ItemEventActions.REPAIR:
                return JsonSerializer.Deserialize<RepairActionDataRequest>(jsonText);
            case ItemEventActions.TRADER_REPAIR:
                return JsonSerializer.Deserialize<TraderRepairActionDataRequest>(jsonText);
            case ItemEventActions.RAGFAIR_ADD_OFFER:
                return JsonSerializer.Deserialize<AddOfferRequestData>(jsonText);
            case ItemEventActions.RAGFAIR_REMOVE_OFFER:
                return JsonSerializer.Deserialize<RemoveOfferRequestData>(jsonText);
            case ItemEventActions.RAGFAIR_RENEW_OFFER:
                return JsonSerializer.Deserialize<ExtendOfferRequestData>(jsonText);
            case ItemEventActions.QUEST_ACCEPT:
                return JsonSerializer.Deserialize<AcceptQuestRequestData>(jsonText);
            case ItemEventActions.QUEST_COMPLETE:
                return JsonSerializer.Deserialize<CompleteQuestRequestData>(jsonText);
            case ItemEventActions.QUEST_HANDOVER:
                return JsonSerializer.Deserialize<HandoverQuestRequestData>(jsonText);
            case ItemEventActions.REPEATABLE_QUEST_CHANGE:
                return JsonSerializer.Deserialize<RepeatableQuestChangeRequest>(jsonText);
            case ItemEventActions.ADD_NOTE:
            case ItemEventActions.EDIT_NOTE:
            case ItemEventActions.DELETE_NOTE:
                return JsonSerializer.Deserialize<NoteActionRequest>(jsonText);
            case ItemEventActions.MOVE:
                return JsonSerializer.Deserialize<InventoryMoveRequestData>(jsonText);
            case ItemEventActions.REMOVE:
                return JsonSerializer.Deserialize<InventoryRemoveRequestData>(jsonText);
            case ItemEventActions.SPLIT:
                return JsonSerializer.Deserialize<InventorySplitRequestData>(jsonText);
            case ItemEventActions.MERGE:
                return JsonSerializer.Deserialize<InventoryMergeRequestData>(jsonText);
            case ItemEventActions.TRANSFER:
                return JsonSerializer.Deserialize<InventoryTransferRequestData>(jsonText);
            case ItemEventActions.SWAP:
                return JsonSerializer.Deserialize<InventorySwapRequestData>(jsonText);
            case ItemEventActions.FOLD:
                return JsonSerializer.Deserialize<InventoryFoldRequestData>(jsonText);
            case ItemEventActions.TOGGLE:
                return JsonSerializer.Deserialize<InventoryToggleRequestData>(jsonText);
            case ItemEventActions.TAG:
                return JsonSerializer.Deserialize<InventoryTagRequestData>(jsonText);
            case ItemEventActions.BIND:
            case ItemEventActions.UNBIND:
                return JsonSerializer.Deserialize<InventoryBindRequestData>(jsonText);
            case ItemEventActions.EXAMINE:
                return JsonSerializer.Deserialize<InventoryExamineRequestData>(jsonText);
            case ItemEventActions.READ_ENCYCLOPEDIA:
                return JsonSerializer.Deserialize<InventoryReadEncyclopediaRequestData>(jsonText);
            case ItemEventActions.APPLY_INVENTORY_CHANGES:
                return JsonSerializer.Deserialize<InventorySortRequestData>(jsonText);
            case ItemEventActions.CREATE_MAP_MARKER:
                return JsonSerializer.Deserialize<InventoryCreateMarkerRequestData>(jsonText);
            case ItemEventActions.DELETE_MAP_MARKER:
                return JsonSerializer.Deserialize<InventoryDeleteMarkerRequestData>(jsonText);
            case ItemEventActions.EDIT_MAP_MARKER:
                return JsonSerializer.Deserialize<InventoryEditMarkerRequestData>(jsonText);
            case ItemEventActions.OPEN_RANDOM_LOOT_CONTAINER:
                return JsonSerializer.Deserialize<OpenRandomLootContainerRequestData>(jsonText);
            case ItemEventActions.HIDEOUT_QTE_EVENT:
                return JsonSerializer.Deserialize<HandleQTEEventRequestData>(jsonText);
            case ItemEventActions.REDEEM_PROFILE_REWARD:
                return JsonSerializer.Deserialize<RedeemProfileRequestData>(jsonText);
            case ItemEventActions.SET_FAVORITE_ITEMS:
                return JsonSerializer.Deserialize<SetFavoriteItems>(jsonText);
            case ItemEventActions.QUEST_FAIL:
                return JsonSerializer.Deserialize<FailQuestRequestData>(jsonText);
            case ItemEventActions.PIN_LOCK:
                return JsonSerializer.Deserialize<PinOrLockItemRequest>(jsonText);
            default:
                throw new Exception(
                    $"Unhandled action type {value.Action}, make sure the BaseInteractionRequestDataConverter has the deserialization for this action handled."
                );
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseInteractionRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
