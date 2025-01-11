using Core.Annotations;
using Core.Models.Eft.Common.Tables;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class TraderAssortService
{
    public TraderAssort GetPristineTraderAssort(string traderId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Store trader assorts inside a class property
    /// </summary>
    /// <param name="traderId">Traderid to store assorts against</param>
    /// <param name="assort">Assorts to store</param>
    public void SetPristineTraderAssort(string traderId, TraderAssort assort)
    {
        throw new NotImplementedException();
    }
}
