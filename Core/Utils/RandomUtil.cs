using System.Security.Cryptography;
using Core.Annotations;

namespace Core.Utils;

// TODO: Finish porting this class
[Injectable(InjectionType.Singleton)]
public class RandomUtil
{
    private readonly Random _random = new();
    
    /// <summary>
    /// The IEEE-754 standard for double-precision floating-point numbers limits the number of digits (including both
    /// integer + fractional parts) to about 15–17 significant digits. 15 is a safe upper bound, so we'll use that.
    /// </summary>
    public const int MaxSignificantDigits = 15;
    
    /// <summary>
    /// Generates a random integer between the specified minimum and maximum values, inclusive.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>A random integer between the specified minimum and maximum values.</returns>
    public int GetInt(int min, int max)
    {
        // Prevents a potential integer overflow.
        if (max == int.MaxValue)
        {
            max -= 1;
        }
        
        // maxVal is exclusive of the passed value, so add 1
        return max > min ? _random.Next(min, max + 1) : min;
    }

    /// <summary>
    /// Generates a random integer between 1 (inclusive) and the specified maximum value (exclusive).
    /// If the maximum value is less than or equal to 1, it returns 1.
    /// </summary>
    /// <param name="max">The upper bound (exclusive) for the random integer generation.</param>
    /// <returns>A random integer between 1 and max - 1, or 1 if max is less than or equal to 1.</returns>
    public int GetIntEx(int max)
    {
        return max > 2 ? _random.Next(1, max - 1) : 1;
    }

    /// <summary>
    /// Generates a random floating-point number within the specified range ~6-9 digits (4 bytes).
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (exclusive).</param>
    /// <returns>A random floating-point number between `min` (inclusive) and `max` (exclusive).</returns>
    public float GetFloat(float min, float max)
    {
        return (float)GetSecureRandomNumber() * (max - min) + min;
    }
    
    /// <summary>
    /// Generates a random floating-point number within the specified range ~15-17 digits (8 bytes).
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (exclusive).</param>
    /// <returns>A random floating-point number between `min` (inclusive) and `max` (exclusive).</returns>
    public double GetDouble(double min, double max)
    {
        return GetSecureRandomNumber() * (max - min) + min;
    }

    /// <summary>
    /// Generates a random boolean value.
    /// </summary>
    /// <returns>A random boolean value, where the probability of `true` and `false` is approximately equal.</returns>
    public bool GetBool()
    {
        return GetSecureRandomNumber() < 0.5;
    }

    /// <summary>
    /// Calculates the percentage of a given number and returns the result.
    /// </summary>
    /// <param name="percent">The percentage to calculate.</param>
    /// <param name="number">The number to calculate the percentage of.</param>
    /// <param name="toFixed">The number of decimal places to round the result to (default is 2).</param>
    /// <returns>The calculated percentage of the given number, rounded to the specified number of decimal places.</returns>
    public float GetPercentOfValue(float percent, float number, int toFixed = 2)
    {
        var num = percent * number / 100;
        
        return (float)Math.Round(num, toFixed);
    }

    /// <summary>
    /// Reduces a given number by a specified percentage.
    /// </summary>
    /// <param name="number">The original number to be reduced.</param>
    /// <param name="percentage">The percentage by which to reduce the number.</param>
    /// <returns>The reduced number after applying the percentage reduction.</returns>
    public float ReduceValueByPercent(float number, float percentage)
    {
        var reductionAmount = number * percentage / 100;
        
        return number - reductionAmount;
    }

    /// <summary>
    /// Determines if a random event occurs based on the given chance percentage.
    /// </summary>
    /// <param name="chancePercent">The percentage chance (0-100) that the event will occur.</param>
    /// <returns>`true` if the event occurs, `false` otherwise.</returns>
    public bool GetChance100(float chancePercent)
    {
        chancePercent = Math.Clamp(chancePercent, 0f, 100f);
        
        return GetIntEx(100) <= chancePercent;
    }

    /// <summary>
    /// Returns a random string from the provided collection of strings.
    ///
    /// This method is separate from GetCollectionValue so we can use a generic inference with GetCollectionValue.
    /// </summary>
    /// <param name="collection">The collection of strings to select a random value from.</param>
    /// <returns>A randomly selected string from the array.</returns>
    public string GetStringCollectionValue(IEnumerable<string> collection)
    {
        return collection.ElementAt(GetInt(0, collection.Count() - 1));
    }
    
    /// <summary>
    /// Returns a random type T from the provided collection of type T.
    /// </summary>
    /// <param name="collection">The collection to get the random element from</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <returns>A random element from the collection.</returns>
    /// <remarks>This was formerly getArrayValue() in the node server</remarks>
    public T GetCollectionValue<T>(IEnumerable<T> collection)
    {
        return collection.ElementAt(GetInt(0, collection.Count() - 1));
    }

