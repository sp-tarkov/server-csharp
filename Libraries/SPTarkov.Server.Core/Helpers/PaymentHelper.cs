using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class PaymentHelper(ConfigServer configServer)
{
    protected bool _addedCustomMoney;
    protected readonly InventoryConfig _inventoryConfig = configServer.GetConfig<InventoryConfig>();
    protected readonly HashSet<MongoId> _moneyTpls = [Money.DOLLARS, Money.EUROS, Money.ROUBLES, Money.GP];

    /// <summary>
    ///     Is the passed in tpl money (also checks custom currencies in inventoryConfig.customMoneyTpls)
    /// </summary>
    /// <param name="tpl">Item Tpl to check</param>
    /// <returns></returns>
    public bool IsMoneyTpl(MongoId tpl)
    {
        // Add custom currency first time this method is accessed
        if (!_addedCustomMoney)
        {
            foreach (var customMoney in _inventoryConfig.CustomMoneyTpls)
            {
                _moneyTpls.Add(customMoney);
            }

            _addedCustomMoney = true;
        }

        return _moneyTpls.Contains(tpl);
    }
}
