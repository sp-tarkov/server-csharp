using SptCommon.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Helpers;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Routers;
using Core.Utils;
using Core.Models.Spt.Config;
using Core.Models.Common;
using Core.Models.Eft.Trade;
using Core.Generators;

namespace Core.Controllers;

[Injectable]
public class RagfairController
{
    private readonly ISptLogger<RagfairController> _logger;
    private readonly TimeUtil _timeUtil;
    private readonly JsonUtil _jsonUtil;
    private readonly HttpResponseUtil _httpResponseUtil;
    private readonly EventOutputHolder _eventOutputHolder;
    private readonly RagfairServer _ragfairServer;
    private readonly ItemHelper _itemHelper;
    private readonly InventoryHelper _inventoryHelper;
    private readonly RagfairSellHelper _ragfairSellHelper;
    private readonly HandbookHelper _handbookHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly PaymentHelper _paymentHelper;
    private readonly RagfairHelper _ragfairHelper;
    private readonly RagfairSortHelper _ragfairSortHelper;
    private readonly RagfairOfferHelper _ragfairOfferHelper;
    private readonly TraderHelper _traderHelper;
    private readonly DatabaseService _databaseService;
    private readonly LocalisationService _localisationService;
    private readonly RagfairTaxService _ragfairTaxService;
    private readonly RagfairOfferService _ragfairOfferService;
    private readonly PaymentService _paymentService;
    private readonly RagfairPriceService _ragfairPriceService;
    private readonly RagfairOfferGenerator _ragfairOfferGenerator;
    private readonly ConfigServer _configServer;

    private readonly RagfairConfig _ragfairConfig;

    public RagfairController(
        ISptLogger<RagfairController> logger,
        TimeUtil timeUtil,
        JsonUtil jsonUtil,
        HttpResponseUtil httpResponseUtil,
        EventOutputHolder eventOutputHolder,
        RagfairServer ragfairServer,
        ItemHelper itemHelper,
        InventoryHelper inventoryHelper,
        RagfairSellHelper ragfairSellHelper,
        HandbookHelper handbookHelper,
        ProfileHelper profileHelper,
        PaymentHelper paymentHelper,
        RagfairHelper ragfairHelper,
        RagfairSortHelper ragfairSortHelper,
        RagfairOfferHelper ragfairOfferHelper,
        TraderHelper traderHelper,
        DatabaseService databaseService,
        LocalisationService localisationService,
        RagfairTaxService ragfairTaxService,
        RagfairOfferService ragfairOfferService,
        PaymentService paymentService,
        RagfairPriceService ragfairPriceService,
        RagfairOfferGenerator ragfairOfferGenerator,
        ConfigServer configServer
        )
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _jsonUtil = jsonUtil;
        _httpResponseUtil = httpResponseUtil;
        _eventOutputHolder = eventOutputHolder;
        _ragfairServer = ragfairServer;
        _itemHelper = itemHelper;
        _inventoryHelper = inventoryHelper;
        _ragfairSellHelper = ragfairSellHelper;
        _handbookHelper = handbookHelper;
        _profileHelper = profileHelper;
        _paymentHelper = paymentHelper;
        _ragfairHelper = ragfairHelper;
        _ragfairSortHelper = ragfairSortHelper;
        _ragfairOfferHelper = ragfairOfferHelper;
        _traderHelper = traderHelper;
        _databaseService = databaseService;
        _localisationService = localisationService;
        _ragfairTaxService = ragfairTaxService;
        _ragfairOfferService = ragfairOfferService;
        _paymentService = paymentService;
        _ragfairPriceService = ragfairPriceService;
        _ragfairOfferGenerator = ragfairOfferGenerator;
        _configServer = configServer;