    /// <summary>
    /// Gets a random key from the given dictionary
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
    /// Gets a random val from the given dictionary
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
    /// Generates a normally distributed random number using the Box-Muller transform.
    /// </summary>
    /// <param name="mean">The mean (μ) of the normal distribution.</param>
    /// <param name="sigma">The standard deviation (σ) of the normal distribution.</param>
    /// <param name="attempt">The current attempt count to generate a valid number (default is 0).</param>
    /// <returns>A normally distributed random number.</returns>
    /// <remarks>
    /// This function uses the Box-Muller transform to generate a normally distributed random number.
    /// If the generated number is less than 0, it will recursively attempt to generate a valid number up to 100 times.
    /// If it fails to generate a valid number after 100 attempts, it will return a random float between 0.01 and twice the mean.
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
                ? GetDouble(0.01f, mean * 2f)
                : GetNormallyDistributedRandomNumber(mean, sigma, attempt + 1);
        }

        return valueDrawn;
    }

    /// <summary>
    /// Generates a random integer between the specified range.
    /// </summary>
    /// <param name="low">The lower bound of the range (inclusive).</param>
    /// <param name="high">The upper bound of the range (exclusive). If not provided, the range will be from 0 to `low`.</param>
    /// <returns>A random integer within the specified range.</returns>
    public int RandInt(int low, int? high)
    {
        // Return a random integer from 0 to low if high is not provided
        if (high is null)
        {
            return _random.Next(0, low);
        }
        
        // Return low directly when low and high are equal
        return low == high
            ? low
            : _random.Next(low, (int)high);
    }

    /// <summary>
    /// Generates a random number between two given values with optional precision.
    /// </summary>
    /// <param name="val1">The first value to determine the range.</param>
    /// <param name="val2">The second value to determine the range. If not provided, 0 is used.</param>
    /// <param name="precision">
    /// The number of decimal places to round the result to. Must be a positive integer between 0
    /// and MaxSignificantDigits(15), inclusive. If not provided, precision is determined by the input values.
    /// </param>
    /// <returns></returns>
    public double RandNum(double val1, double val2 = 0, byte? precision = null)
    {
        if (!double.IsFinite(val1) || !double.IsFinite(val2))
        {
            throw new ArgumentException("RandNum() parameters 'value1' and 'value2' must be finite numbers.");
        }
        
        // Determine the range
        var min = Math.Min(val1, val2);
        var max = Math.Max(val1, val2);

        // Validate and adjust precision
        if (precision is not null)
        {
            if (precision > MaxSignificantDigits)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(precision), "Must be less than 16");
            }
            
            // Calculate the number of whole-number digits in the maximum absolute value of the range
            var maxAbsoluteValue = Math.Max(Math.Abs(min), Math.Abs(max));
            var wholeNumberDigits = (int)Math.Floor(Math.Log10(maxAbsoluteValue)) + 1;
            
            var maxAllowedPrecision = Math.Max(0, MaxSignificantDigits - wholeNumberDigits);

            if (precision > maxAllowedPrecision)
            {
                throw new ArgumentException(
                    $"RandNum() precision of {precision} exceeds the allowable precision ({maxAllowedPrecision}) for the given values."
                );
            }
        }
        
        var result = GetSecureRandomNumber() * (max - min) + min;
        
        // Determine effective precision
        var maxPrecision = Math.Max(GetNumberPrecision(val1), GetNumberPrecision(val2));
        var effectivePrecision = precision ?? maxPrecision;

        var factor = Math.Pow(2, effectivePrecision);
        
        return Math.Round(result * factor) / factor;
    }
    
    /// <summary>
    /// Generates a secure random number between 0 (inclusive) and 1 (exclusive).
    /// 
    /// This method uses the `crypto` module to generate a 48-bit random integer,
    /// which is then divided by the maximum possible 48-bit integer value to 
    /// produce a floating-point number in the range [0, 1).
    /// </summary>
    /// <returns>A secure random number between 0 (inclusive) and 1 (exclusive).</returns>
    private static double GetSecureRandomNumber()
    {
        var buffer = new byte[6];

        using var rng = RandomNumberGenerator.Create();
        
        // Fill buffer with random bytes
        rng.GetBytes(buffer);
        
        var integer = 0;
        for (var i = 0; i < 6; i++)
        {
            integer = (integer << 8) | buffer[i];
        }

        const ulong maxInt = 1UL << 48;
        
        return (double)Math.Abs(integer) / maxInt;
    }

    /// <summary>
    /// Determines the number of decimal places in a number.
    /// </summary>
    /// <param name="num">The number to analyze.</param>
    /// <returns>The number of decimal places, or 0 if none exist.</returns>
    public int GetNumberPrecision(double num)
    {
        var parts = num.ToString($"G{MaxSignificantDigits}").Split('.');
        
        return parts.Length > 1 
            ? parts[1].Length 
            : 0;
    }
}