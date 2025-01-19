using Core.Utils;
using Core.Utils.Cloners;

namespace Core.Models.Spt.Server;

public record ExhaustableArray<T>
{
    private readonly RandomUtil _randomUtil;
    private readonly ICloner _cloner;
    private List<T> pool;

    public ExhaustableArray(List<T> itemPool, RandomUtil randomUtil, ICloner cloner)
    {
        _randomUtil = randomUtil;
        _cloner = cloner;
        this.pool = _cloner.Clone(itemPool);
    }

    public T GetRandomValue()
    {
        if (pool == null || pool.Count == 0)
        {
            return default;
        }

        var index = _randomUtil.GetInt(0, pool.Count - 1);
        T toReturn = _cloner.Clone(pool[index]);
        pool.RemoveAt(index);

        return toReturn;
    }

    public T GetFirstValue()
    {
        if (pool == null || pool.Count == 0)
        {
            return default;
        }

        T toReturn = _cloner.Clone(pool[0]);
        pool.RemoveAt(0);
        return toReturn;
    }

    public bool HasValues()
    {
        return pool?.Count > 0;
    }
}
