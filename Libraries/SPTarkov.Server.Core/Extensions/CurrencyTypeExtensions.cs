using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Extensions;

public static class CurrencyTypeExtensions
{
    /// <summary>
    ///     Gets currency TPL from TAG
    /// </summary>
    /// <param name="currency"></param>
    /// <returns>Tpl of currency</returns>
    public static MongoId GetCurrencyTpl(this CurrencyType currency)
    {
        return currency switch
        {
            CurrencyType.EUR => Money.EUROS,
            CurrencyType.USD => Money.DOLLARS,
            CurrencyType.RUB => Money.ROUBLES,
            CurrencyType.GP => Money.GP,
            _ => string.Empty,
        };
    }
}
