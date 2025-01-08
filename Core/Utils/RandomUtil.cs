using System.Security.Cryptography;
using Core.Annotations;

namespace Core.Utils;

// TODO: Finish porting this class
[Injectable(InjectionType.Singleton)]
public class RandomUtil
{
    private readonly Random _random = new();
    
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
    /// Generates a random floating-point number within the specified range.
    /// </summary>
    /// <param name="min">The minimum value of the range (inclusive).</param>
    /// <param name="max">The maximum value of the range (exclusive).</param>
    /// <returns>A random floating-point number between `min` (inclusive) and `max` (exclusive).</returns>
    public float GetFloat(float min, float max)
    {
        return (float)GetSecureRandomNumber() * (max - min) + min;
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
    /// Returns a random string from the provided collection of strings.
    /// </summary>
    /// <param name="collection"></param>
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
        
        Console.WriteLine(integer);
        
        return (double)Math.Abs(integer) / maxInt;
    }

    /// <summary>
    /// Determines the number of decimal places in a number.
    /// </summary>
    /// <param name="num">The number to analyze.</param>
    /// <returns>The number of decimal places, or 0 if none exist.</returns>
    private static int GetNumberPrecision(double num)
    {
        return num.ToString().Split('.')[1]?.Length ?? 0;
    }
}