using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;

namespace Core.Helpers;

[Injectable]
public class PaymentHelper
{
    private readonly ConfigServer _configServer;
    private readonly InventoryConfig _inventoryConfig;

    public PaymentHelper(
        ConfigServer configServer)
    {
        _configServer = configServer;

        _inventoryConfig = _configServer.GetConfig<InventoryConfig>();
    }

    /// <summary>
    /// Is the passed in tpl money (also checks custom currencies in inventoryConfig.customMoneyTpls)
    /// </summary>
    /// <param name="tpl"></param>
    /// <returns></returns>
    public bool IsMoneyTpl(string tpl)
    {
        var moneyTypes = new List<string>
        {
            Money.DOLLARS,
            Money.ROUBLES,
            Money.GP,

        };
        moneyTypes.AddRange(_inventoryConfig.CustomMoneyTpls);

        return moneyTypes.Contains(tpl);
    }

    /// <summary>
    /// Gets currency TPL from TAG
    /// </summary>
    /// <param name="currency"></param>
    /// <returns></returns>
    public string GetCurrency(string currency)
    {
        throw new NotImplementedException();
    }
}
