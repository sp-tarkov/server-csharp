using Core.Annotations;

namespace Core.Helpers;

[Injectable]
public class PaymentHelper
{
    /// <summary>
    /// Is the passed in tpl money (also checks custom currencies in inventoryConfig.customMoneyTpls)
    /// </summary>
    /// <param name="tpl"></param>
    /// <returns></returns>
    public bool IsMoneyTpl(string tpl)
    {
        throw new NotImplementedException();
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
