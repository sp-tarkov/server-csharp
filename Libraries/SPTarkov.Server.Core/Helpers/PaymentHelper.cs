using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class PaymentHelper(ConfigServer configServer)
{
    protected bool AddedCustomMoney;
    protected readonly InventoryConfig InventoryConfig = configServer.GetConfig<InventoryConfig>();
    protected readonly HashSet<MongoId> MoneyTpls = [Money.DOLLARS, Money.EUROS, Money.ROUBLES, Money.GP];

    /// <summary>
    ///     Is the passed in tpl money (also checks custom currencies in inventoryConfig.customMoneyTpls)
    /// </summary>
    /// <param name="tpl">Item Tpl to check</param>
    /// <returns></returns>
    public bool IsMoneyTpl(MongoId tpl)
    {
        // Add custom currency first time this method is accessed
        if (!AddedCustomMoney)
        {
            foreach (var customMoney in InventoryConfig.CustomMoneyTpls)
            {
                MoneyTpls.Add(customMoney);
            }

            AddedCustomMoney = true;
        }

        return MoneyTpls.Contains(tpl);
    }
}
