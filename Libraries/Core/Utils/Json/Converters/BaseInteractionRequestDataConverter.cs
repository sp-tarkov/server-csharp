using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Hideout;
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
        var value = JsonSerializer.Deserialize<BaseInteractionRequestData>(jsonText, options);
        return ConvertToCorrectType(value, jsonText, options);
    }

    private BaseInteractionRequestData? ConvertToCorrectType(BaseInteractionRequestData? value, string jsonText, JsonSerializerOptions options)
    {
        switch (value.Action)
        {
            case "CustomizationBuy":
                return JsonSerializer.Deserialize<BuyClothingRequestData>(jsonText, options);
            case "CustomizationSet":
                return JsonSerializer.Deserialize<CustomizationSetRequest>(jsonText, options);
            
            /////////////////////////////////////////// InventoryBaseActionRequestData
            
            case "AddToWishList":
                return JsonSerializer.Deserialize<AddToWishlistRequest>(jsonText, options);
            case "RemoveFromWishList":
                return JsonSerializer.Deserialize<RemoveFromWishlistRequest>(jsonText, options);
            case "ChangeWishlistItemCategory":
                return JsonSerializer.Deserialize<ChangeWishlistItemCategoryRequest>(jsonText, options);
            case "TradingConfirm":
                return JsonSerializer.Deserialize<ProcessBaseTradeRequestData>(jsonText, options);
            case "RagFairBuyOffer":
                return JsonSerializer.Deserialize<ProcessRagfairTradeRequestData>(jsonText, options);
            case "SellAllFromSavage":
                return JsonSerializer.Deserialize<SellScavItemsToFenceRequestData>(jsonText, options);
            case "Repair":
                return JsonSerializer.Deserialize<RepairActionDataRequest>(jsonText, options);
            case "TraderRepair":
                return JsonSerializer.Deserialize<TraderRepairActionDataRequest>(jsonText, options);
            case "RagFairAddOffer":
                return JsonSerializer.Deserialize<AddOfferRequestData>(jsonText, options);
            case "RagFairRemoveOffer":
                return JsonSerializer.Deserialize<RemoveOfferRequestData>(jsonText, options);
            case "RagFairRenewOffer":
                return JsonSerializer.Deserialize<ExtendOfferRequestData>(jsonText, options);
            case "QuestAccept":
                return JsonSerializer.Deserialize<AcceptQuestRequestData>(jsonText, options);
            case "QuestComplete":
                return JsonSerializer.Deserialize<CompleteQuestRequestData>(jsonText, options);
            case "QuestHandover":
                return JsonSerializer.Deserialize<HandoverQuestRequestData>(jsonText, options);
            case "RepeatableQuestChange":
                return JsonSerializer.Deserialize<RepeatableQuestChangeRequest>(jsonText, options);
            case "AddNote":
            case "EditNote":
            case "DeleteNote":
                return JsonSerializer.Deserialize<NoteActionData>(jsonText, options);
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
                return JsonSerializer.Deserialize<InventoryBindRequestData>(jsonText, options);
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
                // bitch
            
        }
    }

    public override void Write(Utf8JsonWriter writer, BaseInteractionRequestData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}
