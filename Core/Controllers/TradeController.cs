using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Ragfair;
using Core.Models.Eft.Trade;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Routers;
using Core.Servers;
using Core.Services;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class TradeController
{
    private readonly ILogger _logger;
    private readonly DatabaseService _databaseService;
    private readonly EventOutputHolder _eventOutputHolder;
    private readonly TradeHelper _tradeHelper;
    private readonly TimeUtil _timeUtil;
    private readonly HashUtil _hashUtil;
    private readonly ItemHelper _itemHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly RagfairOfferHelper _ragfairOfferHelper;
    private readonly TraderHelper _traderHelper;
    // private readonly RagfairServer _ragfairServer;
    private readonly HttpResponseUtil _httpResponseUtil;
    private readonly LocalisationService _localisationService;
    private readonly RagfairPriceService _ragfairPriceService;
    // private readonly MailSendService _mailSendService;
    private readonly ConfigServer _configServer;
    
    private readonly RagfairConfig _ragfairConfig;
    private readonly TraderConfig _traderConfig;

    public TradeController
    (
        ILogger logger,
        DatabaseService databaseService,
        EventOutputHolder eventOutputHolder,
        TradeHelper tradeHelper,
        TimeUtil timeUtil,
        HashUtil hashUtil,
        ItemHelper itemHelper,
        ProfileHelper profileHelper,
        RagfairOfferHelper ragfairOfferHelper,
        TraderHelper traderHelper,
        HttpResponseUtil httpResponseUtil,
        LocalisationService localisationService,
        RagfairPriceService ragfairPriceService,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _databaseService = databaseService;
        _eventOutputHolder = eventOutputHolder;
        _tradeHelper = tradeHelper;
        _timeUtil = timeUtil;
        _hashUtil = hashUtil;
        _itemHelper = itemHelper;
        _profileHelper = profileHelper;
        _ragfairOfferHelper = ragfairOfferHelper;
        _traderHelper = traderHelper;
        _httpResponseUtil = httpResponseUtil;
        _localisationService = localisationService;
        _ragfairPriceService = ragfairPriceService;
        _configServer = configServer;

        _ragfairConfig = _configServer.GetConfig<RagfairConfig>();
        _traderConfig = _configServer.GetConfig<TraderConfig>();
    }

    /// <summary>
    /// Handle TradingConfirm event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ConfirmTrading(
        PmcData pmcData,
        ProcessBaseTradeRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle RagFairBuyOffer event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse ConfirmRagfairTrading(
        PmcData pmcData,
        ProcessRagfairTradeRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Buy an item off the flea sold by a trader
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Offer being purchased</param>
    /// <param name="offerRequest">request data from client</param>
    /// <param name="output">Output to send back to client</param>
    private void BuyTraderItemFromRagfair(
        string sessionId,
        PmcData pmcData,
        RagfairOffer fleaOffer,
        OfferRequest offerRequest,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Buy an item off the flea sold by a PMC
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Player profile</param>
    /// <param name="fleaOffer">Offer being purchased</param>
    /// <param name="offerRequest">request data from client</param>
    /// <param name="output">Output to send back to client</param>
    private void BuyPmcItemFromRagfair(
        string sessionId,
        PmcData pmcData,
        RagfairOffer fleaOffer,
        OfferRequest offerRequest,
        ItemEventRouterResponse output)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is the provided offerid and ownerid from a player made offer
    /// </summary>
    /// <param name="offerId">id of the offer</param>
    /// <param name="offerOwnerId">Owner id</param>
    /// <returns>true if offer was made by a player</returns>
    private bool IsPlayerOffer(
        string offerId,
        string offerOwnerId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Does Player have necessary trader loyalty to purchase flea offer
    /// </summary>
    /// <param name="fleaOffer">Flea offer being bought</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>True if player can buy offer</returns>
    private bool PlayerLacksTraderLoyaltyLevelToBuyOffer(
        RagfairOffer fleaOffer,
        PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle SellAllFromSavage event
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="request"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public ItemEventRouterResponse SellScavItemsToFence(
        PmcData pmcData,
        SellScavItemsToFenceRequestData request,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send the specified rouble total to player as mail
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="roublesToSend">amount of roubles to send</param>
    /// <param name="trader">Trader to sell items to</param>
    private void MailMoneyToPlayer(
        string sessionId,
        int roublesToSend,
        string trader)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Looks up an items children and gets total handbook price for them
    /// </summary>
    /// <param name="parentItemId">parent item that has children we want to sum price of</param>
    /// <param name="items">All items (parent + children)</param>
    /// <param name="handbookPrices">Prices of items from handbook</param>
    /// <param name="traderDetails">Trader being sold to, to perform buy category check against</param>
    /// <returns>Rouble price</returns>
    private int GetPriceOfItemAndChildren(
        string parentItemId,
        List<Item> items,
        Dictionary<string, int> handbookPrices,
        TraderBase traderDetails)
    {
        throw new NotImplementedException();
    }
}
