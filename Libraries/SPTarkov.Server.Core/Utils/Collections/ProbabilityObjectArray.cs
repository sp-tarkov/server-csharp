using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SPTarkov.Server.Core.Utils.Collections;

/// <summary>
///     Array of ProbabilityObjectArray which allow to randomly draw of the contained objects
///     based on the relative probability of each of its elements.
///     The probabilities of the contained element is not required to be normalized.
///     Example:
///     po = new ProbabilityObjectArray(
///     new ProbabilityObject("a", 5),
///     new ProbabilityObject("b", 1),
///     new ProbabilityObject("c", 1)
///     );
///     res = po.draw(10000);
///     // count the elements which should be distributed according to the relative probabilities
///     res.filter(x => x==="b").reduce((sum, x) => sum + 1 , 0)
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="V"></typeparam>
public class ProbabilityObjectArray<K, V> : List<ProbabilityObject<K, V>>
{
    private readonly ICloner _cloner;

    public ProbabilityObjectArray(ICloner cloner, ICollection<ProbabilityObject<K, V>>? items = null)
        : base(items ?? [])
    {
        _cloner = cloner;
    }

    /// <summary>
    ///     Calculates the normalized cumulative probability of the ProbabilityObjectArray's elements normalized to 1
    /// </summary>
    /// <param name="probValues">The relative probability values of which to calculate the normalized cumulative sum</param>
    /// <returns>Cumulative Sum normalized to 1</returns>
    public IEnumerable<double> CumulativeProbability(IEnumerable<double> probValues)
    {
        var sum = probValues.Sum();
        var probCumsum = probValues.CumulativeSum();
        probCumsum = probCumsum.Product(1D / sum);

        return probCumsum;
    }

    /// <summary>
    ///     Filter What is inside ProbabilityObjectArray
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns>Filtered results</returns>
    public ProbabilityObjectArray<K, V> Filter(Predicate<ProbabilityObject<K, V>> predicate)
    {
        var result = new ProbabilityObjectArray<K, V>(_cloner, new List<ProbabilityObject<K, V>>());
        foreach (var probabilityObject in this)
        {
            if (predicate.Invoke(probabilityObject))
            {
                result.Add(probabilityObject);
            }
        }

        return result;
    }

    /// <summary>
    ///     Deep clone this ProbabilityObjectArray
    /// </summary>
    /// <returns>Deep Copy of ProbabilityObjectArray</returns>
    public ProbabilityObjectArray<K, V> Clone()
    {
        var clone = _cloner.Clone(this);
        var probabilityObjects = new ProbabilityObjectArray<K, V>(_cloner, new List<ProbabilityObject<K, V>>());
        probabilityObjects.AddRange(clone);

        return probabilityObjects;
    }

    /// <summary>
    ///     Drop an element from the ProbabilityObjectArray
    /// </summary>
    /// <param name="key">The key of the element to drop</param>
    /// <returns>ProbabilityObjectArray without the dropped element</returns>
    public ProbabilityObjectArray<K, V> Drop(K key)
    {
        return (ProbabilityObjectArray<K, V>)this.Where(r => !r.Key?.Equals(key) ?? false);
    }

    /// <summary>
    ///     Return the data field of an element of the ProbabilityObjectArray
    /// </summary>
    /// <param name="key">The key of the element whose data shall be retrieved</param>
    /// <returns>Stored data object</returns>
    public V? Data(K key)
    {
        var element = this.FirstOrDefault(r => r.Key?.Equals(key) ?? false);
        return element == null ? default : element.Data;
    }

    /// <summary>
    ///     Get the relative probability of an element by its key
    ///     Example:
    ///     po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
    ///     po.maxProbability() // returns 5
    /// </summary>
    /// <param name="key">Key of element whose relative probability shall be retrieved</param>
    /// <returns>The relative probability</returns>
    public double? Probability(K key)
    {
        var element = this.FirstOrDefault(r => r.Key.Equals(key));
        return element?.RelativeProbability;
    }

    /// <summary>
    /// Get the maximum relative probability out of a ProbabilityObjectArray
    /// Example:
    /// po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
    /// po.maxProbability() // returns 5
    /// </summary>
    /// <returns>the maximum value of all relative probabilities in this ProbabilityObjectArray</returns>
    public double MaxProbability()
    {
        return this.Max(x => x.RelativeProbability).Value;
    }

