using Core.Annotations;
using Core.Models.Spt.Helper;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Helpers;

[Injectable]
public class WeightedRandomHelper
{
    private readonly ILogger _logger;

    public WeightedRandomHelper(
        ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Choos an item from the passed in array based on the weightings of each
    /// </summary>
    /// <param name="itemArray">Items and weights to use</param>
    /// <returns>Chosen item from array</returns>
    public T GetWeightedValue<T>(Dictionary<T, int> itemArray)
    {
        var itemKeys = itemArray.Keys.ToList();
        var weights = itemArray.Values.ToList();

        var chosenItem = WeightedRandom<T>(itemKeys, weights);

        return chosenItem.Item;
        // SORRY IF THIS BLEW UP, I DONT SEE A REASON ITS GENERIC - CWX
    }

    /// <summary>
    /// Picks the random item based on its weight.
    /// The items with higher weight will be picked more often (with a higher probability).
    /// 
    /// For example:
    /// - items = ['banana', 'orange', 'apple']
    /// - weights = [0, 0.2, 0.8]
    /// - weightedRandom(items, weights) in 80% of cases will return 'apple', in 20% of cases will return
    /// 'orange' and it will never return 'banana' (because probability of picking the banana is 0%)
    /// 
    /// </summary>
    /// <param name="items">List of items</param>
    /// <param name="weights">List of weights</param>
    /// <returns>Dictionary with item and index</returns>
    public WeightedRandomResult<T> WeightedRandom<T>(List<T> items, List<int> weights)
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
            cumulativeWeights.Add(weights[i] + (i > 0 ? cumulativeWeights[i - 1] : 0));
        }

        // Getting the random number in a range of [0...sum(weights)]
        int maxCumulativeWeight = cumulativeWeights[cumulativeWeights.Count - 1];
        double randomNumber = maxCumulativeWeight * new Random().NextDouble();

        // Picking the random item based on its weight.
        for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            if (cumulativeWeights[itemIndex] >= randomNumber)
            {
                return new WeightedRandomResult<T>()
                {
                    Item = items[itemIndex],
                    Index = itemIndex,
                };
            }
        }

        throw new InvalidOperationException("No item was picked.");
    }

    /// <summary>
    /// Find the greated common divisor of all weights and use it on the passed in dictionary
    /// </summary>
    /// <param name="weightedDict">Values to reduce</param>
    public void ReduceWeightValues(Dictionary<string, double> weightedDict)
    {
        throw new NotImplementedException();
    }

    protected double CommonDivisor(List<double> numbers)
    {
        throw new NotImplementedException();
    }

    protected double Gcd(double a, double b)
    {
        throw new NotImplementedException();
    }
}
