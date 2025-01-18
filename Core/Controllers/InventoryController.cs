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
public class InventoryController
{
    protected ISptLogger<InventoryController> _logger;
    protected HashUtil _hashUtil;
    protected RandomUtil _randomUtil;
    protected HttpResponseUtil _httpResponseUtil;
    protected PresetHelper _presetHelper;
    protected InventoryHelper _inventoryHelper;
    protected QuestHelper _questHelper;
    protected HideoutHelper _hideoutHelper;
    protected ProfileHelper _profileHelper;
    protected PaymentHelper _paymentHelper;
    protected TraderHelper _traderHelper;
    protected DatabaseService _databaseService;
    protected FenceService _fenceService;
    protected RagfairOfferService _ragfairOfferService;
    protected MapMarkerService _mapMarkerService;
    protected LocalisationService _localisationService;
    protected PlayerService _playerService;
    protected LootGenerator _lootGenerator;
    protected EventOutputHolder _eventOutputHolder;

    public InventoryController(
        ISptLogger<InventoryController> logger,
        HashUtil hashUtil,
        RandomUtil randomUtil,
        HttpResponseUtil httpResponseUtil,
        PresetHelper presetHelper,
        InventoryHelper inventoryHelper,
        QuestHelper questHelper,
        HideoutHelper hideoutHelper,
        ProfileHelper profileHelper,
        PaymentHelper paymentHelper,
        TraderHelper traderHelper,
        DatabaseService databaseService,
        FenceService fenceService,
        RagfairOfferService ragfairOfferService,
        MapMarkerService mapMarkerService,
        LocalisationService localisationService,
        PlayerService playerService,
        LootGenerator lootGenerator,
        EventOutputHolder eventOutputHolder,

        ICloner cloner
        )
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _randomUtil = randomUtil;
        _httpResponseUtil = httpResponseUtil;
        _presetHelper = presetHelper;
        _inventoryHelper = inventoryHelper;
        _questHelper = questHelper;
        _hideoutHelper = hideoutHelper;
        _profileHelper = profileHelper;
        _paymentHelper = paymentHelper;
        _traderHelper = traderHelper;
        _databaseService = databaseService;
        _fenceService = fenceService;
        _ragfairOfferService = ragfairOfferService;
        _mapMarkerService = mapMarkerService;
        _localisationService = localisationService;
        _playerService = playerService;
        _lootGenerator = lootGenerator;
        _eventOutputHolder = eventOutputHolder;
    }

    public void MoveItem(PmcData pmcData, InventoryMoveRequestData moveRequest, string sessionID, ItemEventRouterResponse output)
    {
        if (output.Warnings.Count > 0)
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
            var originalItemLocation = ownerInventoryItems.From.FirstOrDefault((item) => item.Id == moveRequest.Item);
            if (originalItemLocation is null)
            {
                // Internal item move but item never existed, possible dupe glitch
                AppendTraderExploitErrorResponse(output);
                return;
            }

            var originalLocationSlotId = originalItemLocation?.SlotId;

            var moveResult = _inventoryHelper.MoveItemInternal(pmcData, ownerInventoryItems.From, moveRequest, out var errorMessage);
            if (!moveResult)
            {
                _httpResponseUtil.AppendErrorToOutput(output, errorMessage);
                return;
            }

            // Item is moving into or out of place of fame dogtag slot
            if (moveRequest.To.Container.StartsWith("dogtag")  || originalLocationSlotId.StartsWith("dogtag"))
            {
                _hideoutHelper.ApplyPlaceOfFameDogtagBonus(pmcData);
            }
        }
        else
        {
            _inventoryHelper.MoveItemToProfile(ownerInventoryItems.From, ownerInventoryItems.To, moveRequest);
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
