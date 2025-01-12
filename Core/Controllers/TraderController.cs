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
        return; // TODO: actually implement
    }

    /// <summary>
    /// Runs when onUpdate is fired
    /// If current time is > nextResupply(expire) time of trader, refresh traders assorts and
    /// Fence is handled slightly differently
    /// </summary>
    /// <returns></returns>
    public bool Update()
    {
        throw new NotImplementedException();
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
    private int SortByTraderId(TraderBase traderA, TraderBase traderB)
    {
        // if (traderA.Id > traderB.Id)
        //     return 1;
        //
        // if (traderA.Id < traderB.Id)
        //     return -1;
        //
        // return 0;
        throw new NotImplementedException();
        // TODO: implement me
    }

    /// <summary>
    /// Handle client/trading/api/getTrader
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="traderId"></param>
    /// <returns></returns>
    public TraderBase GetTrader(
        string sessionId,
        string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/trading/api/getTraderAssort
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="traderId"></param>
    /// <returns></returns>
    public TraderAssort GetAssort(
        string sessionId,
        string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/items/prices/TRADERID
    /// </summary>
    /// <returns></returns>
    public GetItemPricesResponse GetItemPrices(string sessionId, string traderId)
    {
        throw new NotImplementedException();
    }
}
