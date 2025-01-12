using Core.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Game;
using Core.Models.Eft.ItemEvent;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Controllers;

[Injectable]
public class TraderController
{
    private ILogger _logger;
    private TimeUtil _timeUtil;
    private DatabaseService _databaseService;
    private TraderAssortHelper _traderAssortHelper;
    private TraderAssortService _traderAssortService;
    private ProfileHelper _profileHelper;
    private TraderHelper _traderHelper;
    private PaymentHelper _paymentHelper;
    private RagfairPriceService _ragfairPriceService;
    private TraderPurchasePersisterService _traderPurchasePersisterService;
    private FenceService _fenceService;
    private FenceBaseAssortGenerator _fenceBaseAssortGenerator;
    private ConfigServer _configServer;
    private ICloner _cloner;

    private TraderConfig _traderConfig;

    public TraderController
    (
        ILogger logger,
        TimeUtil timeUtil,
        DatabaseService databaseService,
        TraderAssortHelper traderAssortHelper,
        TraderAssortService traderAssortService,
        ProfileHelper profileHelper,
        TraderHelper traderHelper,
        PaymentHelper paymentHelper,
        RagfairPriceService ragfairPriceService,
        TraderPurchasePersisterService traderPurchasePersisterService,
        FenceService fenceService,
        FenceBaseAssortGenerator fenceBaseAssortGenerator,
        ConfigServer configServer,
        ICloner cloner
    )
    {
        _logger = logger;
        _timeUtil = timeUtil;
        _databaseService = databaseService;
        _traderAssortHelper = traderAssortHelper;
        _traderAssortService = traderAssortService;
        _profileHelper = profileHelper;
        _traderHelper = traderHelper;
        _paymentHelper = paymentHelper;
        _ragfairPriceService = ragfairPriceService;
        _traderPurchasePersisterService = traderPurchasePersisterService;
        _fenceService = fenceService;
        _fenceBaseAssortGenerator = fenceBaseAssortGenerator;
        _configServer = configServer;
        _cloner = cloner;

        _traderConfig = configServer.GetConfig<TraderConfig>(ConfigTypes.TRADER);
    }

    /// <summary>
    /// Runs when onLoad event is fired
    /// Iterate over traders, ensure a pristine copy of their assorts is stored in traderAssortService
    /// Store timestamp of next assort refresh in nextResupply property of traders .base object
    /// </summary>
    public void Load()
    {
        var nextHourTimestamp = _timeUtil.GetTimeStampOfNextHour();
        var traderResetStartsWithServer = _traderConfig.TradersResetFromServerStart;

        var traders = _databaseService.GetTraders();
        foreach (var trader in traders)
        {
            if (trader.Key == "ragfair" || trader.Key == Traders.LIGHTHOUSEKEEPER)
                continue;

            if (trader.Key == Traders.FENCE)
            {
                _fenceBaseAssortGenerator.GenerateFenceBaseAssorts();
                _fenceService.GenerateFenceAssorts();
                continue;
            }

            // Adjust price by traderPriceMultipler config property
            if (_traderConfig.TraderPriceMultipler != 1)
            {
                foreach (var scheme in trader.Value?.Assort?.BarterScheme)
                {
                    var barterSchemeItem = scheme.Value[0][0];

                    if (barterSchemeItem != null && _paymentHelper.IsMoneyTpl(barterSchemeItem?.Template))
                    {
                        barterSchemeItem.Count += Math.Round((barterSchemeItem?.Count * _traderConfig?.TraderPriceMultipler) ?? 0D, 2);
                    }
                }
            }

            // Create dict of pristine trader assorts on server start
            if (_traderAssortService.GetPristineTraderAssort(trader.Key) != null)
            {
                var assortsClone = _cloner.Clone(trader.Value.Assort);
                _traderAssortService.SetPristineTraderAssort(trader.Key, assortsClone);
            }
            
            _traderPurchasePersisterService.RemoveStalePurchasesFromProfiles(trader.Key);
            
            // Set to next hour on clock or current time + 60 mins
            trader.Value.Base.NextResupply = traderResetStartsWithServer ? _traderHelper.GetNextUpdateTimestamp(trader.Value.Base.Id) : nextHourTimestamp;
        }
    }

    /// <summary>
    /// Runs when onUpdate is fired
    /// If current time is > nextResupply(expire) time of trader, refresh traders assorts and
    /// Fence is handled slightly differently
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {
        foreach (var trader in _databaseService.GetTables().Traders)
        {
            if (trader.Key == "ragfair" || trader.Key == Traders.LIGHTHOUSEKEEPER)
                continue;
            
            if (trader.Key == Traders.FENCE)
            {
                if (_fenceService.NeedsPartialRefresh())
                    _fenceService.GenerateFenceAssorts();
                
                continue;
            }

            // Trader needs to be refreshed
            if (_traderAssortHelper.TraderAssortsHaveExpired(trader.Key))
            {
                _traderAssortHelper.ResetExpiredTrader(trader.Value);
                
                // Reset purchase data per trader as they have independent reset times
                _traderPurchasePersisterService.ResetTraderPurchasesStoredInProfile(trader.Key);
            }
        }

        return true;
    }

    /// <summary>
    /// Handle client/trading/api/traderSettings
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <returns>Return a list of all traders</returns>
    public List<TraderBase> GetAllTraders(string sessionId)
    {
        var traders = new List<TraderBase>();
        var pmcData = _profileHelper.GetPmcProfile(sessionId);
        foreach (var trader in _databaseService.GetTables().Traders)
        {
            if (trader.Value.Base.Id == "ragfair")
                continue;

            traders.Add(_traderHelper.GetTrader(trader.Key, sessionId));

            if (pmcData?.Info != null)
                _traderHelper.LevelUp(trader.Key, pmcData);
        }

        // traders.Sort((a, b) => SortByTraderId(a, b));
        return traders;
    }

    /// <summary>
    /// Order traders by their traderId (Ttid)
    /// </summary>
    /// <param name="traderA">First trader to compare</param>
    /// <param name="traderB">Second trader to compare</param>
    /// <returns>1,-1 or 0</returns>
    private int SortByTraderId(
        TraderBase traderA,
        TraderBase traderB)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/trading/api/getTrader
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="traderId"></param>
    /// <returns></returns>
    public TraderBase GetTrader(string sessionId, string traderId)
    {
        return _traderHelper.GetTrader(sessionId, traderId);
    }

    /// <summary>
    /// Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="traderId"></param>
    /// <returns></returns>
    public TraderAssort GetAssort(string sessionId, string traderId)
    {
        return _traderAssortHelper.GetAssort(sessionId, traderId);
    }

    /// <summary>
    /// Handle client/items/prices/TRADERID
    /// </summary>
    /// <returns></returns>
    public GetItemPricesResponse GetItemPrices(string sessionId, string traderId)
    {
        var handbookPrices = _ragfairPriceService.GetAllStaticPrices();
        var handbookPricesClone = _cloner.Clone(handbookPrices);

        return new()
        {
            SupplyNextTime = _traderHelper.GetNextUpdateTimestamp(traderId),
            Prices = handbookPricesClone,
            CurrencyCourses = new Dictionary<string, double>()
            {
                { "5449016a4bdc2d6f028b456f", handbookPrices[Money.ROUBLES] },
                { "569668774bdc2da2298b4568", handbookPrices[Money.EUROS] },
                { "5696686a4bdc2da3298b456a", handbookPrices[Money.DOLLARS] }
            }
        };
    }
}
