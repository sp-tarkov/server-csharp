using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

/// <summary>
/// Source of truth for all default traders as well as any additional trader a server mod may add.
/// </summary>
[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.TraderRegistration)]
public class TraderStore : IOnLoad
{
    private readonly DatabaseService _databaseService;
    private readonly IEnumerable<ITrader> _injectedTraders;
    private readonly ISptLogger<TraderStore> _logger;

    private readonly Dictionary<string, ITrader> _traders = new();

    public TraderStore(DatabaseService databaseService,
        IEnumerable<ITrader> injectedTraders,
        ISptLogger<TraderStore> logger)
    {
        _databaseService = databaseService;
        _injectedTraders = injectedTraders;
        _logger = logger;
    }

    public Task OnLoad()
    {
        _logger.Info("Importing traders...");
        var customTraders = 0;

        foreach (var trader in _injectedTraders)
        {
            if (trader is ICustomTrader customTrader)
            {
                try
                {
                    var dbTrader = new Trader()
                    {
                        Assort = customTrader.GetAssort(),
                        Base = customTrader.GetBase(),
                        QuestAssort = customTrader.GetQuestAssort(),
                        Dialogue = customTrader.GetDialogues(),
                        Suits = customTrader.GetSuits(),
                        Services = customTrader.GetServices(),
                    };
                    _databaseService.GetTraders().Add(trader.Id, dbTrader);
                    _traders.Add(trader.Id, trader);
                    _logger.Info($"Loaded custom trader: {trader.Name}");
                    customTraders++;
                }
                catch (Exception e)
                {
                    _logger.Error("Failed to load custom trader", e);
                }
            }
            else
            {
                _traders.Add(trader.Id, trader);
            }
        }

        _logger.Info($"Importing traders complete {(customTraders == 0 ? "" : $"[{customTraders} traders loaded]")}");
        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-trader-registration";
    }

    /// <summary>
    /// Returns a trader by given ID.
    /// </summary>
    /// <param name="traderId"></param>
    /// <returns></returns>
    public ITrader? GetTraderById(string traderId)
    {
        if (_traders.TryGetValue(traderId, out var trader))
        {
            return trader;
        }

        return null;
    }

    /// <summary>
    /// Returns all traders in the game, including custom traders.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ITrader> GetAllTraders()
    {
        return _traders.Values;
    }
}