        _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
    }

    /**
     * Check all profiles and sell player offers / send player money for listing if it sold
     */
    public void Update()
    {
        foreach (var (sessionId, profile) in _profileHelper.GetProfiles()) {
            // Check profile is capable of creating offers
            var pmcProfile = profile.CharacterData.PmcData;
            if (
                pmcProfile.RagfairInfo is not null && pmcProfile.Info.Level >= _databaseService.GetGlobals().Configuration.RagFair.MinUserLevel
            )
            {
                _ragfairOfferHelper.ProcessOffersOnProfile(sessionId);
            }
        }
    }

    /**
     * Handles client/ragfair/find
     * Returns flea offers that match required search parameters
     * @param sessionID Player id
     * @param searchRequest Search request data
     * @returns IGetOffersResult
     */
    public GetOffersResult GetOffers(string sessionID, SearchRequestData searchRequest)
    {
        var profile = _profileHelper.GetFullProfile(sessionID);

        var itemsToAdd = _ragfairHelper.FilterCategories(sessionID, searchRequest);
        var traderAssorts = _ragfairHelper.GetDisplayableAssorts(sessionID);
        var result = new GetOffersResult{
        Offers = new List<RagfairOffer>(),
            OffersCount = searchRequest.Limit,
            SelectedCategory = searchRequest.HandbookId,
        };

        result.Offers = GetOffersForSearchType(searchRequest, itemsToAdd, traderAssorts, profile.CharacterData.PmcData);

        // Client requested a category refresh
        if (searchRequest.UpdateOfferCount is not null)
        {
            result.Categories = GetSpecificCategories(profile.CharacterData.PmcData, searchRequest, result.Offers);
        }

        AddIndexValueToOffers(result.Offers);

        // Sort offers
        result.Offers = _ragfairSortHelper.SortOffers(
            result.Offers,
            searchRequest.SortType.Value,
            searchRequest.SortDirection.Value);

        // Match offers with quests and lock unfinished quests - get offers from traders
        foreach (var traderOffer in result.Offers.Where(offer => _ragfairOfferHelper.OfferIsFromTrader(offer)))
        {
            // For the items, check the barter schemes. The method getDisplayableAssorts sets a flag sptQuestLocked
            // to true if the quest is not completed yet
            if (_ragfairOfferHelper.TraderOfferItemQuestLocked(traderOffer, traderAssorts))
            {
                traderOffer.Locked = true;
            }

            // Update offers BuyRestrictionCurrent/BuyRestrictionMax values
            SetTraderOfferPurchaseLimits(traderOffer, profile);
            SetTraderOfferStackSize(traderOffer);
        }

        result.OffersCount = result.Offers.Count;

        // Handle paging before returning results only if searching for general items, not preset items
        if (searchRequest.BuildCount == 0)
        {
            var start = searchRequest.Page * searchRequest.Limit;
            var end = (int)Math.Min((double)((searchRequest.Page + 1) * searchRequest.Limit), result.Offers.Count);
            result.Offers = result.Offers.Slice(start.Value, end);
        }
        return result;
    }

    /**
     * Adjust ragfair offer stack count to match same value as traders assort stack count
     * @param offer Flea offer to adjust stack size of
     */
    private void SetTraderOfferStackSize(RagfairOffer offer)
    {
        var firstItem = offer.Items[0];
        var traderAssorts = _traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;

        var assortPurchased = traderAssorts.FirstOrDefault(x => x.Id == offer.Items.First().Id);
        if (assortPurchased is null)
        {
            _logger.Warning(
                _localisationService.GetText("ragfair-unable_to_adjust_stack_count_assort_not_found", new {
                offerId = offer.Items.First().Id,
                traderId = offer.User.Id,
            }));

            return;
        }

        firstItem.Upd.StackObjectsCount = assortPurchased.Upd.StackObjectsCount;
    }

    /**
     * Update a trader flea offer with buy restrictions stored in the traders assort
     * @param offer Flea offer to update
     * @param fullProfile Players full profile
     */
    private void SetTraderOfferPurchaseLimits(RagfairOffer offer, SptProfile fullProfile)
    {
        // No trader found, create a blank record for them
        fullProfile.TraderPurchases[offer.User.Id] ??= new();

        var traderAssorts = _traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;
        var assortId = offer.Items.First().Id;
        var assortData = traderAssorts.FirstOrDefault((item) => item.Id == assortId);

        // Use value stored in profile, otherwise use value directly from in-memory trader assort data
        offer.BuyRestrictionCurrent = fullProfile.TraderPurchases[offer.User.Id][assortId] is not null
            ? fullProfile.TraderPurchases[offer.User.Id][assortId].PurchaseCount
            : assortData.Upd.BuyRestrictionCurrent;

        offer.BuyRestrictionMax = assortData.Upd.BuyRestrictionMax;
    }

    /**
     * Add index to all offers passed in (0-indexed)
     * @param offers Offers to add index value to
     */
    private void AddIndexValueToOffers(List<RagfairOffer> offers)
    {
        var counter = 0;

        foreach (var offer in offers) {
            offer.InternalId = ++counter;
        }
    }

    /**
     * Get categories for the type of search being performed, linked/required/all
     * @param searchRequest Client search request data
     * @param offers Ragfair offers to get categories for
     * @returns record with templates + counts
     */
    private Dictionary<string, int>? GetSpecificCategories(PmcData pmcProfile, SearchRequestData searchRequest, List<RagfairOffer> offers)
    {
        // Linked/required search categories
        var playerHasFleaUnlocked =
        pmcProfile.Info.Level >= _databaseService.GetGlobals().Configuration.RagFair.MinUserLevel;
        List<RagfairOffer> offerPool = [];
        if (IsLinkedSearch(searchRequest) || IsRequiredSearch(searchRequest))
        {
            offerPool = offers;
        }
        else if (!(IsLinkedSearch(searchRequest) || IsRequiredSearch(searchRequest)))
        {
            // Get all categories
            offerPool = _ragfairOfferService.GetOffers();
        }
        else
        {
            _logger.Error(_localisationService.GetText("ragfair-unable_to_get_categories"));
            _logger.Debug(_jsonUtil.Serialize(searchRequest));
            return new Dictionary<string, int>();
        }

        return _ragfairServer.GetAllActiveCategories(playerHasFleaUnlocked, searchRequest, offerPool);
    }

    /**
     * Is the flea search being performed a 'linked' search type
     * @param info Search request
     * @returns True if it is a 'linked' search type
     */
    private bool IsLinkedSearch(SearchRequestData searchRequest)
    {
        return searchRequest.LinkedSearchId != "";
    }

    /**
     * Is the flea search being performed a 'required' search type
     * @param info Search request
     * @returns True if it is a 'required' search type
     */
    private bool IsRequiredSearch(SearchRequestData searchRequest)
    {
        return searchRequest.NeededSearchId != "";
    }

    /**
     * Get offers for the client based on type of search being performed
     * @param searchRequest Client search request data
     * @param itemsToAdd Comes from ragfairHelper.filterCategories()
     * @param traderAssorts Trader assorts
     * @param pmcProfile Player profile
     * @returns array of offers
     */
    private List<RagfairOffer> GetOffersForSearchType(SearchRequestData searchRequest, List<string> itemsToAdd, Dictionary<string, TraderAssort> traderAssorts, PmcData pmcProfile)
    {
        // Searching for items in preset menu
        if (searchRequest.BuildCount is not null)
        {
            return _ragfairOfferHelper.GetOffersForBuild(searchRequest, itemsToAdd, traderAssorts, pmcProfile);
        }

        if (searchRequest.NeededSearchId?.Length > 0)
        {
            return _ragfairOfferHelper.GetOffersThatRequireItem(searchRequest, pmcProfile);
        }

        // Searching for general items
        return _ragfairOfferHelper.GetValidOffers(searchRequest, itemsToAdd, traderAssorts, pmcProfile);
    }

    /**
     * Called when creating an offer on flea, fills values in top right corner
     * @param getPriceRequest Client request object
     * @param ignoreTraderOffers Should trader offers be ignored in the calcualtion
     * @returns min/avg/max values for an item based on flea offers available
     */
    public GetItemPriceResult GetItemMinAvgMaxFleaPriceValues(GetMarketPriceRequestData getPriceRequest, bool ignoreTraderOffers = true)
    {
        // Get all items of tpl
        var offers = _ragfairOfferService.GetOffersOfType(getPriceRequest.TemplateId);

        // Offers exist for item, get averages of what's listed
        if (offers.Count > 0)
        {
            // These get calculated while iterating through the list below
            var minMax = new MinMax(0, int.MaxValue);

            // Get the average offer price, excluding barter offers
            var average = GetAveragePriceFromOffers(offers, minMax, ignoreTraderOffers);

            return new GetItemPriceResult{ Avg = Math.Round(average), Min = minMax.Min, Max = minMax.Max };
        }

        // No offers listed, get price from live ragfair price list prices.json
        // No flea price, get handbook price
        var fleaPrices = _databaseService.GetPrices();
        if (!fleaPrices.TryGetValue(getPriceRequest.TemplateId, out var tplPrice))
        {
            tplPrice = _handbookHelper.GetTemplatePrice(getPriceRequest.TemplateId) ?? 0;
        }
        
        return new GetItemPriceResult{ Avg = tplPrice, Min = tplPrice, Max = tplPrice };
    }

    private double GetAveragePriceFromOffers(List<RagfairOffer> offers, MinMax minMax, bool ignoreTraderOffers)
    {
        var sum = 0d;
        var totalOfferCount = 0;

        foreach (var offer in offers)
        {
            // Exclude barter items, they tend to have outrageous equivalent prices
            if (offer.Requirements.Any(req => !_paymentHelper.IsMoneyTpl(req.Template)))
            {
                continue;
            }

            if (ignoreTraderOffers && _ragfairOfferHelper.OfferIsFromTrader(offer))
            {
                continue;
            }

            // Figure out how many items the requirementsCost is applying to, and what the per-item price is
            var offerItemCount = offer.SellInOnePiece.GetValueOrDefault(false)
                ? (offer.Items.First().Upd?.StackObjectsCount ?? 1)
                : 1;
            var perItemPrice = offer.RequirementsCost / offerItemCount;

            // Handle min/max calculations based on the per-item price
            if (perItemPrice < minMax.Min)
            {
                minMax.Min = perItemPrice;
            }
            else if (perItemPrice > minMax.Max)
            {
                minMax.Max = perItemPrice;
            }

            sum += perItemPrice.Value;
            totalOfferCount++;
        }

        if (totalOfferCount == 0)
        {
            return -1d;
        }

        return sum / totalOfferCount;
    }

    /**
     * List item(s) on flea for sale
     * @param pmcData Player profile
     * @param offerRequest Flea list creation offer
     * @param sessionID Session id
     * @returns IItemEventRouterResponse
     */
    public ItemEventRouterResponse AddPlayerOffer(PmcData pmcData, AddOfferRequestData offerRequest, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);
        var fullProfile = _profileHelper.GetFullProfile(sessionID);

        var validationMessage = "";
        if (!IsValidPlayerOfferRequest(offerRequest, validationMessage))
        {
            return _httpResponseUtil.AppendErrorToOutput(output, validationMessage);
        }

        var typeOfOffer = GetOfferType(offerRequest);
        if (typeOfOffer == FleaOfferType.UNKNOWN)
        {
            return _httpResponseUtil.AppendErrorToOutput(output, "Unknown offer type, cannot list item on flea");
        }

        switch (typeOfOffer)
        {
            case FleaOfferType.SINGLE:
                return CreateSingleOffer(sessionID, offerRequest, fullProfile, output);
            case FleaOfferType.MULTI:
                return CreateMultiOffer(sessionID, offerRequest, fullProfile, output);
            case FleaOfferType.PACK:
                return CreatePackOffer(sessionID, offerRequest, fullProfile, output);
            case FleaOfferType.UNKNOWN:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /**
     * Is the item to be listed on the flea valid
     * @param offerRequest Client offer request
     * @param errorMessage message to show to player when offer is invalid
     * @returns Is offer valid
     */
    private bool IsValidPlayerOfferRequest(AddOfferRequestData offerRequest, string validationMessage)
    {
        if (offerRequest?.Items is null || offerRequest.Items.Count == 0)
        {
            _logger.Error(_localisationService.GetText("ragfair-invalid_player_offer_request"));

            return false;
        }

        if (offerRequest.Requirements is null)
        {
            _logger.Error(_localisationService.GetText("ragfair-unable_to_place_offer_with_no_requirements"));

            return false;
        }

        return true;
    }

    /**
     * Given a client request, determine what type of offer is being created
     * single/multi/pack
     * @param offerRequest Client request
     * @returns FleaOfferType
     */
    private FleaOfferType GetOfferType(AddOfferRequestData offerRequest)
    {
        var sellInOncePiece = offerRequest.SellInOnePiece.GetValueOrDefault(false);

        if (!sellInOncePiece)
        {
            if (offerRequest.Items.Count == 1)
            {
                return FleaOfferType.SINGLE;
            }
            if (offerRequest.Items.Count > 1)
            {
                return FleaOfferType.MULTI;
            }
        }

        if (sellInOncePiece)
        {
            return FleaOfferType.PACK;
        }

        return FleaOfferType.UNKNOWN;
    }

    /**
     * Create a flea offer for multiples of the same item, can be single items or items with multiple in the stack
     * e.g. 2 ammo stacks of 30 cartridges each
     * Each item can be purchased individually
     * @param sessionID Session id
     * @param offerRequest Offer request from client
     * @param fullProfile Full profile of player
     * @param output Response to send to client
     * @returns IItemEventRouterResponse
     */
    private ItemEventRouterResponse CreateMultiOffer(string sessionId, AddOfferRequestData offerRequest, SptProfile fullProfile, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /**
     * Create a flea offer for multiple items, can be single items or items with multiple in the stack
     * e.g. 2 ammo stacks of 30 cartridges each
     * The entire package must be purchased in one go
     * @param sessionID Session id
     * @param offerRequest Offer request from client
     * @param fullProfile Full profile of player
     * @param output Response to send to client
     * @returns IItemEventRouterResponse
     */
    private ItemEventRouterResponse CreatePackOffer(string sessionId, AddOfferRequestData offerRequest, SptProfile fullProfile, ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /**
     * Create a flea offer for a single item - includes an item with > 1 sized stack
     * e.g. 1 ammo stack of 30 cartridges
     * @param sessionID Session id
     * @param offerRequest Offer request from client
     * @param fullProfile Full profile of player
     * @param output Response to send to client
     * @returns IItemEventRouterResponse
     */
    private ItemEventRouterResponse CreateSingleOffer(string sessionID, AddOfferRequestData offerRequest, SptProfile fullProfile, ItemEventRouterResponse output)
    {
        var pmcData = fullProfile.CharacterData.PmcData;
        //var itemsToListCount = offerRequest.Items.Count; // Does not count stack size, only items

        // Find items to be listed on flea from player inventory
        var result = GetItemsToListOnFleaFromInventory(pmcData, offerRequest.Items);
        if (result.Items is null || result.error is not null)
        {
            _httpResponseUtil.AppendErrorToOutput(output, result.errorMessage);
        }

        // Total count of items summed using their stack counts
        var stackCountTotal = _ragfairOfferHelper.GetTotalStackCountSize(result.Items);

        // Checks are done, create the offer
        var playerListedPriceInRub = CalculateRequirementsPriceInRub(offerRequest.Requirements);
        var offer = CreatePlayerOffer(
            sessionID,
            offerRequest.Requirements,
            result.Items.First(),
            false);
        var rootItem = offer.Items.First();

        // Get average of items quality+children
        var qualityMultiplier = _itemHelper.GetItemQualityModifierForItems(offer.Items, true);

        // Average offer price for single item (or whole weapon)
        var averages = GetItemMinAvgMaxFleaPriceValues(new GetMarketPriceRequestData{ TemplateId = rootItem.Template });
        var averageOfferPriceSingleItem = averages.Avg;

        // Check for and apply item price modifer if it exists in config
        if (_ragfairConfig.Dynamic.ItemPriceMultiplier.TryGetValue(rootItem.Template, out double itemPriceModifer))
        {
            averageOfferPriceSingleItem *= itemPriceModifer;
        }

        // Multiply single item price by quality
        averageOfferPriceSingleItem *= qualityMultiplier;

        // Packs are reduced to the average price of a single item in the pack vs the averaged single price of an item
        var sellChancePercent = _ragfairSellHelper.CalculateSellChance(
            averageOfferPriceSingleItem.Value,
            playerListedPriceInRub,
            qualityMultiplier);
        offer.SellResult = _ragfairSellHelper.RollForSale(sellChancePercent, stackCountTotal);

        // Subtract flea market fee from stash
        if (_ragfairConfig.Sell.Fees)
        {
            var taxFeeChargeFailed = ChargePlayerTaxFee(
                sessionID,
                rootItem,
                pmcData,
                playerListedPriceInRub,
                stackCountTotal,
                offerRequest,
                output);
            if (taxFeeChargeFailed)
            {
                return output;
            }
        }

        // Add offer to players profile + add to client response
        fullProfile.CharacterData.PmcData.RagfairInfo.Offers.Add(offer);
        output.ProfileChanges[sessionID].RagFairOffers.Add(offer);

        // Remove items from inventory after creating offer
        foreach (var itemToRemove in offerRequest.Items) {
            _inventoryHelper.RemoveItem(pmcData, itemToRemove, sessionID, output);
        }

        return output;
    }

    /**
     * Charge player a listing fee for using flea, pulls charge from data previously sent by client
     * @param sessionID Player id
     * @param rootItem Base item being listed (used when client tax cost not found and must be done on server)
     * @param pmcData Player profile
     * @param requirementsPriceInRub Rouble cost player chose for listing (used when client tax cost not found and must be done on server)
     * @param itemStackCount How many items were listed by player (used when client tax cost not found and must be done on server)
     * @param offerRequest Add offer request object from client
     * @param output IItemEventRouterResponse
     * @returns True if charging tax to player failed
     */
    private bool ChargePlayerTaxFee(
        string sessionId,
        Item rootItem,
        PmcData pmcData,
        double requirementsPriceInRub,
        int itemStackCount,
        AddOfferRequestData offerRequest,
        ItemEventRouterResponse output)
    {
        // Get tax from cache hydrated earlier by client, if that's missing fall back to server calculation (inaccurate)
        var storedClientTaxValue = _ragfairTaxService.GetStoredClientOfferTaxValueById(offerRequest.Items[0]);
        var tax = storedClientTaxValue is not null
            ? storedClientTaxValue.Fee
            : _ragfairTaxService.CalculateTax(
                rootItem,
                pmcData,
                requirementsPriceInRub,
                itemStackCount,
                offerRequest.SellInOnePiece.GetValueOrDefault(false));

        _logger.Debug($"Offer tax to charge: { tax}, pulled from client: { storedClientTaxValue.Count is not null}");

        // cleanup of cache now we've used the tax value from it
        _ragfairTaxService.ClearStoredOfferTaxById(offerRequest.Items.First());

        var buyTradeRequest = CreateBuyTradeRequestObject("RUB", tax.Value);
        _paymentService.PayMoney(pmcData, buyTradeRequest, sessionId, output);
        if (output.Warnings.Count > 0)
        {
            _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("ragfair-unable_to_pay_commission_fee", tax));
            return true;
        }

        return false;
    }

    private RagfairOffer CreatePlayerOffer(string sessionId, List<Requirement> requirements, List<Item> items, bool sellInOnePiece)
    {
        var loyalLevel = 1;
        var formattedItems = items.Select(item => {
            var isChild = items.Any((subItem) => subItem.Id == item.ParentId);

            return new Item {
                Id = item.Id,
                Template = item.Template,
                ParentId = isChild ? item.ParentId: "hideout",
                SlotId = isChild ? item.SlotId: "hideout",
                Upd = item.Upd };
        });

        var formattedRequirements = requirements.Select(item => new BarterScheme{
                Template = item.Template,
                Count = item.Count,
                OnlyFunctional = item.OnlyFunctional,
            }
        );

        return _ragfairOfferGenerator.CreateAndAddFleaOffer(
            sessionId,
            _timeUtil.GetTimeStamp(),
            formattedItems.ToList(),
            formattedRequirements.ToList(),
            loyalLevel,
            sellInOnePiece);
    }

    /**
     * Get the handbook price in roubles for the items being listed
     * @param requirements
     * @returns Rouble price
     */
    private double CalculateRequirementsPriceInRub(List<Requirement> requirements)
    {
        var requirementsPriceInRub = 0d;
        foreach (var item in requirements) {
            var requestedItemTpl = item.Template;

            if (_paymentHelper.IsMoneyTpl(requestedItemTpl))
            {
                requirementsPriceInRub += _handbookHelper.InRUB(item.Count.Value, requestedItemTpl);
            }
            else
            {
                requirementsPriceInRub += _ragfairPriceService.GetDynamicPriceForItem(requestedItemTpl).Value * item.Count.Value;
            }
        }

        return requirementsPriceInRub;
    }

    private dynamic GetItemsToListOnFleaFromInventory(PmcData pmcData, List<string> itemIdsFromFleaOfferRequest)
    {
        List<List<Item> > itemsToReturn = [];
        var errorMessage = string.Empty;

        // Count how many items are being sold and multiply the requested amount accordingly
        foreach (var itemId in itemIdsFromFleaOfferRequest) {
            var item = pmcData.Inventory.Items.FirstOrDefault((i) => i.Id == itemId);
            if (item is null)
            {
                errorMessage = _localisationService.GetText("ragfair-unable_to_find_item_in_inventory", new { id = itemId});
                _logger.Error(errorMessage);

                return new { itemsToReturn, errorMessage };
            }

            item = _itemHelper.FixItemStackCount(item);
            itemsToReturn.Add(_itemHelper.FindAndReturnChildrenAsItems(pmcData.Inventory.Items, itemId));
        }

        if (itemsToReturn?.Count == 0)
        {
            errorMessage = _localisationService.GetText("ragfair-unable_to_find_requested_items_in_inventory");
            _logger.Error(errorMessage);

            return new { ErrorMessage = errorMessage };
        }

        return new { Items = itemsToReturn, ErrorMessage = errorMessage };
    }

    public ItemEventRouterResponse RemoveOffer(RemoveOfferRequestData removeRequest, string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var playerProfileOffers = pmcData.RagfairInfo.Offers;
        if (playerProfileOffers is null)
        {
            _logger.Warning(
                _localisationService.GetText("ragfair-unable_to_remove_offer_not_found_in_profile", new {
                profileId = sessionId,
                offerId = removeRequest.OfferId }));

            pmcData.RagfairInfo.Offers = new List<RagfairOffer>();
        }

        var playerOfferIndex = playerProfileOffers.FindIndex(offer => offer.Id == removeRequest.OfferId);
        if (playerOfferIndex == -1)
        {
            _logger.Error(
                _localisationService.GetText("ragfair-offer_not_found_in_profile", new {
                offerId = removeRequest.OfferId }));
            return _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("ragfair-offer_not_found_in_profile_short"));
        }

        var differenceInSeconds = playerProfileOffers[playerOfferIndex].EndTime - _timeUtil.GetTimeStamp();
        if (differenceInSeconds > _ragfairConfig.Sell.ExpireSeconds)
        {
            // `expireSeconds` Default is 71 seconds
            var newEndTime = _ragfairConfig.Sell.ExpireSeconds + _timeUtil.GetTimeStamp();
            playerProfileOffers[playerOfferIndex].EndTime = (long?)Math.Round((double)newEndTime);
        }

        return output;
    }

    public ItemEventRouterResponse ExtendOffer(ExtendOfferRequestData extendRequest, string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var playerOffers = pmcData.RagfairInfo.Offers;
        var playerOfferIndex = playerOffers.FindIndex((offer) => offer.Id == extendRequest.OfferId);
        var secondsToAdd = extendRequest.RenewalTime * TimeUtil.OneHourAsSeconds;

        if (playerOfferIndex == -1)
        {
            _logger.Warning(
                _localisationService.GetText("ragfair-offer_not_found_in_profile", new {
            offerId = extendRequest.OfferId }));
            return _httpResponseUtil.AppendErrorToOutput(output, _localisationService.GetText("ragfair-offer_not_found_in_profile_short"));
        }

        var playerOffer = playerOffers[playerOfferIndex];

        // MOD: Pay flea market fee
        if (_ragfairConfig.Sell.Fees)
        {
            var count = 1;
            var sellInOncePiece = playerOffer.SellInOnePiece.GetValueOrDefault(false);
            if (!sellInOncePiece)
            {
                count = (int) playerOffer.Items.Sum(offerItem => offerItem.Upd?.StackObjectsCount ?? 0);
            }

            var tax = _ragfairTaxService.CalculateTax(
                playerOffer.Items.First(),
                pmcData,
                playerOffer.RequirementsCost.Value,
                count,
                sellInOncePiece);

            var request = CreateBuyTradeRequestObject("RUB", tax);
            _paymentService.PayMoney(pmcData, request, sessionId, output);
            if (output.Warnings.Count > 0)
            {
                return _httpResponseUtil.AppendErrorToOutput(
                    output,
                    _localisationService.GetText("ragfair-unable_to_pay_commission_fee"));
            }
        }

        // Add extra time to offer
        playerOffers[playerOfferIndex].EndTime += (long?)Math.Round((decimal)secondsToAdd);

        return output;
    }

    /**
     * Create a basic trader request object with price and currency type
     * @param currency What currency: RUB, EURO, USD
     * @param value Amount of currency
     * @returns IProcessBuyTradeRequestData
     */
    private ProcessBuyTradeRequestData CreateBuyTradeRequestObject(string currency, double value)
    {
        return new ProcessBuyTradeRequestData
        {
            TId = "ragfair",
            Action = "TradingConfirm",
            SchemeItems = [new SchemeItem { Id = _paymentHelper.GetCurrency(currency), Count = Math.Round(value) }],
            Type = "",
            ItemId = "",
            Count = 0,
            SchemeId = 0,
        };
    }

    public Dictionary<string, double> GetAllFleaPrices()
    {
        return _ragfairPriceService.GetAllFleaPrices();
    }

    public Dictionary<string, double> GetStaticPrices() {
        return _ragfairPriceService.GetAllStaticPrices();
    }

    public RagfairOffer? GetOfferById(string sessionId, GetRagfairOfferByIdRequest request)
    {
        var offers = _ragfairOfferService.GetOffers();
        var offerToReturn = offers.FirstOrDefault((offer) => offer.InternalId == request.Id);

        return offerToReturn;
    }
}
