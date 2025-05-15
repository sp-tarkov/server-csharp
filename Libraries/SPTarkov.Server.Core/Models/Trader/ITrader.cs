using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Services;

namespace SPTarkov.Server.Core.Models;

public interface ITrader
{
    public string Name { get; }
    public string Id { get; }
}

public abstract record ICustomTrader : ITrader
{
    public abstract string Name { get; }
    public abstract string Id { get; }
    public abstract TraderAssort? GetAssort();
    public abstract Dictionary<string, Dictionary<string, string>>? GetQuestAssort();
    public abstract TraderBase? GetBase();

    public virtual List<Suit>? GetSuits()
    {
        return null;
    }

    public virtual List<TraderServiceModel>? GetServices()
    {
        return null;
    }

    public virtual Dictionary<string, List<string>?>? GetDialogues()
    {
        return null;
    }
}
