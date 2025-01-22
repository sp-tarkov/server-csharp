using SptCommon.Annotations;
using Core.Generators;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Game;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Controllers;

[Injectable]
public class TraderController(
    ISptLogger<TraderController> _logger,
    TimeUtil _timeUtil,
    DatabaseService _databaseService,
    TraderAssortHelper _traderAssortHelper,
    TraderAssortService _traderAssortService,
    ProfileHelper _profileHelper,
    TraderHelper _traderHelper,
    PaymentHelper _paymentHelper,
    RagfairPriceService _ragfairPriceService,
    TraderPurchasePersisterService _traderPurchasePersisterService,
    FenceService _fenceService,
    FenceBaseAssortGenerator _fenceBaseAssortGenerator,
    ConfigServer _configServer,
    ICloner _cloner
)
{
    protected TraderConfig _traderConfig = _configServer.GetConfig<TraderConfig>();

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
                foreach (var kvp in trader.Value?.Assort?.BarterScheme)
                {
                    var barterSchemeItem = kvp.Value[0][0];

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
        foreach (var (traderId, data) in _databaseService.GetTables().Traders)
        {
            switch (traderId)
            {
                case Traders.LIGHTHOUSEKEEPER:
                    continue;
                case Traders.FENCE:
                {
                    _logger.Warning($"commented out Fence refresh as not implemented so we can get further");
                    // if (_fenceService.NeedsPartialRefresh())
                    //     _fenceService.GenerateFenceAssorts();

                    continue;
                }
            }

            // Trader needs to be refreshed
            if (_traderAssortHelper.TraderAssortsHaveExpired(traderId))
            {
                _traderAssortHelper.ResetExpiredTrader(data);

                // Reset purchase data per trader as they have independent reset times
                _traderPurchasePersisterService.ResetTraderPurchasesStoredInProfile(traderId);
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
        foreach (var (traderId, data) in _databaseService.GetTables().Traders)
        {
            traders.Add(_traderHelper.GetTrader(traderId, sessionId));

            if (pmcData?.Info != null)
                _traderHelper.LevelUp(traderId, pmcData);
        }

        traders.Sort(SortByTraderId);
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
        return string.CompareOrdinal(traderA.Id, traderB.Id);
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