    /// <summary>
    ///     Get the minimum relative probability out of a ProbabilityObjectArray
    ///     * Example:
    ///     po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
    ///     po.minProbability() // returns 1
    /// </summary>
    /// <returns>the minimum value of all relative probabilities in this ProbabilityObjectArray</returns>
    public double MinProbability()
    {
        return this.Min(x => x.RelativeProbability.Value);
    }

    /// <summary>
    ///Draw random element of the ProbabilityObject N times to return an array of N keys
    /// Keeps chosen element in place
    /// Chosen items can be duplicates
    /// </summary>
    /// <param name="itemCountToDraw">The number of times we want to draw</param>
    /// <returns>Collection consisting of N random keys for this ProbabilityObjectArray</returns>
    public List<K> Draw(int itemCountToDraw = 1)
    {
        if (Count == 0)
        {
            // Nothing in pool
            return [];
        }

        var cumulativeProbabilities = CumulativeProbability(this.Select(x => x.RelativeProbability.Value)).ToList();

        // Init results collection
        var results = new List<K>(itemCountToDraw);

        // Loop until we've picked to desired item count
        for (var i = 0; i < itemCountToDraw; i++)
        {
            var rand = Random.Shared.NextDouble();
            var randomIndex = cumulativeProbabilities.FindIndex(probability => probability > rand);
            results.Add(this[randomIndex].Key);
        }

        return results;
    }

    /// <summary>
    ///Draw random element of the ProbabilityObject N times to return an array of N keys
    /// Removes drawn elements
    /// </summary>
    /// <param name="itemCountToDraw">The number of times we want to draw</param>
    /// <param name="neverRemoveWhitelist">List of keys which shall be replaced even if drawing without replacement</param>
    /// <returns>Collection consisting of N random keys for this ProbabilityObjectArray</returns>
    public List<K> DrawAndRemove(int itemCountToDraw = 1, List<K>? neverRemoveWhitelist = null)
    {
        if (Count == 0)
        {
            // Nothing in pool
            return [];
        }

        var availableItems = this.Select(x => (x.Key, Weight: x.RelativeProbability.Value)).ToList();

        // Calculate total weighting of all items combined
        var totalWeight = availableItems.Sum(x => x.Weight);

        // Init results collection
        var drawnKeys = new List<K>(itemCountToDraw);

        // Loop until we have drawn to desired count or pool is empty
        for (var i = 0; i < itemCountToDraw && availableItems.Any(); i++)
        {
            // Get value between 0 and 1 to act as a target to aim for
            var randomTarget = Random.Shared.NextDouble() * totalWeight;

            // Set default index to start
            var chosenIndex = -1;

            // Find element related to random target (greedy)
            for (var j = 0; j < availableItems.Count; j++)
            {
                // Subtract weight of item from above chosen value
                randomTarget -= availableItems[j].Weight;
                if (randomTarget <= 0)
                {
                    // Item falls within 'slice' of desired target,
                    // item has weight that eclipses accumulated weight of randomTarget
                    chosenIndex = j;
                    break;
                }
            }

            // If index not found choose the last element
            chosenIndex = (chosenIndex == -1) ? availableItems.Count - 1 : chosenIndex;

            // Get chosen item via index and add to results
            var chosenItem = availableItems[chosenIndex];
            drawnKeys.Add(chosenItem.Key);

            // Only remove item if it's not in whitelist
            if (neverRemoveWhitelist is not null && !neverRemoveWhitelist.Contains(chosenItem.Key))
            {
                // Reduce total weight value by items weight + Remove item from pool
                totalWeight -= chosenItem.Weight;
                availableItems.RemoveAt(chosenIndex);
            }
        }

        return drawnKeys;
    }
}

/// <summary>
///     A ProbabilityObject which is use as an element to the ProbabilityObjectArray array
///     It contains a key, the relative probability as well as optional data.
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="V"></typeparam>
public class ProbabilityObject<K, V>
{
    public ProbabilityObject() { }

    /// <summary>
    /// constructor for the ProbabilityObject
    /// </summary>
    /// <param name="key">The key of the element</param>
    /// <param name="relativeProbability">The relative probability of this element</param>
    /// <param name="data">Optional data attached to the element</param>
    public ProbabilityObject(K key, double? relativeProbability, V? data)
    {
        Key = key;
        RelativeProbability = relativeProbability;
        Data = data;
    }

    [JsonPropertyName("key")]
    public K? Key { get; set; }

    /// <summary>
    ///     Weighting of key compared to other ProbabilityObjects
    /// </summary>
    [JsonPropertyName("relativeProbability")]
    public double? RelativeProbability { get; set; }

    [JsonPropertyName("data")]
    public V? Data { get; set; }
}
