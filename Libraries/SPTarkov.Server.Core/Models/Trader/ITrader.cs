using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Services;

namespace SPTarkov.Server.Core.Models;

public interface ITrader
{
    public string Name { get; }
    public MongoId Id { get; }
}

public abstract record ICustomTrader : ITrader
{
    public abstract string Name { get; }
    public abstract MongoId Id { get; }

    public abstract TraderAssort GetAssort();

    public abstract Dictionary<string, Dictionary<MongoId, MongoId>> GetQuestAssort();

    public abstract TraderBase GetBase();

    public virtual List<Suit>? GetSuits()
    {
        return null;
    }

    public virtual List<TraderServiceModel>? GetServices()
    {
        return null;
    }

    public virtual Dictionary<string, List<string>?> GetDialogues()
    {
        return null;
    }
}
