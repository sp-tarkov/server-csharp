using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace UnitTests.Mock;

[Injectable(TypeOverride = typeof(RandomUtil))]
public class MockRandomUtil(ISptLogger<RandomUtil> logger, ICloner cloner) : RandomUtil(logger, cloner)
{
    public override int GetInt(int min, int max = int.MaxValue, bool exclusive = false)
    {
        return min;
    }

    public override double GetDouble(double min, double max)
    {
        return min;
    }

    public override bool GetBool()
    {
        return true;
    }

    public override void NextBytes(Span<byte> bytes)
    {
        // TODO: No idea what this does
        base.NextBytes(bytes);
    }

    public override double GetPercentOfValue(double percent, double number, int toFixed = 2)
    {
        // TODO: No idea what this does
        return base.GetPercentOfValue(percent, number, toFixed);
    }

    public override double ReduceValueByPercent(double number, double percentage)
    {
        // TODO: No idea what this does
        return base.ReduceValueByPercent(number, percentage);
    }

    public override bool GetChance100(double? chancePercent)
    {
        return true;
    }

    public override T GetRandomElement<T>(IEnumerable<T> collection)
    {
        if (!collection.Any())
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        return collection.First();
    }

    public override TKey GetKey<TKey, TVal>(Dictionary<TKey, TVal> dictionary)
    {
        return GetRandomElement(dictionary.Keys);
    }

    public override TVal GetVal<TKey, TVal>(Dictionary<TKey, TVal> dictionary)
    {
        return GetRandomElement(dictionary.Values);
    }

    public override double GetNormallyDistributedRandomNumber(double mean, double sigma, int attempt = 0)
    {
        // TODO: No idea what to do with this
        return base.GetNormallyDistributedRandomNumber(mean, sigma, attempt);
    }

    public override int RandInt(int low, int? high = null)
    {
        return low;
    }

    public override double RandNum(double val1, double val2 = 0, int precision = 6)
    {
        return val1;
    }

    public override List<T> DrawRandomFromList<T>(List<T> originalList, int count = 1, bool replacement = true)
    {
        return originalList.Slice(0, count);
    }

    public override List<TKey> DrawRandomFromDict<TKey, TVal>(Dictionary<TKey, TVal> dict, int count = 1, bool replacement = true)
    {
        // TODO: derandomize
        return base.DrawRandomFromDict(dict, count, replacement);
    }

    public override double GetBiasedRandomNumber(double min, double max, double shift, double n)
    {
        return min;
    }

    public override List<T> Shuffle<T>(List<T> originalList)
    {
        return originalList;
    }

    public override int GetNumberPrecision(double num)
    {
        // TODO: derandomize
        return base.GetNumberPrecision(num);
    }

    public override T? GetArrayValue<T>(IEnumerable<T> list)
        where T : default
    {
        return GetRandomElement(list);
    }

    public override bool RollChance(double chance, double scale = 1)
    {
        return true;
    }
}
