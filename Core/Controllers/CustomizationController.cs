using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Customization;
using Core.Models.Eft.Hideout;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;
using Product = Core.Models.Eft.ItemEvent.Product;

namespace Core.Controllers;

[Injectable]
public class CustomizationController
{
    protected ILogger _logger;
    protected EventOutputHolder _eventOutputHolder;
    protected DatabaseService _databaseService;
    protected SaveServer _saveServer;
    protected LocalisationService _localisationService;
    protected ProfileHelper _profileHelper;
    protected ICloner _cloner;

    public CustomizationController
    (
        ILogger logger,
        EventOutputHolder eventOutputHolder,
        DatabaseService databaseService,
        SaveServer saveServer,
        LocalisationService localisationService,
        ProfileHelper profileHelper,
        ICloner cloner
    )
    {
        _logger = logger;
        _eventOutputHolder = eventOutputHolder;
        _databaseService = databaseService;
        _saveServer = saveServer;
        _localisationService = localisationService;
        _profileHelper = profileHelper;
        _cloner = cloner;
    }

    /// <summary>
    /// Get purchasable clothing items from trader that match players side (usec/bear)
    /// </summary>
    /// <param name="traderId">trader to look up clothing for</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>Suit array</returns>
    public List<Suit> GetTraderSuits(string traderId, string sessionId)
    {
        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var clothing = _databaseService.GetCustomization();
        var suits = _databaseService.GetTrader(traderId)?.Suits;

        var matchingSuits = suits.Where(s => clothing.ContainsKey(s?.SuiteId)).ToList();
        matchingSuits = matchingSuits?.Where(s => clothing[s?.SuiteId].Properties.Side.Contains(pmcData?.Info?.Side)).ToList();

        if (matchingSuits == null)
            throw new Exception(_localisationService.GetText("customisation-unable_to_get_trader_suits", traderId));

        return matchingSuits;
    }

    /// <summary>
    /// Handle CustomizationBuy event
    /// Purchase/unlock a clothing item from a trader
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="buyClothingRequest">Request object</param>
    /// <param name="sessionId">Session id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse BuyClothing(
        PmcData pmcData,
        BuyClothingRequestData buyClothingRequest,
        string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var traderOffer = GetTraderClothingOffer(sessionId, buyClothingRequest?.Offer);
        if (traderOffer == null)
        {
            _logger.Error(_localisationService.GetText("customisation-unable_to_find_suit_by_id", buyClothingRequest.Offer));
            return output;
        }

        var suitId = traderOffer?.SuiteId;
        if (OutfitAlreadyPurchased(suitId, sessionId))
        {
            var suitDetails = _databaseService.GetCustomization()[suitId];
            _logger.Error(_localisationService.GetText("customisation-item_already_purchased", new
            {
                ItemId = suitDetails?.Id,
                ItemName = suitDetails?.Name,
            }));
        }

