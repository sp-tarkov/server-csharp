using SptCommon.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Helpers;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class TraderAssortService(
    TraderHelper _traderHelper)
{
    protected readonly Dictionary<string, TraderAssort> _pristineTraderAssorts = new();

    public TraderAssort? GetPristineTraderAssort(string traderId)
    {
        return _traderHelper.GetTraderAssortsByTraderId(traderId);
        _pristineTraderAssorts.TryGetValue(traderId, out var result);

        return result;
    }

    /// <summary>
    /// Store trader assorts inside a class property
    /// </summary>
    /// <param name="traderId">Trader id to store assorts against</param>
    /// <param name="assort">Assorts to store</param>
    public void SetPristineTraderAssort(string traderId, TraderAssort assort)
    {
        // TODO: remove
        //_pristineTraderAssorts[traderId] = assort;
    }
}
