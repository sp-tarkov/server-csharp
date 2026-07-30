using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Tables;

public class TradersTable : Dictionary<MongoId, Trader>
{
    public Trader? GetTrader(MongoId traderId)
    {
        return ContainsKey(traderId) ? this[traderId] : null;
    }
}
