using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Utils;
using Core.Routers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Controllers;

[Injectable]
public class InventoryController(
    ISptLogger<InventoryController> _logger,
    HashUtil _hashUtil,
    RandomUtil _randomUtil,
    HttpResponseUtil _httpResponseUtil,
    PresetHelper _presetHelper,
    InventoryHelper _inventoryHelper,
    QuestHelper _questHelper,
    HideoutHelper _hideoutHelper,
    ProfileHelper _profileHelper,
    PaymentHelper _paymentHelper,
    TraderHelper _traderHelper,
    DatabaseService _databaseService,
    FenceService _fenceService,
    RagfairOfferService _ragfairOfferService,
    MapMarkerService _mapMarkerService,
    LocalisationService _localisationService,
    PlayerService _playerService,
    LootGenerator _lootGenerator,
    EventOutputHolder _eventOutputHolder,
    ICloner _cloner
)
{
    public void MoveItem(PmcData pmcData, InventoryMoveRequestData moveRequest, string sessionID, ItemEventRouterResponse output)
    {
        if (output.Warnings?.Count > 0)
        {
            return;
        }

        // Changes made to result apply to character inventory
        var ownerInventoryItems = _inventoryHelper.GetOwnerInventoryItems(moveRequest, moveRequest.Item, sessionID);
        if (ownerInventoryItems.SameInventory.GetValueOrDefault(false))
        {
            // Don't move items from trader to profile, this can happen when editing a traders preset weapons
            if (moveRequest.FromOwner?.Type == "Trader" && !ownerInventoryItems.IsMail.GetValueOrDefault(false))
            {
                AppendTraderExploitErrorResponse(output);
                return;
            }

            // Check for item in inventory before allowing internal transfer
            var originalItemLocation = ownerInventoryItems.From?.FirstOrDefault((item) => item.Id == moveRequest.Item);
            if (originalItemLocation is null)
            {
                // Internal item move but item never existed, possible dupe glitch
                AppendTraderExploitErrorResponse(output);
                return;
            }

            var originalLocationSlotId = originalItemLocation.SlotId;

            var moveResult = _inventoryHelper.MoveItemInternal(pmcData, ownerInventoryItems.From ?? [], moveRequest, out var errorMessage);
            if (!moveResult)
            {
                _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
                return;
            }

            // Item is moving into or out of place of fame dogtag slot
            if (moveRequest.To?.Container != null && (moveRequest.To.Container.StartsWith("dogtag") || originalLocationSlotId!.StartsWith("dogtag")))
            {
                _hideoutHelper.ApplyPlaceOfFameDogtagBonus(pmcData);
            }
        }
        else
        {
            _inventoryHelper.MoveItemToProfile(ownerInventoryItems.From ?? [], ownerInventoryItems.To ?? [], moveRequest);
        }
    }

    private void AppendTraderExploitErrorResponse(ItemEventRouterResponse output)
    {
        _httpResponseUtil.AppendErrorToOutput(
            output,
            _localisationService.GetText("inventory-edit_trader_item"),
            (BackendErrorCodes)228
        );
    }
}
