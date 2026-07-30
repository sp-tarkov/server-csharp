using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class ModdedTraderCustomizationController(TradersTable traderTable)
{
    public ModdedTraderListResponse GetCustomizationSellerIds()
    {
        var customizationSellers = new ModdedTraderListResponse { ModdedTraders = [] };

        foreach (var trader in traderTable)
        {
            if (trader.Value.Base.CustomizationSeller!.Value && trader.Key != Traders.RAGMAN)
            {
                customizationSellers.ModdedTraders.Add(trader.Key);
            }
        }
        return customizationSellers;
    }
}
