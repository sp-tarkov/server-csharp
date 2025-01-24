using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Utils;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;
using SptCommon.Annotations;

namespace Core.Controllers;

[Injectable]
public class CustomizationController(
    ISptLogger<CustomizationController> _logger,
    EventOutputHolder _eventOutputHolder,
    DatabaseService _databaseService,
    SaveServer _saveServer,
    LocalisationService _localisationService,
    ProfileHelper _profileHelper,
    ICloner _cloner
)
{
    protected string _lowerParentClothingId = "5cd944d01388ce000a659df9";
    protected string _upperParentClothingId = "5cd944ca1388ce03a44dc2a4";

    /// <summary>
    ///     Get purchasable clothing items from trader that match players side (usec/bear)
    /// </summary>
    /// <param name="traderId">trader to look up clothing for</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>Suit array</returns>
    public List<Suit> GetTraderSuits(string traderId, string sessionId)
    {
        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var clothing = _databaseService.GetCustomization();
        var suits = _databaseService.GetTrader(traderId).Suits;

        var matchingSuits = suits?.Where(s => clothing.ContainsKey(s.SuiteId!)).ToList();
        matchingSuits = matchingSuits?.Where(
                s => clothing[s.SuiteId ?? string.Empty]
                         ?.Properties?.Side?
                         .Contains(pmcData?.Info?.Side ?? string.Empty) ??
                     false
            )
            .ToList();

        if (matchingSuits == null)
        {
            throw new Exception(_localisationService.GetText("customisation-unable_to_get_trader_suits", traderId));
        }

        return matchingSuits;
    }

    /// <summary>
    ///     Handle CustomizationBuy event
    ///     Purchase/unlock a clothing item from a trader
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="buyClothingRequest">Request object</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse BuyCustomisation(
        PmcData pmcData,
        BuyClothingRequestData buyClothingRequest,
        string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var traderOffer = GetTraderClothingOffer(sessionId, buyClothingRequest.Offer);
        if (traderOffer is null)
        {
            _logger.Error(
                _localisationService.GetText("customisation-unable_to_find_suit_by_id", buyClothingRequest.Offer)
            );
            return output;
        }

        var suitId = traderOffer.SuiteId;
        if (OutfitAlreadyPurchased(suitId ?? string.Empty, sessionId))
        {
            var suitDetails = _databaseService.GetCustomization()!.GetValueOrDefault(suitId);
            _logger.Error(
                _localisationService.GetText(
                    "customisation-item_already_purchased",
                    new
                    {
                        itemId = suitDetails?.Id,
                        itemName = suitDetails?.Name
                    }
                )
            );

            return output;
        }

        // Charge player for buying item
        PayForClothingItems(sessionId, pmcData, buyClothingRequest.Items, output);

        var profile = _saveServer.GetProfile(sessionId);

        //TODO: Merge with function _profileHelper.addHideoutCustomisationUnlock
        var rewardToStore = new CustomisationStorage
        {
            Id = suitId,
            Source = CustomisationSource.UNLOCKED_IN_GAME,
            Type = CustomisationType.SUITE
        };

        profile.CustomisationUnlocks.Add(rewardToStore);

        return output;
    }

    private bool OutfitAlreadyPurchased(object suitId, string sessionId)
    {
        return (_saveServer.GetProfile(sessionId).Suits ?? []).Contains(suitId);
    }

    private Suit? GetTraderClothingOffer(string sessionId, string? offerId)
    {
        var foundSuit = GetAllTraderSuits(sessionId).FirstOrDefault(s => s.Id == offerId);
        if (foundSuit == null)
        {
            _logger.Error(_localisationService.GetText("customisation-unable_to_find_suit_with_id", offerId));
        }

        return foundSuit;
    }

    /// <summary>
    ///     Update output object and player profile with purchase details
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemsToPayForClothingWith">Clothing purchased</param>
    /// <param name="output">Client response</param>
    private void PayForClothingItems(string sessionId, PmcData pmcData,
        List<PaymentItemForClothing>? itemsToPayForClothingWith,
        ItemEventRouterResponse output)
    {
        foreach (var inventoryItemToProcess in itemsToPayForClothingWith ?? [])
            PayForClothingItem(sessionId, pmcData, inventoryItemToProcess, output);
    }

    /// <summary>
    ///     Update output object and player profile with purchase details for single piece of clothing
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="paymentItemDetails">Payment details</param>
    /// <param name="output">Client response</param>
    private void PayForClothingItem(string sessionId, PmcData pmcData, PaymentItemForClothing? paymentItemDetails,
        ItemEventRouterResponse output)
    {
        var inventoryItem = pmcData.Inventory?.Items?.FirstOrDefault(x => x.Id == paymentItemDetails?.Id);
        if (inventoryItem == null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "customisation-unable_to_find_clothing_item_in_inventory",
                    paymentItemDetails?.Id
                )
            );
            return;
        }

        if (paymentItemDetails?.Del != null)
        {
            output.ProfileChanges[sessionId]
                .Items?.DeletedItems.Add(
                    new Item
                    {
                        Id = inventoryItem.Id,
                        Template = inventoryItem.Template,
                        ParentId = inventoryItem.ParentId,
                        SlotId = inventoryItem.SlotId,
                        Location = (ItemLocation?)inventoryItem.Location,
                        Upd = inventoryItem.Upd
                    }
                );
        }

        pmcData.Inventory?.Items?.Remove(inventoryItem);

        inventoryItem.Upd ??= new Upd { StackObjectsCount = 1 };

        if (inventoryItem.Upd?.StackObjectsCount == null)
        {
            if (inventoryItem.Upd != null)
            {
                inventoryItem.Upd.StackObjectsCount = 1;
            }
        }

        if (inventoryItem.Upd?.StackObjectsCount == paymentItemDetails?.Count)
        {
            output.ProfileChanges[sessionId]
                .Items?.DeletedItems.Add(
                    new Item
                    {
                        Id = inventoryItem.Id,
                        Template = inventoryItem.Template,
                        ParentId = inventoryItem.ParentId,
                        SlotId = inventoryItem.SlotId,
                        Location = (ItemLocation?)inventoryItem.Location,
                        Upd = inventoryItem.Upd
                    }
                );

            pmcData.Inventory?.Items?.Remove(inventoryItem);
            return;
        }

        if (!(inventoryItem.Upd?.StackObjectsCount > paymentItemDetails?.Count))
        {
            return;
        }

        inventoryItem.Upd.StackObjectsCount -= paymentItemDetails.Count;
        output.ProfileChanges[sessionId]
            .Items?.ChangedItems?.Add(
                new Item
                {
                    Id = inventoryItem.Id,
                    Template = inventoryItem.Template,
                    ParentId = inventoryItem.ParentId,
                    SlotId = inventoryItem.SlotId,
                    Location = (ItemLocation?)inventoryItem.Location,
                    Upd = inventoryItem.Upd
                }
            );
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private List<Suit> GetAllTraderSuits(string sessionId)
    {
        var traders = _databaseService.GetTraders();
        var result = new List<Suit>();

        foreach (var trader in traders)
            if (trader.Value.Base?.CustomizationSeller is not null && trader.Value.Base.CustomizationSeller.Value)
            {
                result.AddRange(GetTraderSuits(trader.Key, sessionId));
            }

        return result;
    }

    /// <summary>
    ///     Handle client/hideout/customization/offer/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public HideoutCustomisation GetHideoutCustomisation(string sessionId, EmptyRequestData info)
    {
        return _databaseService.GetHideout().Customisation!;
    }

    /// <summary>
    ///     Handle client/customization/storage
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public List<CustomisationStorage> GetCustomisationStorage(
        string sessionId,
        EmptyRequestData info)
    {
        var customisationResultsClone = _cloner.Clone(_databaseService.GetTemplates().CustomisationStorage);

        var profile = _profileHelper.GetFullProfile(sessionId);
        if (profile is null)
        {
            return customisationResultsClone!;
        }

        customisationResultsClone!.AddRange(profile.CustomisationUnlocks ?? []);

        return customisationResultsClone;
    }

    /// <summary>
    ///     Handle CustomizationSet event
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <param name="pmcData"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SetCustomisation(string sessionId, CustomizationSetRequest request, PmcData pmcData)
    {
        foreach (var customisation in request.Customizations)
            switch (customisation.Type)
            {
                case "dogTag":
                    pmcData.Customization!.DogTag = customisation.Id;
                    break;
                case "suite":
                    ApplyClothingItemToProfile(customisation, pmcData);
                    break;
                default:
                    _logger.Error($"Unhandled customisation type: {customisation.Type}");
                    break;
            }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    ///     Applies a purchased suit to the players doll
    /// </summary>
    /// <param name="customisation">Suit to apply to profile</param>
    /// <param name="pmcData">Profile to update</param>
    private void ApplyClothingItemToProfile(CustomizationSetOption customisation, PmcData pmcData)
    {
        var dbSuit = _databaseService.GetCustomization()[customisation.Id!];

        if (dbSuit is null)
        {
            _logger.Error(
                $"Unable to find suit customisation id: {customisation.Id}, cannot apply clothing to player profile: {pmcData.Id}"
            );
            return;
        }

        // Body
        if (dbSuit.Parent == _upperParentClothingId)
        {
            pmcData.Customization.Body = dbSuit.Properties.Body;
            pmcData.Customization.Hands = dbSuit.Properties.Hands;

            return;
        }

        // Feet
        if (dbSuit.Parent == _lowerParentClothingId)
        {
            pmcData.Customization.Feet = dbSuit.Properties.Feet;
        }
    }
}
