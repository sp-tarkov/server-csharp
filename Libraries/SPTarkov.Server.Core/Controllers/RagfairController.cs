using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class RagfairController
{
    protected ConfigServer _configServer;
    protected DatabaseService _databaseService;
    protected EventOutputHolder _eventOutputHolder;
    protected HandbookHelper _handbookHelper;
    protected HttpResponseUtil _httpResponseUtil;
    protected InventoryHelper _inventoryHelper;
    protected ItemHelper _itemHelper;
    protected JsonUtil _jsonUtil;
    protected LocalisationService _localisationService;
    protected ISptLogger<RagfairController> _logger;
    protected PaymentHelper _paymentHelper;
    protected PaymentService _paymentService;
    protected ProfileHelper _profileHelper;

    protected RagfairConfig _ragfairConfig;
    protected RagfairHelper _ragfairHelper;
    protected RagfairOfferGenerator _ragfairOfferGenerator;
    protected RagfairOfferHelper _ragfairOfferHelper;
    protected RagfairOfferService _ragfairOfferService;
    protected RagfairPriceService _ragfairPriceService;
    protected RagfairSellHelper _ragfairSellHelper;
    protected RagfairServer _ragfairServer;
    protected RagfairSortHelper _ragfairSortHelper;
    protected RagfairTaxService _ragfairTaxService;
    protected TimeUtil _timeUtil;
    protected TraderHelper _traderHelper;

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

    /// <summary>
    /// Check all profiles and sell player offers / send player money for listing if it sold
    /// </summary>
    public void Update()
    {
        foreach (var (sessionId, profile) in _profileHelper.GetProfiles())
        {
            // Check profile is capable of creating offers
            var pmcProfile = profile.CharacterData.PmcData;
            if (
                pmcProfile.RagfairInfo is not null &&
                pmcProfile.Info.Level >= _databaseService.GetGlobals().Configuration.RagFair.MinUserLevel
            )
            {
                _ragfairOfferHelper.ProcessOffersOnProfile(sessionId);
            }
        }
    }

    /// <summary>
    /// Handles client/ragfair/find
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="searchRequest">Search request data</param>
    /// <returns>Flea offers that match required search parameters</returns>
    public GetOffersResult GetOffers(string sessionID, SearchRequestData searchRequest)
    {
        var profile = _profileHelper.GetFullProfile(sessionID);

        var itemsToAdd = _ragfairHelper.FilterCategories(sessionID, searchRequest);
        var traderAssorts = _ragfairHelper.GetDisplayableAssorts(sessionID);
        var result = new GetOffersResult
        {
            Offers = [],
            OffersCount = searchRequest.Limit,
            SelectedCategory = searchRequest.HandbookId
        };

        result.Offers = GetOffersForSearchType(searchRequest, itemsToAdd, traderAssorts, profile.CharacterData.PmcData);

        // Client requested a category refresh
        if (searchRequest.UpdateOfferCount.GetValueOrDefault(false))
        {
            result.Categories = GetSpecificCategories(profile.CharacterData.PmcData, searchRequest, result.Offers);
        }

        AddIndexValueToOffers(result.Offers);

        // Sort offers
        result.Offers = _ragfairSortHelper.SortOffers(
            result.Offers,
            searchRequest.SortType.Value,
            searchRequest.SortDirection.Value
        );

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
            var end = Math.Min((searchRequest.Page.Value + 1) * searchRequest.Limit.Value, result.Offers.Count);
            result.Offers = result.Offers.Slice(start.Value, end - start.Value);
        }

        return result;
    }

    /// <summary>
    /// Adjust ragfair offer stack count to match same value as traders assort stack count
    /// </summary>
    /// <param name="offer">Flea offer to adjust stack size of</param>
    private void SetTraderOfferStackSize(RagfairOffer offer)
    {
        var firstItem = offer.Items[0];
        var traderAssorts = _traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;

        var assortPurchased = traderAssorts?.FirstOrDefault(x => x.Id == offer.Items.First().Id);
        if (assortPurchased is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "ragfair-unable_to_adjust_stack_count_assort_not_found",
                    new
                    {
                        offerId = offer.Items.First().Id,
                        traderId = offer.User.Id
                    }
                )
            );

            return;
        }

        firstItem.Upd.StackObjectsCount = assortPurchased.Upd.StackObjectsCount;
    }

    /// <summary>
    /// Update a trader flea offer with buy restrictions stored in the traders assort
    /// </summary>
    /// <param name="offer">Flea offer to update</param>
    /// <param name="fullProfile">Players full profile</param>
    private void SetTraderOfferPurchaseLimits(RagfairOffer offer, SptProfile fullProfile)
    {
        var offerRootItem = offer.Items.First();
        var assortId = offerRootItem.Id;

        // No trader found in profile, create a blank record for them
        var existsInProfile = !fullProfile.TraderPurchases.TryAdd(
            offer.User.Id,
            new Dictionary<string, TraderPurchaseData>()
        );
        if (!existsInProfile)
        {
            // Not purchased by player before, use value from assort data

            // Find patching assort by its id
            var traderAssorts = _traderHelper.GetTraderAssortsByTraderId(offer.User.Id).Items;
            var assortData = traderAssorts.FirstOrDefault(item => item.Id == assortId);

            // Set restriction based on data found above
            offer.BuyRestrictionMax = assortData.Upd.BuyRestrictionMax;

            return;
        }

        // Get purchases player made with trader since last reset
        var traderPurchases = fullProfile.TraderPurchases[offer.User.Id];

        // Get specific assort purchase data and set current purchase buy value
        traderPurchases.TryGetValue(assortId, out var assortTraderPurchaseData);

        offer.BuyRestrictionCurrent = (int?) assortTraderPurchaseData?.PurchaseCount ?? 0;
        offer.BuyRestrictionMax = offerRootItem.Upd.BuyRestrictionMax;
    }

    /// <summary>
    /// Add index to all offers passed in (0-indexed)
    /// </summary>
    /// <param name="offers">Offers to add index value to</param>
    protected void AddIndexValueToOffers(List<RagfairOffer> offers)
    {
        var counter = 0;

        foreach (var offer in offers)
        {
            offer.InternalId = ++counter;
        }
    }

    /// <summary>
    /// Get categories for the type of search being performed, linked/required/all
    /// </summary>
    /// <param name="pmcProfile"></param>
    /// <param name="searchRequest">Client search request data</param>
    /// <param name="offers">Ragfair offers to get categories for</param>
    /// <returns>Record with templates + counts</returns>
    protected Dictionary<MongoId, int> GetSpecificCategories(PmcData pmcProfile, SearchRequestData searchRequest,
        List<RagfairOffer> offers)
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
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug(_jsonUtil.Serialize(searchRequest));
            }

            return new Dictionary<MongoId, int>();
        }

        return _ragfairServer.GetAllActiveCategories(playerHasFleaUnlocked, searchRequest, offerPool);
    }

    /// <summary>
    /// Is the flea search being performed a 'linked' search type
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>True = a 'linked' search type</returns>
    protected bool IsLinkedSearch(SearchRequestData searchRequest)
    {
        return !string.IsNullOrEmpty(searchRequest.LinkedSearchId);
    }

    /// <summary>
    /// Is the flea search being performed a 'required' search type
    /// </summary>
    /// <param name="searchRequest">Search request</param>
    /// <returns>True if it is a 'required' search type</returns>
    protected bool IsRequiredSearch(SearchRequestData searchRequest)
    {
        return !string.IsNullOrEmpty(searchRequest.NeededSearchId);
    }

    /// <summary>
    /// Get offers for the client based on type of search being performed
    /// </summary>
    /// <param name="searchRequest">Client search request data</param>
    /// <param name="itemsToAdd">Comes from ragfairHelper.filterCategories()</param>
    /// <param name="traderAssorts">Trader assorts</param>
    /// <param name="pmcProfile"></param>
    /// <returns>Array of offers</returns>
    protected List<RagfairOffer> GetOffersForSearchType(SearchRequestData searchRequest, List<string> itemsToAdd,
        Dictionary<string, TraderAssort> traderAssorts,
        PmcData pmcProfile)
    {
        // Searching for items in preset menu
        if (searchRequest.BuildCount > 0)
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

    /// <summary>
    /// Called when creating an offer on flea, fills values in top right corner
    /// </summary>
    /// <param name="getPriceRequest">Client request object</param>
    /// <param name="ignoreTraderOffers">OPTIONAL - Should trader offers be ignored in the calculation</param>
    /// <returns>min/avg/max values for an item based on flea offers available</returns>
    public GetItemPriceResult GetItemMinAvgMaxFleaPriceValues(GetMarketPriceRequestData getPriceRequest,
        bool ignoreTraderOffers = true)
    {
        // Get all items of tpl
        var offers = _ragfairOfferService.GetOffersOfType(getPriceRequest.TemplateId);

        // Offers exist for item, get averages of what's listed
        if (offers.Count > 0)
        {
            // These get calculated while iterating through the list below
            var minMax = new MinMax<double>(int.MaxValue, 0);

            // Get the average offer price, excluding barter offers
            var average = GetAveragePriceFromOffers(offers, minMax, ignoreTraderOffers);

            return new GetItemPriceResult
            {
                Avg = Math.Round(average),
                Min = minMax.Min,
                Max = minMax.Max
            };
        }

        // No offers listed, get price from live ragfair price list prices.json
        // No flea price, get handbook price
        var fleaPrices = _databaseService.GetPrices();
        if (!fleaPrices.TryGetValue(getPriceRequest.TemplateId, out var tplPrice))
        {
            tplPrice = _handbookHelper.GetTemplatePrice(getPriceRequest.TemplateId);
        }

        return new GetItemPriceResult
        {
            Avg = tplPrice,
            Min = tplPrice,
            Max = tplPrice
        };
    }

    protected double GetAveragePriceFromOffers(List<RagfairOffer> offers, MinMax<double> minMax, bool ignoreTraderOffers)
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
                ? offer.Items.First().Upd?.StackObjectsCount ?? 1
                : 1;
            var perItemPrice = offer.RequirementsCost / offerItemCount;

            // Handle min/max calculations based on the per-item price
            if (perItemPrice < minMax.Min)
            {
                minMax.Min = perItemPrice.Value;
            }
            else if (perItemPrice > minMax.Max)
            {
                minMax.Max = perItemPrice.Value;
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

    /// <summary>
    /// List item(s) on flea for sale
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="offerRequest">Flea list creation offer</param>
    /// <param name="sessionID">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse AddPlayerOffer(PmcData pmcData, AddOfferRequestData offerRequest, string sessionID)
    {
        var output = _eventOutputHolder.GetOutput(sessionID);
        var fullProfile = _profileHelper.GetFullProfile(sessionID);

        if (!IsValidPlayerOfferRequest(offerRequest))
        {
            return _httpResponseUtil.AppendErrorToOutput(output, "Unable to add offer, check server for error");
        }

        var typeOfOffer = GetOfferType(offerRequest);
        if (typeOfOffer == FleaOfferType.UNKNOWN)
        {
            return _httpResponseUtil.AppendErrorToOutput(
                output,
                $"Unknown offer type: {typeOfOffer}, cannot list item on flea"
            );
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
                return _httpResponseUtil.AppendErrorToOutput(
                    output,
                    $"Unknown offer type: {typeOfOffer}, cannot list item on flea"
                );
        }
    }

    /// <summary>
    /// Is the item to be listed on the flea valid
    /// </summary>
    /// <param name="offerRequest">Client offer request</param>
    /// <returns>Is offer valid</returns>
    protected bool IsValidPlayerOfferRequest(AddOfferRequestData offerRequest)
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

    /// <summary>
    /// Given a client request, determine what type of offer is being created single/multi/pack
    /// </summary>
    /// <param name="offerRequest">Client request</param>
    /// <returns>FleaOfferType</returns>
    protected FleaOfferType GetOfferType(AddOfferRequestData offerRequest)
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

    /// <summary>
    /// Create a flea offer for multiples of the same item, can be single items or items with multiple in the stack
    /// e.g. 2 ammo stacks of 30 cartridges each
    /// Each item can be purchased individually
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="offerRequest">Offer request from client</param>
    /// <param name="fullProfile">Full profile of player</param>
    /// <param name="output">output Response to send to client</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse CreateMultiOffer(string sessionID, AddOfferRequestData offerRequest,
        SptProfile fullProfile, ItemEventRouterResponse output)
    {
        var pmcData = fullProfile.CharacterData.PmcData;
        // var itemsToListCount = offerRequest.Items.Count; // Wasnt used to commented out for now // Does not count stack size, only items

        // multi-offers are all the same item,
        // Get first item and its children and use as template
        var firstListingAndChidren = _itemHelper.FindAndReturnChildrenAsItems(
            pmcData.Inventory.Items,
            offerRequest.Items[0]
        );

        // Find items to be listed on flea (+ children) from player inventory
        var result = GetItemsToListOnFleaFromInventory(pmcData, offerRequest.Items);
        if (result.Items is null || !string.IsNullOrEmpty(result.ErrorMessage))
        {
            _httpResponseUtil.AppendErrorToOutput(output, result.ErrorMessage);
        }

        // Total count of items summed using their stack counts
        var stackCountTotal = _ragfairOfferHelper.GetTotalStackCountSize(result.Items);

        // When listing identical items on flea, condense separate items into one stack with a merged stack count
        // e.g. 2 ammo items, stackObjectCount = 3 for each, will result in 1 stack of 6
        firstListingAndChidren[0].Upd ??= new Upd();
        firstListingAndChidren[0].Upd.StackObjectsCount = stackCountTotal;

        // Create flea object
        var offer = CreatePlayerOffer(sessionID, offerRequest.Requirements, firstListingAndChidren, false);

        // This is the item that will be listed on flea, has merged stackObjectCount
        var newRootOfferItem = offer.Items[0];

        // Average offer price for single item (or whole weapon)
        var averages =
            GetItemMinAvgMaxFleaPriceValues(
                new GetMarketPriceRequestData
                {
                    TemplateId = offer.Items[0].Template
                }
            );
        var averageOfferPrice = averages.Avg;

        // Check for and apply item price modifer if it exists in config
        if (_ragfairConfig.Dynamic.ItemPriceMultiplier.TryGetValue((MongoId) newRootOfferItem.Template, out var itemPriceModifer))
        {
            averageOfferPrice *= itemPriceModifer;
        }

        // Get average of item+children quality
        var qualityMultiplier = _itemHelper.GetItemQualityModifierForItems(offer.Items, true);

        // Multiply single item price by quality
        averageOfferPrice *= qualityMultiplier;

        // Get price player listed items for in roubles
        var playerListedPriceInRub = CalculateRequirementsPriceInRub(offerRequest.Requirements);

        // Roll sale chance
        var sellChancePercent = _ragfairSellHelper.CalculateSellChance(
            averageOfferPrice.Value,
            playerListedPriceInRub,
            qualityMultiplier
        );

        // Create array of sell times for items listed
        offer.SellResults = _ragfairSellHelper.RollForSale(sellChancePercent, (int) stackCountTotal);

        // Subtract flea market fee from stash
        if (_ragfairConfig.Sell.Fees)
        {
            var taxFeeChargeFailed = ChargePlayerTaxFee(
                sessionID,
                newRootOfferItem,
                pmcData,
                playerListedPriceInRub,
                (int) stackCountTotal,
                offerRequest,
                output
            );
            if (taxFeeChargeFailed)
            {
                return output;
            }
        }

        // Add offer to players profile + add to client response
        fullProfile.CharacterData.PmcData.RagfairInfo.Offers.Add(offer);
        output.ProfileChanges[sessionID].RagFairOffers.Add(offer);

        // Remove items from inventory after creating offer
        foreach (var itemToRemove in offerRequest.Items)
        {
            _inventoryHelper.RemoveItem(pmcData, itemToRemove, sessionID, output);
        }

        return output;
    }

    /// <summary>
    /// Create a flea offer for multiple items, can be single items or items with multiple in the stack
    /// e.g. 2 ammo stacks of 30 cartridges each
    /// The entire package must be purchased in one go
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="offerRequest">Offer request from client</param>
    /// <param name="fullProfile">Full profile of player</param>
    /// <param name="output">Response to send to client</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse CreatePackOffer(string sessionID, AddOfferRequestData offerRequest,
        SptProfile fullProfile, ItemEventRouterResponse output)
    {
        var pmcData = fullProfile.CharacterData.PmcData;
        // var itemsToListCount = offerRequest.Items.Count; // Wasn't used so commented out for now // Does not count stack size, only items

        // multi-offers are all the same item,
        // Get first item and its children and use as template
        var firstListingAndChidren = _itemHelper.FindAndReturnChildrenAsItems(
            pmcData.Inventory.Items,
            offerRequest.Items[0]
        );

        // Find items to be listed on flea (+ children) from player inventory
        var result = GetItemsToListOnFleaFromInventory(pmcData, offerRequest.Items);
        if (result.Items is null || !string.IsNullOrEmpty(result.ErrorMessage))
        {
            _httpResponseUtil.AppendErrorToOutput(output, result.ErrorMessage);
        }

        // Total count of items summed using their stack counts
        var stackCountTotal = _ragfairOfferHelper.GetTotalStackCountSize(result.Items);

        // When listing identical items on flea, condense separate items into one stack with a merged stack count
        // e.g. 2 ammo items, stackObjectCount = 3 for each, will result in 1 stack of 6
        firstListingAndChidren[0].Upd ??= new Upd();

        firstListingAndChidren[0].Upd.StackObjectsCount = stackCountTotal;

        // Create flea object
        var offer = CreatePlayerOffer(sessionID, offerRequest.Requirements, firstListingAndChidren, true);

        // This is the item that will be listed on flea, has merged stackObjectCount
        var newRootOfferItem = offer.Items[0];

        // Single price for an item
        var averages = GetItemMinAvgMaxFleaPriceValues(
            new GetMarketPriceRequestData
            {
                TemplateId = firstListingAndChidren[0].Template
            }
        );
        var singleItemPrice = averages.Avg;

        // Check for and apply item price modifer if it exists in config
        if (_ragfairConfig.Dynamic.ItemPriceMultiplier.TryGetValue((MongoId) newRootOfferItem.Template, out var itemPriceModifer))
        {
            singleItemPrice *= itemPriceModifer;
        }

        // Get average of item+children quality
        var qualityMultiplier = _itemHelper.GetItemQualityModifierForItems(offer.Items, true);

        // Multiply single item price by quality
        singleItemPrice *= qualityMultiplier;

        // Get price player listed items for in roubles
        var playerListedPriceInRub = CalculateRequirementsPriceInRub(offerRequest.Requirements);

        // Roll sale chance
        var sellChancePercent = _ragfairSellHelper.CalculateSellChance(
            singleItemPrice.Value * stackCountTotal,
            playerListedPriceInRub,
            qualityMultiplier
        );

        // Create array of sell times for items listed + sell all at once as its a pack
        offer.SellResults = _ragfairSellHelper.RollForSale(sellChancePercent, (int) stackCountTotal, true);

        // Subtract flea market fee from stash
        if (_ragfairConfig.Sell.Fees)
        {
            var taxFeeChargeFailed = ChargePlayerTaxFee(
                sessionID,
                newRootOfferItem,
                pmcData,
                playerListedPriceInRub,
                (int) stackCountTotal,
                offerRequest,
                output
            );
            if (taxFeeChargeFailed)
            {
                return output;
            }
        }

        // Add offer to players profile + add to client response
        fullProfile.CharacterData.PmcData.RagfairInfo.Offers.Add(offer);
        output.ProfileChanges[sessionID].RagFairOffers.Add(offer);

        // Remove items from inventory after creating offer
        foreach (var itemToRemove in offerRequest.Items)
        {
            _inventoryHelper.RemoveItem(pmcData, itemToRemove, sessionID, output);
        }

        return output;
    }

    /// <summary>
    /// Create a flea offer for a single item - includes an item with > 1 sized stack
    /// e.g. 1 ammo stack of 30 cartridges
    /// </summary>
    /// <param name="sessionID">Session/Player id</param>
    /// <param name="offerRequest">Offer request from client</param>
    /// <param name="fullProfile">Full profile of player</param>
    /// <param name="output">Response to send to client</param>
    /// <returns>ItemEventRouterResponse</returns>
    protected ItemEventRouterResponse CreateSingleOffer(string sessionID, AddOfferRequestData offerRequest,
        SptProfile fullProfile,
        ItemEventRouterResponse output)
    {
        var pmcData = fullProfile.CharacterData.PmcData;
        // var itemsToListCount = offerRequest.Items.Count; // Wasn't used so commented out for now // Does not count stack size, only items

        // Find items to be listed on flea from player inventory
        var result = GetItemsToListOnFleaFromInventory(pmcData, offerRequest.Items);
        if (result.Items is null || !string.IsNullOrEmpty(result.ErrorMessage))
        {
            _httpResponseUtil.AppendErrorToOutput(output, result.ErrorMessage);
        }

        // Total count of items summed using their stack counts
        var stackCountTotal = _ragfairOfferHelper.GetTotalStackCountSize(result.Items);

        // Checks are done, create the offer
        var playerListedPriceInRub = CalculateRequirementsPriceInRub(offerRequest.Requirements);
        var offer = CreatePlayerOffer(
            sessionID,
            offerRequest.Requirements,
            result.Items.First(),
            false
        );
        var rootItem = offer.Items.First();

        // Get average of items quality+children
        var qualityMultiplier = _itemHelper.GetItemQualityModifierForItems(offer.Items, true);

        // Average offer price for single item (or whole weapon)
        var averages =
            GetItemMinAvgMaxFleaPriceValues(
                new GetMarketPriceRequestData
                {
                    TemplateId = rootItem.Template
                }
            );
        var averageOfferPriceSingleItem = averages.Avg;

        // Check for and apply item price modifer if it exists in config
        if (_ragfairConfig.Dynamic.ItemPriceMultiplier.TryGetValue((MongoId) rootItem.Template, out var itemPriceModifer))
        {
            averageOfferPriceSingleItem *= itemPriceModifer;
        }

        // Multiply single item price by quality
        averageOfferPriceSingleItem *= qualityMultiplier;

        // Packs are reduced to the average price of a single item in the pack vs the averaged single price of an item
        var sellChancePercent = _ragfairSellHelper.CalculateSellChance(
            averageOfferPriceSingleItem.Value,
            playerListedPriceInRub,
            qualityMultiplier
        );
        offer.SellResults = _ragfairSellHelper.RollForSale(sellChancePercent, (int) stackCountTotal);

        // Subtract flea market fee from stash
        if (_ragfairConfig.Sell.Fees)
        {
            var taxFeeChargeFailed = ChargePlayerTaxFee(
                sessionID,
                rootItem,
                pmcData,
                playerListedPriceInRub,
                (int) stackCountTotal,
                offerRequest,
                output
            );
            if (taxFeeChargeFailed)
            {
                return output;
            }
        }

        // Add offer to players profile + add to client response
        fullProfile.CharacterData.PmcData.RagfairInfo.Offers.Add(offer);
        output.ProfileChanges[sessionID].RagFairOffers.Add(offer);

        // Remove items from inventory after creating offer
        foreach (var itemToRemove in offerRequest.Items)
        {
            _inventoryHelper.RemoveItem(pmcData, itemToRemove, sessionID, output);
        }

        return output;
    }

    /// <summary>
    /// Charge player a listing fee for using flea, pulls charge from data previously sent by client
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="rootItem">Base item being listed (used when client tax cost not found and must be done on server)</param>
    /// <param name="pmcData"></param>
    /// <param name="requirementsPriceInRub">Rouble cost player chose for listing (used when client tax cost not found and must be done on server)</param>
    /// <param name="itemStackCount">How many items were listed by player (used when client tax cost not found and must be done on server)</param>
    /// <param name="offerRequest">Add offer request object from client</param>
    /// <param name="output">ItemEventRouterResponse</param>
    /// <returns>True if charging tax to player failed</returns>
    protected bool ChargePlayerTaxFee(
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
                offerRequest.SellInOnePiece.GetValueOrDefault(false)
            );

        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Offer tax to charge: {tax}, pulled from client: {storedClientTaxValue.Count is not null}");
        }

        // cleanup of cache now we've used the tax value from it
        _ragfairTaxService.ClearStoredOfferTaxById(offerRequest.Items.First());

        var buyTradeRequest = CreateBuyTradeRequestObject(CurrencyType.RUB, tax.Value);
        _paymentService.PayMoney(pmcData, buyTradeRequest, sessionId, output);
        if (output.Warnings.Count > 0)
        {
            _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("ragfair-unable_to_pay_commission_fee", tax)
            );
            return true;
        }

        return false;
    }

    /// <summary>
    /// Create a flea offer for a player
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="requirements"></param>
    /// <param name="items">Item(s) to list on flea (with children)</param>
    /// <param name="sellInOnePiece">Is this a pack offer</param>
    /// <returns>RagfairOffer</returns>
    protected RagfairOffer CreatePlayerOffer(string sessionId, List<Requirement> requirements, List<Item> items,
        bool sellInOnePiece)
    {
        const int loyalLevel = 1;
        var formattedItems = items.Select(
            item =>
            {
                var isChild = items.Any(subItem => subItem.Id == item.ParentId);

                return new Item
                {
                    Id = item.Id,
                    Template = item.Template,
                    ParentId = isChild ? item.ParentId : "hideout",
                    SlotId = isChild ? item.SlotId : "hideout",
                    Upd = item.Upd
                };
            }
        );

        var formattedRequirements = requirements.Select(
            item => new BarterScheme
            {
                Template = item.Template,
                Count = item.Count,
                OnlyFunctional = item.OnlyFunctional
            }
        );

        return _ragfairOfferGenerator.CreateAndAddFleaOffer(
            sessionId,
            _timeUtil.GetTimeStamp(),
            formattedItems.ToList(),
            formattedRequirements.ToList(),
            loyalLevel,
            (int?)items.FirstOrDefault()?.Upd?.StackObjectsCount ?? 1,
            sellInOnePiece
        );
    }

    /// <summary>
    /// Get the handbook price in roubles for the items being listed
    /// </summary>
    /// <param name="requirements"></param>
    /// <returns>Rouble price</returns>
    protected double CalculateRequirementsPriceInRub(List<Requirement> requirements)
    {
        return requirements.Sum(requirement =>
            {
                if (string.IsNullOrEmpty(requirement.Template) || !requirement.Count.HasValue || requirement.Count == 0)
                {
                    return 0;
                }

                return _paymentHelper.IsMoneyTpl(requirement.Template)
                    ? _handbookHelper.InRUB(requirement.Count.Value, requirement.Template)
                    : _itemHelper.GetDynamicItemPrice(requirement.Template).Value * requirement.Count.Value;
            }
        );
    }

    /// <summary>
    /// Find items with their children from players inventory
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="itemIdsFromFleaOfferRequest">Request</param>
    /// <returns>GetItemsToListOnFleaFromInventoryResult</returns>
    protected GetItemsToListOnFleaFromInventoryResult GetItemsToListOnFleaFromInventory(PmcData pmcData,
        List<MongoId> itemIdsFromFleaOfferRequest)
    {
        List<List<Item>> itemsToReturn = [];
        var errorMessage = string.Empty;

        // Count how many items are being sold and multiply the requested amount accordingly
        foreach (var itemId in itemIdsFromFleaOfferRequest)
        {
            var item = pmcData.Inventory?.Items?.FirstOrDefault(i => i.Id == itemId);
            if (item is null)
            {
                errorMessage = _localisationService.GetText(
                    "ragfair-unable_to_find_item_in_inventory",
                    new
                    {
                        id = itemId
                    }
                );
                _logger.Error(errorMessage);

                return new GetItemsToListOnFleaFromInventoryResult
                {
                    Items = itemsToReturn,
                    ErrorMessage = errorMessage
                };
            }

            item = _itemHelper.FixItemStackCount(item);
            itemsToReturn.Add(_itemHelper.FindAndReturnChildrenAsItems(pmcData.Inventory.Items, itemId));
        }

        if (itemsToReturn?.Count == 0)
        {
            errorMessage = _localisationService.GetText("ragfair-unable_to_find_requested_items_in_inventory");
            _logger.Error(errorMessage);

            return new GetItemsToListOnFleaFromInventoryResult
            {
                ErrorMessage = errorMessage
            };
        }

        return new GetItemsToListOnFleaFromInventoryResult
        {
            Items = itemsToReturn,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Flag an offer as being ready for removal - sets expiry for very near future
    /// Will be picked up by update() once expiry time has passed
    /// </summary>
    /// <param name="offerId">Id of offer to remove</param>
    /// <param name="sessionId">Session id of requesting player</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse FlagOfferForRemoval(string offerId, string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var playerProfileOffers = pmcData.RagfairInfo.Offers;
        if (playerProfileOffers is null)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "ragfair-unable_to_remove_offer_not_found_in_profile",
                    new
                    {
                        profileId = sessionId,
                        offerId = offerId
                    }
                )
            );

            pmcData.RagfairInfo.Offers = [];
        }

        var playerOffer = playerProfileOffers?.FirstOrDefault(x => x.Id == offerId);
        if (playerOffer is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "ragfair-offer_not_found_in_profile",
                    new
                    {
                        offerId = offerId
                    }
                )
            );

            return _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("ragfair-offer_not_found_in_profile_short")
            );
        }

        // Only reduce time to end if time remaining is greater than what we would set it to
        var differenceInSeconds = playerOffer.EndTime - _timeUtil.GetTimeStamp();
        if (differenceInSeconds > _ragfairConfig.Sell.ExpireSeconds)
        {
            // `expireSeconds` Default is 71 seconds
            var newEndTime = _ragfairConfig.Sell.ExpireSeconds + _timeUtil.GetTimeStamp();
            playerOffer.EndTime = (long?) Math.Round((double) newEndTime);
        }

        return output;
    }

    /// <summary>
    /// Extend a flea offers active time
    /// </summary>
    /// <param name="extendRequest">Extend time request</param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>ItemEventRouterResponse</returns>
    public ItemEventRouterResponse ExtendOffer(ExtendOfferRequestData extendRequest, string sessionId)
    {
        var output = _eventOutputHolder.GetOutput(sessionId);

        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        var playerOffers = pmcData.RagfairInfo.Offers;
        var playerOfferIndex = playerOffers.FindIndex(offer => offer.Id == extendRequest.OfferId);
        var secondsToAdd = extendRequest.RenewalTime * TimeUtil.OneHourAsSeconds;

        if (playerOfferIndex == -1)
        {
            _logger.Warning(
                _localisationService.GetText(
                    "ragfair-offer_not_found_in_profile",
                    new
                    {
                        offerId = extendRequest.OfferId
                    }
                )
            );
            return _httpResponseUtil.AppendErrorToOutput(
                output,
                _localisationService.GetText("ragfair-offer_not_found_in_profile_short")
            );
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
                sellInOncePiece
            );

            var request = CreateBuyTradeRequestObject(CurrencyType.RUB, tax);
            _paymentService.PayMoney(pmcData, request, sessionId, output);
            if (output.Warnings.Count > 0)
            {
                return _httpResponseUtil.AppendErrorToOutput(
                    output,
                    _localisationService.GetText("ragfair-unable_to_pay_commission_fee")
                );
            }
        }

        // Add extra time to offer
        playerOffers[playerOfferIndex].EndTime += (long?) Math.Round((decimal) secondsToAdd);

        return output;
    }

    /// <summary>
    /// Create a basic trader request object with price and currency type
    /// </summary>
    /// <param name="currency">What currency: RUB, EURO, USD</param>
    /// <param name="value">Amount of currency</param>
    /// <returns>ProcessBuyTradeRequestData</returns>
    protected ProcessBuyTradeRequestData CreateBuyTradeRequestObject(CurrencyType currency, double value)
    {
        return new ProcessBuyTradeRequestData
        {
            TransactionId = "ragfair",
            Action = "TradingConfirm",
            SchemeItems =
            [
                new IdWithCount
                {
                    Id = new MongoId(_paymentHelper.GetCurrency(currency)),
                    Count = Math.Round(value)
                }
            ],
            Type = "",
            ItemId = "",
            Count = 0,
            SchemeId = 0
        };
    }

    /// <summary>
    /// Get prices for all items on flea
    /// </summary>
    /// <returns>Dictionary of tpl and item price</returns>
    public Dictionary<MongoId, double> GetAllFleaPrices()
    {
        return _ragfairPriceService.GetAllFleaPrices();
    }

    public Dictionary<MongoId, double> GetStaticPrices()
    {
        return _ragfairPriceService.GetAllStaticPrices();
    }

    public RagfairOffer? GetOfferByInternalId(string sessionId, GetRagfairOfferByIdRequest request)
    {
        var offers = _ragfairOfferService.GetOffers();
        var offerToReturn = offers.FirstOrDefault(offer => offer.InternalId == request.Id);

        return offerToReturn;
    }

    public record GetItemsToListOnFleaFromInventoryResult
    {
        public List<List<Item>>? Items
        {
            get;
            set;
        }

        public string? ErrorMessage
        {
            get;
            set;
        }
    }
}
