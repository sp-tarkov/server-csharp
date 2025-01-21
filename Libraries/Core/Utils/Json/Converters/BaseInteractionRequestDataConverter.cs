using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Health;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.Insurance;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.Notes;
using Core.Models.Eft.Quests;
using Core.Models.Eft.Ragfair;
using Core.Models.Eft.Repair;
using Core.Models.Eft.Trade;
using Core.Models.Eft.Wishlist;
using Core.Models.Enums;

namespace Core.Utils.Json.Converters;

public class BaseInteractionRequestDataConverter : JsonConverter<BaseInteractionRequestData>
{
    public override BaseInteractionRequestData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var jsonText = jsonDocument.RootElement.GetRawText();
        var value = JsonSerializer.Deserialize<BaseInteractionRequestData>(jsonText);
        return ConvertToCorrectType(value, jsonText);
    }

    private BaseInteractionRequestData? ConvertToCorrectType(BaseInteractionRequestData? value, string jsonText)
    {
        switch (value.Action)
        {
            case "CustomizationBuy":
                return JsonSerializer.Deserialize<BuyClothingRequestData>(jsonText);
            case "CustomizationSet":
                return JsonSerializer.Deserialize<CustomizationSetRequest>(jsonText);
            case "Eat":
                return JsonSerializer.Deserialize<OffraidEatRequestData>(jsonText);
            case "Heal":
                return JsonSerializer.Deserialize<OffraidHealRequestData>(jsonText);
            case "RestoreHealth":
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
                return JsonSerializer.Deserialize<HideoutImproveAreaRequestData>(jsonText);
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
            case "Insure":
                return JsonSerializer.Deserialize<InsureRequestData>(jsonText);
            
            /////////////////////////////////////////// InventoryBaseActionRequestData
            
            case "AddToWishList":
                return JsonSerializer.Deserialize<AddToWishlistRequest>(jsonText);
            case "RemoveFromWishList":
                return JsonSerializer.Deserialize<RemoveFromWishlistRequest>(jsonText);
            case "ChangeWishlistItemCategory":
                return JsonSerializer.Deserialize<ChangeWishlistItemCategoryRequest>(jsonText);
            case "TradingConfirm":
                return JsonSerializer.Deserialize<ProcessBaseTradeRequestData>(jsonText);
            case "RagFairBuyOffer":
                return JsonSerializer.Deserialize<ProcessRagfairTradeRequestData>(jsonText);
            case "SellAllFromSavage":
                return JsonSerializer.Deserialize<SellScavItemsToFenceRequestData>(jsonText);
            case "Repair":
                return JsonSerializer.Deserialize<RepairActionDataRequest>(jsonText);
            case "TraderRepair":
                return JsonSerializer.Deserialize<TraderRepairActionDataRequest>(jsonText);
            case "RagFairAddOffer":
                return JsonSerializer.Deserialize<AddOfferRequestData>(jsonText);
            case "RagFairRemoveOffer":
                return JsonSerializer.Deserialize<RemoveOfferRequestData>(jsonText);
            case "RagFairRenewOffer":
                return JsonSerializer.Deserialize<ExtendOfferRequestData>(jsonText);
            case "QuestAccept":
                return JsonSerializer.Deserialize<AcceptQuestRequestData>(jsonText);
            case "QuestComplete":
                return JsonSerializer.Deserialize<CompleteQuestRequestData>(jsonText);
            case "QuestHandover":
                return JsonSerializer.Deserialize<HandoverQuestRequestData>(jsonText);
            case "RepeatableQuestChange":
                return JsonSerializer.Deserialize<RepeatableQuestChangeRequest>(jsonText);
            case "AddNote":
            case "EditNote":
            case "DeleteNote":
                return JsonSerializer.Deserialize<NoteActionData>(jsonText);
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
                return JsonSerializer.Deserialize<InventoryBindRequestData>(jsonText);
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
                throw new Exception($"Unhandled action type {value.Action}, make sure the BaseInteractionRequestDataConverter has the deserialization for this action handled.");
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseInteractionRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
