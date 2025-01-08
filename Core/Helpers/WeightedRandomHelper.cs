namespace Core.Helpers;

public class WeightedRandomHelper
{
    public WeightedRandomHelper()
    {
    }

    /// <summary>
    /// Choos an item from the passed in array based on the weightings of each
    /// </summary>
    /// <param name="itemArray">Items and weights to use</param>
    /// <returns>Chosen item from array</returns>
    public T GetWeightedValue<T>(Dictionary<string, object> itemArray)
    {
        throw new NotImplementedException();
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
    public Dictionary<string, object> WeightedRandom(List<object> items, List<double> weights)
    {
        throw new NotImplementedException();
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
