using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostDBModLoader)]
public class TraderStore : IOnLoad
{
    private readonly DatabaseService _databaseService;
    private readonly ISptLogger<TraderStore> _logger;

    private readonly Dictionary<string, ITrader> _traders = new();

    public TraderStore(DatabaseService databaseService, ISptLogger<TraderStore> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public Task OnLoad()
    {
        _logger.Info("Importing traders...");
        var customTraders = 0;
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ITrader).IsAssignableFrom(type) && !type.IsAbstract)
            .ToList()
            .ForEach(type =>
            {
                if ((type.BaseType?.IsAbstract).GetValueOrDefault(false) && type.BaseType == typeof(ICustomTrader))
                {
                    try
                    {
                        var trader = (ICustomTrader)Activator.CreateInstance(type)!;
                        var dbTrader = new Trader()
                        {
                            Assort = trader.GetAssort(),
                            Base = trader.GetBase(),
                            QuestAssort = trader.GetQuestAssort(),
                            Dialogue = trader.GetDialogues(),
                            Suits = trader.GetSuits(),
                            Services = trader.GetServices(),
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
                    var trader = (ITrader)Activator.CreateInstance(type)!;
                    _traders.Add(trader.Id, trader);
                }
            });
        _logger.Info($"Importing traders complete {(customTraders == 0 ? "" : $"[{customTraders} traders loaded]")}");
        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-post-db-mods";
    }

    public ITrader? GetTrader(string traderId)
    {
        if (_traders.TryGetValue(traderId, out var trader))
        {
            return trader;
        }

        return null;
    }

    public IEnumerable<ITrader> GetAllTraders()
    {
        return _traders.Values;
    }
}
