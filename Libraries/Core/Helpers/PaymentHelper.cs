using SptCommon.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;

namespace Core.Helpers;

[Injectable]
public class PaymentHelper(ConfigServer _configServer)
{
    protected InventoryConfig _inventoryConfig = _configServer.GetConfig<InventoryConfig>();
    protected List<string> _moneyTpls = [Money.DOLLARS, Money.EUROS, Money.ROUBLES, Money.GP];
    protected bool _addedCustomMoney;

    /// <summary>
    /// Is the passed in tpl money (also checks custom currencies in inventoryConfig.customMoneyTpls)
    /// </summary>
    /// <param name="tpl"></param>
    /// <returns></returns>
    public bool IsMoneyTpl(string tpl)
    {
        if (!_addedCustomMoney)
        {
            _moneyTpls.AddRange(_inventoryConfig.CustomMoneyTpls);
            _addedCustomMoney = true;
        }

        return _moneyTpls.Contains(tpl);
    }

    /// <summary>
    /// Gets currency TPL from TAG
    /// </summary>
    /// <param name="currency"></param>
    /// <returns></returns>
    public string GetCurrency(CurrencyType? currency)
    {
        return currency switch
        {
            CurrencyType.EUR => Money.EUROS,
            CurrencyType.USD => Money.DOLLARS,
            CurrencyType.RUB => Money.ROUBLES,
            CurrencyType.GP => Money.GP,
            _ => ""
        };
    }
}
