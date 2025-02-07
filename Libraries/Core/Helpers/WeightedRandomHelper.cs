using Core.Models.Spt.Helper;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace Core.Helpers;

[Injectable]
public class WeightedRandomHelper(
    ISptLogger<WeightedRandomHelper> _logger
)
{
    /// <summary>
    ///     Choose an item from the passed in array based on the weightings of each
    /// </summary>
    /// <param name="values">Items and weights to use</param>
    /// <returns>Chosen item from array</returns>
    public T GetWeightedValue<T>(Dictionary<T, double> values) where T : notnull
    {
        var itemKeys = values.Keys.ToList();
        var weights = values.Values.ToList();

        var chosenItem = WeightedRandom<T>(itemKeys, weights);

        return chosenItem.Item;
    }

    /// <summary>
    ///     Picks the random item based on its weight.
    ///     The items with higher weight will be picked more often (with a higher probability).
    ///     For example:
    ///     - items = ['banana', 'orange', 'apple']
    ///     - weights = [0, 0.2, 0.8]
    ///     - weightedRandom(items, weights) in 80% of cases will return 'apple', in 20% of cases will return
    ///     'orange' and it will never return 'banana' (because probability of picking the banana is 0%)
    /// </summary>
    /// <param name="items">List of items</param>
    /// <param name="weights">List of weights</param>
    /// <returns>Dictionary with item and index</returns>
    public WeightedRandomResult<T> WeightedRandom<T>(List<T> items, List<double> weights)
    {
        if (items.Count == 0)
        {
            _logger.Error("Items must not be empty");
        }

        if (weights.Count == 0)
        {
            _logger.Error("Item weights must not be empty");
        }

        if (items.Count != weights.Count)
        {
            _logger.Error("Items and weight inputs must be of the same length");
        }

        // Preparing the cumulative weights list.
        List<int> cumulativeWeights = [];
        for (var i = 0; i < weights.Count; i++)
        {
            cumulativeWeights.Add((int) weights[i] + (i > 0 ? cumulativeWeights[i - 1] : 0));
        }

        // Getting the random number in a range of [0...sum(weights)]
        var maxCumulativeWeight = cumulativeWeights[cumulativeWeights.Count - 1];
        var randomNumber = maxCumulativeWeight * new Random().NextDouble();

        // Picking the random item based on its weight.
        for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            if (cumulativeWeights[itemIndex] >= randomNumber)
            {
                return new WeightedRandomResult<T>
                {
                    Item = items[itemIndex],
                    Index = itemIndex
                };
            }
        }

        throw new InvalidOperationException("No item was picked.");
    }

    /// <summary>
    ///     Find the greated common divisor of all weights and use it on the passed in dictionary
    /// </summary>
    /// <param name="weightedDict">Values to reduce</param>
    public void ReduceWeightValues(Dictionary<string, double> weightedDict)
    {
        // No values, nothing to reduce
        if (weightedDict.Count == 0)
        {
            return;
        }

        // Only one value, set to 1 and exit
        if (weightedDict.Count == 1)
        {
            var kvp = weightedDict.FirstOrDefault();
            weightedDict[kvp.Key] = 1;

            return;
        }

        var weights = weightedDict.Values.ToList();
        var commonDivisor = CommonDivisor(weights);

        // No point in dividing by  1
        if (commonDivisor == 1)
        {
            return;
        }

        foreach (var kvp in weightedDict)
        {
            weightedDict[kvp.Key] /= commonDivisor;
        }
    }

    /**
     * Get the common divisor between all values in the passed in list and returns it
     */
    protected double CommonDivisor(List<double> numbers)
    {
        var result = numbers[0];
        for (var i = 1; i < numbers.Count; i++)
        {
            result = Gcd(result, numbers[i]);
        }

        return result;
    }

    protected double Gcd(double a, double b)
    {
        var x = a;
        var y = b;
        while (y != 0)
        {
            var temp = y;
            y = x % y;
            x = temp;
        }

        return x;
    }
}
