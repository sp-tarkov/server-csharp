using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public class TraderTable(Dictionary<MongoId, Trader> traders) : Dictionary<MongoId, Trader>(traders)
{
    public Trader? GetTrader(MongoId traderId)
    {
        return ContainsKey(traderId) ? this[traderId] : null;
    }
}
