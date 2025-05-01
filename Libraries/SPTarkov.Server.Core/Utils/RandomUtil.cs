using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Utils;

// TODO: Finish porting this class
[Injectable(InjectionType.Singleton)]
public class RandomUtil(ISptLogger<RandomUtil> _logger, ICloner _cloner)
{
    private const int DecimalPointRandomPrecision = 6;

    /// <summary>
    ///     The IEEE-754 standard for double-precision floating-point numbers limits the number of digits (including both
    ///     integer + fractional parts) to about 15–17 significant digits. 15 is a safe upper bound, so we'll use that.
    /// </summary>
    public const int MaxSignificantDigits = 15;

    private static readonly int DecimalPointRandomPrecisionMultiplier = (int) Math.Pow(10, DecimalPointRandomPrecision);
    public readonly Random Random = new();

    /// <summary>
    ///     Generates a random integer between the specified minimum and maximum values, inclusive.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (optional).</param>
    /// <param name="exclusive">If max is exclusive or not.</param>
    /// <returns>A random integer between the specified minimum and maximum values.</returns>
    public int GetInt(int min, int max = int.MaxValue, bool exclusive = false)
    {
        // Prevents a potential integer overflow.
        if (exclusive && max == int.MaxValue)
        {
            max -= 1;
        }

        return max > min ? Random.Next(min, exclusive ? max : max + 1) : min;
    }

    /// <summary>
    ///     Generates a random floating-point number within the specified range ~15-17 digits (8 bytes).
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (exclusive).</param>
    /// <returns>A random floating-point number between `min` (inclusive) and `max` (exclusive).</returns>
    public double GetDouble(double min, double max)
    {
        var realMin = (long) (min * DecimalPointRandomPrecisionMultiplier);
        var realMax = (long) (max * DecimalPointRandomPrecisionMultiplier);

        return Math.Round(
            Random.NextInt64(realMin, realMax) / (double) DecimalPointRandomPrecisionMultiplier,
            DecimalPointRandomPrecision
        );
    }

    /// <summary>
    ///     Generates a random boolean value.
    /// </summary>
    /// <returns>A random boolean value, where the probability of `true` and `false` is approximately equal.</returns>
    public bool GetBool()
    {
        return Random.Next(0, 2) == 1;
    }

    /// <summary>
    ///     Calculates the percentage of a given number and returns the result.
    /// </summary>
    /// <param name="percent">The percentage to calculate.</param>
    /// <param name="number">The number to calculate the percentage of.</param>
    /// <param name="toFixed">The number of decimal places to round the result to (default is 2).</param>
    /// <returns>The calculated percentage of the given number, rounded to the specified number of decimal places.</returns>
    public double GetPercentOfValue(double percent, double number, int toFixed = 2)
    {
        var num = percent * (number / 100);

        return Math.Round(num, toFixed);
    }

    /// <summary>
    ///     Reduces a given number by a specified percentage.
    /// </summary>
    /// <param name="number">The original number to be reduced.</param>
    /// <param name="percentage">The percentage by which to reduce the number.</param>
    /// <returns>The reduced number after applying the percentage reduction.</returns>
    public double ReduceValueByPercent(double number, double percentage)
    {
        var reductionAmount = number * percentage / 100;

        return number - reductionAmount;
    }

    /// <summary>
    ///     Determines if a random event occurs based on the given chance percentage.
    /// </summary>
    /// <param name="chancePercent">The percentage chance (0-100) that the event will occur.</param>
    /// <returns>`true` if the event occurs, `false` otherwise.</returns>
    public bool GetChance100(double? chancePercent)
    {
        chancePercent = Math.Clamp(chancePercent ?? 0, 0D, 100D);

        return GetInt(0, 100) <= chancePercent;
    }

