using Core.Utils.Cloners;

namespace Core.Utils.Collections;

public record ExhaustableArray<T> : IExhaustableArray<T>
{
    private readonly ICloner _cloner;
    private readonly RandomUtil _randomUtil;
    private readonly LinkedList<T>? pool;

    public ExhaustableArray(
        T[]? itemPool,
        RandomUtil randomUtil,
        ICloner cloner
    ) : this(new LinkedList<T>(itemPool ?? []), randomUtil, cloner)
    {
    }

    public ExhaustableArray(
        LinkedList<T>? itemPool,
        RandomUtil randomUtil,
        ICloner cloner
    )
    {
        _cloner = cloner;
        _randomUtil = randomUtil;
        pool = cloner.Clone(itemPool ?? []);
    }

    public ExhaustableArray(
        ICollection<T>? itemPool,
        RandomUtil randomUtil,
        ICloner cloner
    ) : this(new LinkedList<T>(itemPool ?? []), randomUtil, cloner)
    {
    }

    public T? GetRandomValue()
    {
        if (pool?.Count == 0)
        {
            return default;
        }

        var index = _randomUtil.GetInt(0, pool.Count - 1);
        var element = pool.ElementAt(index);
        pool.Remove(element);
        return _cloner.Clone(element);
    }

    public T? GetFirstValue()
    {
        if (pool?.Count == 0)
        {
            return default;
        }

        var element = pool.ElementAt(0);
        pool.Remove(element);
        return _cloner.Clone(element);
    }

    public bool HasValues()
    {
        return pool?.Count != 0;
    }
}

public interface IExhaustableArray<T>
{
    T? GetRandomValue();
    T? GetFirstValue();
    bool HasValues();
}