        return output;
    }

    private bool OutfitAlreadyPurchased(object suitId, string sessionId)
    {
        return _saveServer.GetProfile(sessionId).Suits.Contains(suitId);
    }

    private Suit GetTraderClothingOffer(string sessionId, string? offerId)
    {
        var foundSuit = GetAllTraderSuits(sessionId).FirstOrDefault(s => s?.Id == offerId);
        if (foundSuit == null)
            throw new Exception(_localisationService.GetText("customisation-unable_to_find_suit_with_id", offerId));

        return foundSuit;
    }

    /// <summary>
    /// Update output object and player profile with purchase details
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="itemsToPayForClothingWith">Clothing purchased</param>
    /// <param name="output">Client response</param>
    private void PayForClothingItems(string sessionId, PmcData pmcData, List<PaymentItemForClothing> itemsToPayForClothingWith,
        ItemEventRouterResponse output)
    {
        foreach (var inventoryItemToProcess in itemsToPayForClothingWith)
        {
            PayForClothingItem(sessionId, pmcData, inventoryItemToProcess, output);
        }
    }

    /// <summary>
    /// Update output object and player profile with purchase details for single piece of clothing
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="paymentItemDetails">Payment details</param>
    /// <param name="output">Client response</param>
    private void PayForClothingItem(string sessionId, PmcData pmcData, PaymentItemForClothing paymentItemDetails, ItemEventRouterResponse output)
    {
        var inventoryItem = pmcData?.Inventory?.Items?.FirstOrDefault(x => x?.Id == paymentItemDetails?.Id);
        if (inventoryItem == null)
        {
            _logger.Error(_localisationService.GetText("customisation-unable_to_find_clothing_item_in_inventory", paymentItemDetails.Id));
            return;
        }

        if (paymentItemDetails?.Del != null)
        {
            output?.ProfileChanges[sessionId]?.Items?.DeletedItems?.Add(new Product
            {
                Id = inventoryItem?.Id,
                Template = inventoryItem?.Template,
                ParentId = inventoryItem?.ParentId,
                SlotId = inventoryItem?.SlotId,
                Location = (ItemLocation)inventoryItem?.Location,
                Upd = inventoryItem?.Upd
            });

            pmcData?.Inventory?.Items?.Remove(inventoryItem);
        }

        if (inventoryItem?.Upd == null)
            inventoryItem.Upd = new() { StackObjectsCount = 1 };
        
        if (inventoryItem?.Upd?.StackObjectsCount == null)
            inventoryItem.Upd.StackObjectsCount = 1;

        if (inventoryItem?.Upd?.StackObjectsCount == paymentItemDetails?.Count)
        {
            output?.ProfileChanges[sessionId]?.Items?.DeletedItems?.Add(new Product
            {
                Id = inventoryItem?.Id,
                Template = inventoryItem?.Template,
                ParentId = inventoryItem?.ParentId,
                SlotId = inventoryItem?.SlotId,
                Location = (ItemLocation)inventoryItem?.Location,
                Upd = inventoryItem?.Upd
            });
            
            pmcData?.Inventory?.Items?.Remove(inventoryItem);
            return;
        }

        if (inventoryItem.Upd.StackObjectsCount > paymentItemDetails?.Count)
        {
            inventoryItem.Upd.StackObjectsCount -= paymentItemDetails?.Count;
            output?.ProfileChanges[sessionId]?.Items?.ChangedItems.Add(new Product
            {
                Id = inventoryItem?.Id,
                Template = inventoryItem?.Template,
                ParentId = inventoryItem?.ParentId,
                SlotId = inventoryItem?.SlotId,
                Location = (ItemLocation)inventoryItem?.Location,
                Upd = inventoryItem?.Upd
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private List<Suit> GetAllTraderSuits(string sessionId)
    {
        var traders = _databaseService.GetTraders();
        var result = new List<Suit>();

        foreach (var trader in traders)
        {
            if (trader.Value.Base.CustomizationSeller.Value)
                result.AddRange(GetTraderSuits(trader.Key, sessionId));
        }
        
        return result;
    }

    /// <summary>
    /// Handle client/hideout/customization/offer/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public HideoutCustomisation GetHideoutCustomisation(string sessionId, EmptyRequestData info)
    {
        return _databaseService.GetHideout().Customisation;
    }

    /// <summary>
    /// Handle client/customization/storage
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
        switch (GetGameEdition(profile))
        {
            case GameEditions.EDGE_OF_DARKNESS:
                customisationResultsClone.Add(new ()
                {
                    Id = "6746fd09bafff85008048838",
                    Source = "default",
                    Type = "dogTag"
                });
                customisationResultsClone.Add(new ()
                {
                    Id = "67471938bafff850080488b7",
                    Source = "default",
                    Type = "dogTag"
                });
                break;
            case GameEditions.UNHEARD:
                customisationResultsClone.Add(new ()
                {
                    Id = "6746fd09bafff85008048838",
                    Source = "default",
                    Type = "dogTag"
                });
                customisationResultsClone.Add(new ()
                {
                    Id = "67471938bafff850080488b7",
                    Source = "default",
                    Type = "dogTag"
                });
                customisationResultsClone.Add(new ()
                {
                    Id = "67471928d17d6431550563b5",
                    Source = "default",
                    Type = "dogTag"
                });
                customisationResultsClone.Add(new ()
                {
                    Id = "6747193f170146228c0d2226",
                    Source = "default",
                    Type = "dogTag"
                });
                break;
            default:
                throw new Exception($"Unknown game edition given from profile {profile}");
        }

        var prestigeLevel = profile?.CharacterData?.PmcData?.Info?.PrestigeLevel;
        if (prestigeLevel != null)
        {
            if (prestigeLevel >= 1)
            {
                customisationResultsClone.Add(new ()
                {
                    Id = "674dbf593bee1152d407f005",
                    Source = "default",
                    Type = "dogTag"
                });
            }
            if (prestigeLevel >= 2)
            {
                customisationResultsClone.Add(new ()
                {
                    Id = "675dcfea7ae1a8792107ca99",
                    Source = "default",
                    Type = "dogTag"
                });
            }
        }
        
        return customisationResultsClone;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <returns></returns>
    private string GetGameEdition(SptProfile profile)
    {
        var edition = profile?.CharacterData?.PmcData?.Info?.GameVersion;
        if (edition == null)
        {
            var launcherEdition = profile?.ProfileInfo?.Edition;
            switch (launcherEdition.ToLower())
            {
                case "edge of darkness":
                    return GameEditions.EDGE_OF_DARKNESS;
                case "unheard":
                    return GameEditions.UNHEARD;
                default:
                    return GameEditions.STANDARD;
            }
        }

        return edition;
    }

    /// <summary>
    /// Handle CustomizationSet event
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <param name="pmcData"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SetClothing(string sessionId, CustomizationSetRequest request, PmcData pmcData)
    {
        foreach (var customisation in request?.Customizations)
        {
            switch (customisation.Id)
            {
                case "dogTag":
                    pmcData.Customization.DogTag = customisation?.Id;
                    break;
                case "suite":
                    ApplyClothingItemToProfile(customisation, pmcData);
                    break;
                default:
                    _logger.Error($"Unhandled customisation type: {customisation?.Type}");
                    break;
            }
        }

        return _eventOutputHolder.GetOutput(sessionId);
    }

    /// <summary>
    /// Applies a purchased suit to the players doll
    /// </summary>
    /// <param name="customization">Suit to apply to profile</param>
    /// <param name="pmcData">Profile to update</param>
    private void ApplyClothingItemToProfile(CustomizationSetOption customisation, PmcData pmcData)
    {
        var dbSuit = _databaseService.GetCustomization()[customisation?.Id];
        if (dbSuit == null)
        {
            _logger.Error($"Unable to find suit customisation id: {customisation?.Id}, cannot apply clothing to player profile: {pmcData.Id}");
            return;
        }

        if (dbSuit.Parent == "5cd944d01388ce000a659df9")
        {
            pmcData.Customization.Body = dbSuit?.Properties?.Body;
            pmcData.Customization.Hands = dbSuit?.Properties?.Hands;

            return;
        }

        if (dbSuit.Parent == "5cd944ca1388ce03a44dc2a4")
        {
            pmcData.Customization.Feet = dbSuit?.Properties?.Feet;
        }
    }
}