    /// <summary>
    ///     Returns a random string from the provided collection of strings.
    ///     This method is separate from GetCollectionValue so we can use a generic inference with GetCollectionValue.
    /// </summary>
    /// <param name="collection">The collection of strings to select a random value from.</param>
    /// <returns>A randomly selected string from the array.</returns>
    public T GetCollectionValue<T>(IEnumerable<T> collection)
    {
        // We can call `count` directly if it's a list
        if (collection is IList<T> list)
        {
            if (!list.Any())
            {
                throw new InvalidOperationException("Sequence contains no elements.");
            }

            return list[GetInt(0, list.Count - 1)];
        }

        var count = collection.Count(); // Run query and count elements
        if (count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        return collection.ElementAt(GetInt(0, count - 1));
    }

    /// <summary>
    ///     Gets a random key from the given dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve a key.</param>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TVal">Type of Value</typeparam>
    /// <returns>A random TKey representing one of the keys of the dictionary.</returns>
    public TKey GetKey<TKey, TVal>(Dictionary<TKey, TVal> dictionary) where TKey : notnull
    {
        return GetCollectionValue(dictionary.Keys);
    }

    /// <summary>
    ///     Gets a random val from the given dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary from which to retrieve a value.</param>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TVal">Type of Value</typeparam>
    /// <returns>A random TVal representing one of the values of the dictionary.</returns>
    public TVal GetVal<TKey, TVal>(Dictionary<TKey, TVal> dictionary) where TKey : notnull
    {
        return GetCollectionValue(dictionary.Values);
    }

    /// <summary>
    ///     Generates a normally distributed random number using the Box-Muller transform.
    /// </summary>
    /// <param name="mean">The mean (μ) of the normal distribution.</param>
    /// <param name="sigma">The standard deviation (σ) of the normal distribution.</param>
    /// <param name="attempt">The current attempt count to generate a valid number (default is 0).</param>
    /// <returns>A normally distributed random number.</returns>
    /// <remarks>
    ///     This function uses the Box-Muller transform to generate a normally distributed random number.
    ///     If the generated number is less than 0, it will recursively attempt to generate a valid number up to 100 times.
    ///     If it fails to generate a valid number after 100 attempts, it will return a random float between 0.01 and twice the mean.
    /// </remarks>
    public double GetNormallyDistributedRandomNumber(double mean, double sigma, int attempt = 0)
    {
        double u, v;

        do
        {
            u = GetSecureRandomNumber();
        } while (u == 0);

        do
        {
            v = GetSecureRandomNumber();
        } while (v == 0);

        // Apply the Box-Muller transform
        var w = Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
        var valueDrawn = mean + w * sigma;

        // Check if the generated value is valid
        if (valueDrawn < 0)
        {
            return attempt > 100
                ? GetDouble(0.01D, mean * 2D)
                : GetNormallyDistributedRandomNumber(mean, sigma, attempt + 1);
        }

        return valueDrawn;
    }

    /// <summary>
    ///     Generates a random integer between the specified range.
    /// </summary>
    /// <param name="low">The lower bound of the range (inclusive).</param>
    /// <param name="high">The upper bound of the range (exclusive). If not provided, the range will be from 0 to `low`.</param>
    /// <returns>A random integer within the specified range.</returns>
    public int RandInt(int low, int? high = null)
    {
        // Return a random integer from 0 to low if high is not provided
        if (high is null)
        {
            return Random.Next(0, low);
        }

        // Return low directly when low and high are equal
        return low == high
            ? low
            : Random.Next(low, (int) high);
    }

    /// <summary>
    ///     Generates a random number between two given values with optional precision.
    /// </summary>
    /// <param name="val1">The first value to determine the range.</param>
    /// <param name="val2">The second value to determine the range. If not provided, 0 is used.</param>
    /// <param name="precision">
    ///     The number of decimal places to round the result to. Must be a positive integer between 0
    ///     and MaxSignificantDigits(15), inclusive. If not provided, precision is determined by the input values.
    /// </param>
    /// <returns></returns>
    public double RandNum(double val1, double val2 = 0, int precision = DecimalPointRandomPrecision)
    {
        if (!double.IsFinite(val1) || !double.IsFinite(val2))
        {
            throw new ArgumentException("RandNum() parameters 'value1' and 'value2' must be finite numbers.");
        }

        // Determine the range
        var min = Math.Min(val1, val2);
        var max = Math.Max(val1, val2);

        var realPrecision = (long) Math.Pow(10, precision);

        var minInt = (long) (min * realPrecision);
        var maxInt = (long) (max * realPrecision);

        return Math.Round(Random.NextInt64(minInt, maxInt) / (double) realPrecision, precision);
    }

    /// <summary>
    ///     Draws a specified number of random elements from a given list.
    /// </summary>
    /// <param name="originalList">The list to draw elements from.</param>
    /// <param name="count">The number of elements to draw. Defaults to 1.</param>
    /// <param name="replacement">Whether to draw with replacement. Defaults to true.</param>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <returns>A List containing the drawn elements.</returns>
    public List<T> DrawRandomFromList<T>(List<T> originalList, int count = 1, bool replacement = true)
    {
        var list = originalList;
        var drawCount = count;

        if (!replacement)
        {
            list = _cloner.Clone(originalList);
            // Adjust drawCount to avoid drawing more elements than available
            if (drawCount > list.Count)
            {
                drawCount = list.Count;
            }
        }

        var results = new List<T>();
        for (var i = 0; i < drawCount; i++)
        {
            var randomIndex = RandInt(list.Count);
            if (replacement)
            {
                results.Add(list[randomIndex]);
            }
            else
            {
                results.Add(list.Splice(randomIndex, 1)[0]);
            }
        }

        return results;
    }

    /// <summary>
    ///     Draws a specified number of random keys from a given dictionary.
    /// </summary>
    /// <param name="dict">The dictionary from which to draw keys.</param>
    /// <param name="count">The number of keys to draw. Defaults to 1.</param>
    /// <param name="replacement">Whether to draw with replacement. Defaults to true.</param>
    /// <typeparam name="TKey">The type of elements in keys</typeparam>
    /// <typeparam name="TVal">The type of elements in values</typeparam>
    /// <returns>A list of randomly drawn keys from the dictionary.</returns>
    public List<TKey> DrawRandomFromDict<TKey, TVal>(Dictionary<TKey, TVal> dict, int count = 1, bool replacement = true) where TKey : notnull
    {
        var keys = dict.Keys.ToList();
        var randomKeys = DrawRandomFromList(keys, count, replacement);
        return randomKeys;
    }

    /// <summary>
    ///     Generates a biased random number within a specified range.
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (inclusive).</param>
    /// <param name="shift">The bias shift to apply to the random number generation.</param>
    /// <param name="n">The number of iterations to use for generating a Gaussian random number.</param>
    /// <returns>A biased random number within the specified range.</returns>
    public double GetBiasedRandomNumber(double min, double max, double shift, double n)
    {
        // This function generates a random number based on a gaussian distribution with an option to add a bias via shifting.

        // Here's an example graph of how the probabilities can be distributed:
        // https://www.boost.org/doc/libs/1_49_0/libs/math/doc/sf_and_dist/graphs/normal_pdf.png

        // Our parameter 'n' is sort of like σ (sigma) in the example graph.

        // An 'n' of 1 means all values are equally likely. Increasing 'n' causes numbers near the edge to become less likely.
        // By setting 'shift' to whatever 'max' is, we can make values near 'min' very likely, while values near 'max' become extremely unlikely.

        // Here's a place where you can play around with the 'n' and 'shift' values to see how the distribution changes:
        // http://jsfiddle.net/e08cumyx/

        if (max < min)
        {
            _logger.Error($"Invalid argument, Bounded random number generation max is smaller than min({max} < {min}");
            return -1;
        }

        if (n < 1)
        {
            _logger.Error($"Invalid argument, 'n' must be 1 or greater(received {n})");
            return -1;
        }

        if (min == max)
        {
            return min;
        }

        if (shift > max - min)
        {
            // If a rolled number is out of bounds (due to bias being applied), we roll it again.
            // As the shifting increases, the chance of rolling a number within bounds decreases.
            // A shift that is equal to the available range only has a 50% chance of rolling correctly, theoretically halving performance.
            // Shifting even further drops the success chance very rapidly - so we want to warn against that

            _logger.Warning(
                "Bias shift for random number generation is greater than the range of available numbers. This will have a severe performance impact"
            );
            _logger.Warning($"min-> {min}; max-> {max}; shift-> {shift}");
        }


        var biasedMin = shift >= 0 ? min - shift : min;
        var biasedMax = shift < 0 ? max + shift : max;

        double num;
        do
        {
            num = GetBoundedGaussian(biasedMin, biasedMax, n);
        } while (num < min || num > max);

        return num;
    }

    private double GetBoundedGaussian(double start, double end, double n)
    {
        return Math.Round(start + GetGaussianRandom(n) * (end - start + 1));
    }

    private double GetGaussianRandom(double n)
    {
        var rand = 0d;
        for (var i = 0; i < n; i += 1)
        {
            rand += GetSecureRandomNumber();
        }

        return rand / n;
    }

    /// <summary>
    ///     Shuffles a list in place using the Fisher-Yates algorithm.
    /// </summary>
    /// <param name="originalList">The list to shuffle.</param>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <returns>The shuffled list.</returns>
    public List<T> Shuffle<T>(List<T> originalList)
    {
        var currentIndex = originalList.Count;

        while (currentIndex != 0)
        {
            var randomIndex = GetInt(0, currentIndex);
            currentIndex--;

            // Swap it with the current element.
            (originalList[currentIndex], originalList[randomIndex]) = (originalList[randomIndex], originalList[currentIndex]);
        }

        return originalList;
    }

    /// <summary>
    ///     Generates a secure random number between 0 (inclusive) and 1 (exclusive).
    ///     This method uses the `crypto` module to generate a 48-bit random integer,
    ///     which is then divided by the maximum possible 48-bit integer value to
    ///     produce a floating-point number in the range [0, 1).
    /// </summary>
    /// <returns>A secure random number between 0 (inclusive) and 1 (exclusive).</returns>
    private double GetSecureRandomNumber()
    {
        return Random.NextSingle();
    }

    /// <summary>
    ///     Determines the number of decimal places in a number.
    /// </summary>
    /// <param name="num">The number to analyze.</param>
    /// <returns>The number of decimal places, or 0 if none exist.</returns>
    public int GetNumberPrecision(double num)
    {
        var preciseNum = (decimal) num;
        var factor = 0;
        while ((double) (preciseNum % 1) > double.Epsilon)
        {
            preciseNum *= 10M;
            factor++;
        }

        return factor;
    }

    public T? GetArrayValue<T>(IEnumerable<T> list)
    {
        return GetCollectionValue(list);
    }
}
