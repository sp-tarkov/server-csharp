using System.Numerics;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public sealed class MathUtil
{
    /// <summary>
    ///     Helper to add a constant to all list elements
    /// </summary>
    /// <param name="values">The list of numbers to which the summand should be added</param>
    /// <param name="additive"></param>
    /// <returns>A list of elements with the additive added to all elements</returns>
    public IEnumerable<double> ListAdd(IEnumerable<double> values, double additive)
    {
        return values.Select(v => v + additive);
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

        return Math.Clamp(minOut + xScale * deltaOut, minOut, maxOut);
    }

    /// <summary>
    ///     Linear interpolation
    ///     e.g. used to do a continuous integration for quest rewards which are defined for specific support centers of pmcLevel
    /// </summary>
    /// <param name="xp">The point of x at which to interpolate</param>
    /// <param name="x">Support points in x (of same length as y)</param>
    /// <param name="y">Support points in y (of same length as x)</param>
    /// <returns>Interpolated value at xp, or null if xp is out of bounds</returns>
    public T? Interp1<T>(T xp, IReadOnlyList<T> x, IReadOnlyList<T> y)
        where T : INumber<T>
    {
        if (xp > x[^1]) // ^1 is the last index in C#
        {
            // Value is above max provided value in x array, Clamp result to last value
            return y[^1];
        }

        if (xp < x[0])
        {
            // Value is below min provided value in x array, Clamp result to first value
            return y[0];
        }

        for (var i = 0; i < x.Count - 1; i++)
        {
            if (xp >= x[i] && xp <= x[i + 1])
            {
                return y[i] + (xp - x[i]) * (y[i + 1] - y[i]) / (x[i + 1] - x[i]);
            }
        }

        return default;
    }
}
