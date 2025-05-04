using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class MathUtil
{
    /// <summary>
    ///     Helper to create the sum of all list elements
    /// </summary>
    /// <param name="values">List of floats to sum</param>
    /// <returns>sum of all values</returns>
    public double ListSum(List<double> values)
    {
        // Sum the list starting with an initial value of 0
        return values.Sum();
    }

    /// <summary>
    ///     Helper to create the cumulative sum of all list elements
    ///     ListCumSum([1, 2, 3, 4]) = [1, 3, 6, 10]
    /// </summary>
    /// <param name="values">The list with numbers of which to calculate the cumulative sum</param>
    /// <returns>cumulative sum of values</returns>
    public List<double> ListCumSum(List<double> values)
    {
        if (values.Count == 0)
        {
            return [];
        }

        var cumSumArray = new double[values.Count];
        cumSumArray[0] = values[0];

        for (var i = 1; i < values.Count; i++)
        {
            cumSumArray[i] = cumSumArray[i - 1] + values[i];
        }

        return [.. cumSumArray];
    }

    /// <summary>
    ///     Helper to create the product of each element times factor
    /// </summary>
    /// <param name="values">The list of numbers which shall be multiplied by the factor</param>
    /// <param name="factor">Number to multiply each element by</param>
    /// <returns>A list of elements all multiplied by the factor</returns>
    public List<double> ListProduct(List<double> values, double factor)
    {
        return values.Select(v => v * factor).ToList();
    }

    /// <summary>
    ///     Helper to add a constant to all list elements
    /// </summary>
    /// <param name="values">The list of numbers to which the summand should be added</param>
    /// <param name="additive"></param>
    /// <returns>A list of elements with the additive added to all elements</returns>
    public List<double> ListAdd(List<double> values, double additive)
    {
        return values.Select(v => v + additive).ToList();
    }

    /// <summary>
    ///     Maps a value from an input range to an output range linearly.
    ///     Example:
    ///     a_min = 0; a_max=1;
    ///     b_min = 1; b_max=3;
    ///     MapToRange(0.5, a_min, a_max, b_min, b_max) // returns 2
    /// </summary>
    /// <param name="x">The value from the input range to be mapped to the output range.</param>
    /// <param name="minIn">Minimum of the input range.</param>
    /// <param name="maxIn">Maximum of the input range.</param>
    /// <param name="minOut">Minimum of the output range.</param>
    /// <param name="maxOut">Maximum of the output range.</param>
    /// <returns>The result of the mapping.</returns>
    public double MapToRange(double x, double minIn, double maxIn, double minOut, double maxOut)
    {
        var deltaIn = maxIn - minIn;
        var deltaOut = maxOut - minOut;

        var xScale = (x - minIn) / deltaIn;
        return Math.Max(minOut, Math.Min(maxOut, minOut + xScale * deltaOut));
    }

    /// <summary>
    ///     Linear interpolation
    ///     e.g. used to do a continuous integration for quest rewards which are defined for specific support centers of pmcLevel
    /// </summary>
    /// <param name="xp">The point of x at which to interpolate</param>
    /// <param name="x">Support points in x (of same length as y)</param>
    /// <param name="y">Support points in y (of same length as x)</param>
    /// <returns>Interpolated value at xp, or null if xp is out of bounds</returns>
    public double? Interp1(double xp, List<double> x, List<double> y)
    {
        if (xp > x[^1]) // ^1 is the last index in C#
        {
            return y[^1];
        }

        if (xp < x[0])
        {
            return y[0];
        }

        for (var i = 0; i < x.Count - 1; i++)
        {
            if (xp >= x[i] && xp <= x[i + 1])
            {
                return y[i] + (xp - x[i]) * (y[i + 1] - y[i]) / (x[i + 1] - x[i]);
            }
        }

        return null;
    }
}
