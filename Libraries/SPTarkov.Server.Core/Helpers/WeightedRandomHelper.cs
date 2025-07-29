using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Helper;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class WeightedRandomHelper(
    ISptLogger<WeightedRandomHelper> logger,
    ServerLocalisationService localisationService,
    RandomUtil randomUtil
)
{
    /// <summary>
    ///     Choose an item from the passed in array based on the weightings of each
    /// </summary>
    /// <param name="values">Items and weights to use</param>
    /// <returns>Chosen item from array</returns>
    public T GetWeightedValue<T>(Dictionary<T, double> values)
        where T : notnull
    {
        if (values.Count == 1)
        {
            return values.Keys.First();
        }

        var itemKeys = values.Keys.ToList();
        var weights = values.Values.ToList();

        var chosenItem = WeightedRandom(itemKeys, weights);

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
    public WeightedRandomResult<T> WeightedRandom<T>(IList<T> items, IList<double> weights)
    {
        if (items.Count == 0)
        {
            logger.Error(localisationService.GetText("weightedrandomhelper-supplied_items_empty"));
        }

        if (weights.Count == 0)
        {
            logger.Error(localisationService.GetText("weightedrandomhelper-supplied_weights_empty"));
        }

        if (items.Count != weights.Count)
        {
            logger.Error(
                localisationService.GetText(
                    "weightedrandomhelper-supplied_data_doesnt_match",
                    new { itemCount = items.Count, weightCount = weights.Count }
                )
            );
        }

        // Preparing the cumulative weights list.
        var cumulativeWeights = new double[weights.Count];
        double sumOfWeights = 0;
        for (var i = 0; i < weights.Count; i++)
        {
            if (weights[i] < 0)
            {
                logger.Warning($"Weight at index: {i} is negative ({weights[i]}), skipping");
                continue;
            }

            sumOfWeights += weights[i];
            cumulativeWeights[i] = sumOfWeights;
        }

        if (sumOfWeights == weights.Count)
        {
            // Weights are all the same, early exit
            var randomIndex = randomUtil.GetInt(0, items.Count - 1);
            return new WeightedRandomResult<T> { Item = items[randomIndex], Index = randomIndex };
        }

        // Getting the random number in a range of [0...sum(weights)]
        var randomNumber = sumOfWeights * randomUtil.GetDouble(0, 1);

        // Picking the random item based on its weight.
        for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            if (cumulativeWeights[itemIndex] >= randomNumber)
            {
                return new WeightedRandomResult<T> { Item = items[itemIndex], Index = itemIndex };
            }
        }

        throw new InvalidOperationException("No item was picked.");
    }

    /// <summary>
    ///     Find the greatest common divisor of all weights and use it on the passed in dictionary
    /// </summary>
    /// <param name="weightedDict">Values to reduce</param>
    public void ReduceWeightValues(IDictionary<MongoId, double> weightedDict)
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

    /// <summary>
    /// Get the common divisor between all values from provided list and return it
    /// </summary>
    /// <param name="numbers">Numbers to get common divisor of</param>
    /// <returns>Common divisor</returns>
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
