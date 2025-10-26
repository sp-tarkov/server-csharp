using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class ModdedTraderCustomizationController(DatabaseService databaseService)
{
    public ModdedTraderListResponse GetCustomizationSellerIds()
    {
        var traders = databaseService.GetTraders();
        var customizationSellers = new ModdedTraderListResponse
        {
            ModdedTraders = []
        };

        foreach (var trader in traders)
        {
            if (trader.Value.Base.CustomizationSeller!.Value && trader.Key != Traders.RAGMAN)
            {
                customizationSellers.ModdedTraders.Add(trader.Key);
            }
        }
        return customizationSellers;
    }
}
