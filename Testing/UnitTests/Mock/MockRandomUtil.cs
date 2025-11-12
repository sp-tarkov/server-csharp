using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace UnitTests.Mock;

[Injectable(TypeOverride = typeof(RandomUtil))]
public class MockRandomUtil(ISptLogger<RandomUtil> logger, ICloner cloner) : RandomUtil(logger, cloner)
{
    public new int GetInt(int min, int max = int.MaxValue, bool exclusive = false)
    {
        return min;
    }

    public new double GetDouble(double min, double max)
    {
        return min;
    }

    public new bool GetBool()
    {
        return true;
    }

    public new void NextBytes(Span<byte> bytes)
    {
        // TODO: No idea what this does
        base.NextBytes(bytes);
    }

    public new double GetPercentOfValue(double percent, double number, int toFixed = 2)
    {
        // TODO: No idea what this does
        return base.GetPercentOfValue(percent, number, toFixed);
    }

    public new double ReduceValueByPercent(double number, double percentage)
    {
        // TODO: No idea what this does
        return base.ReduceValueByPercent(number, percentage);
    }

    public new bool GetChance100(double? chancePercent)
    {
        return true;
    }

    public new T GetRandomElement<T>(IEnumerable<T> collection)
    {
        if (!collection.Any())
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        return collection.First();
    }

    public new TKey GetKey<TKey, TVal>(Dictionary<TKey, TVal> dictionary)
    {
        return GetRandomElement(dictionary.Keys);
    }

    public new TVal GetVal<TKey, TVal>(Dictionary<TKey, TVal> dictionary)
    {
        return GetRandomElement(dictionary.Values);
    }

    public new double GetNormallyDistributedRandomNumber(double mean, double sigma, int attempt = 0)
    {
        // TODO: No idea what to do with this
        return base.GetNormallyDistributedRandomNumber(mean, sigma, attempt);
    }

    public new int RandInt(int low, int? high = null)
    {
        return low;
    }

    public new double RandNum(double val1, double val2 = 0, int precision = 6)
    {
        return val1;
    }

    public new List<T> DrawRandomFromList<T>(List<T> originalList, int count = 1, bool replacement = true)
    {
        return originalList.Slice(0, count);
    }

    public new List<TKey> DrawRandomFromDict<TKey, TVal>(Dictionary<TKey, TVal> dict, int count = 1, bool replacement = true)
    {
        // TODO: derandomize
        return base.DrawRandomFromDict(dict, count, replacement);
    }

    public new double GetBiasedRandomNumber(double min, double max, double shift, double n)
    {
        return min;
    }

    public new List<T> Shuffle<T>(List<T> originalList)
    {
        return originalList;
    }

    public new int GetNumberPrecision(double num)
    {
        // TODO: derandomize
        return base.GetNumberPrecision(num);
    }

    public new T? GetArrayValue<T>(IEnumerable<T> list)
    {
        return GetRandomElement(list);
    }

    public new bool RollChance(double chance, double scale = 1)
    {
        return true;
    }
}
