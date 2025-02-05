using Core.Utils.Cloners;
using System.Text.Json.Serialization;

namespace Core.Utils.Collections;

/// <summary>
/// Array of ProbabilityObjectArray which allow to randomly draw of the contained objects
/// based on the relative probability of each of its elements.
/// The probabilities of the contained element is not required to be normalized.
///
/// Example:
///   po = new ProbabilityObjectArray(
///          new ProbabilityObject("a", 5),
///          new ProbabilityObject("b", 1),
///          new ProbabilityObject("c", 1)
///   );
///   res = po.draw(10000);
///   // count the elements which should be distributed according to the relative probabilities
///   res.filter(x => x==="b").reduce((sum, x) => sum + 1 , 0)
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="K"></typeparam>
/// <typeparam name="V"></typeparam>
public class ProbabilityObjectArray<K, V> : List<ProbabilityObject<K, V>>
{
    private readonly MathUtil _mathUtil;
    private readonly ICloner _cloner;

    public ProbabilityObjectArray(
        MathUtil mathUtil,
        ICloner cloner,
        ICollection<ProbabilityObject<K, V>>? items = null
    ) : base(items ?? [])
    {
        _mathUtil = mathUtil;
        _cloner = cloner;
    }

    /// <summary>
    /// Calculates the normalized cumulative probability of the ProbabilityObjectArray's elements normalized to 1
    /// </summary>
    /// <param name="probValues">The relative probability values of which to calculate the normalized cumulative sum</param>
    /// <returns>Cumulative Sum normalized to 1</returns>
    public List<double> CumulativeProbability(List<double> probValues)
    {
        var sum = _mathUtil.ListSum(probValues);
        var probCumsum = _mathUtil.ListCumSum(probValues);
        probCumsum = _mathUtil.ListProduct(probCumsum, 1D / sum);

        return probCumsum;
    }

    /// <summary>
    /// Filter What is inside ProbabilityObjectArray
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns>Filtered results</returns>
    public ProbabilityObjectArray<K, V> Filter(Predicate<ProbabilityObject<K, V>> predicate)
    {
        var result = new ProbabilityObjectArray<K, V>(_mathUtil, _cloner, new List<ProbabilityObject<K, V>>());
        foreach (var probabilityObject in this)
            if (predicate.Invoke(probabilityObject))
                result.Add(probabilityObject);

        return result;
    }

    /// <summary>
    /// Deep clone this ProbabilityObjectArray
    /// </summary>
    /// <returns>Deep Copy of ProbabilityObjectArray</returns>
    public ProbabilityObjectArray<K, V> Clone()
    {
        var clone = _cloner.Clone(this);
        var probabilityObjects = new ProbabilityObjectArray<K, V>(
            _mathUtil,
            _cloner,
            new List<ProbabilityObject<K, V>>()
        );
        probabilityObjects.AddRange(clone);

        return probabilityObjects;
    }

    /// <summary>
    /// Drop an element from the ProbabilityObjectArray
    /// </summary>
    /// <param name="key">The key of the element to drop</param>
    /// <returns>ProbabilityObjectArray without the dropped element</returns>
    public ProbabilityObjectArray<K, V> Drop(K key)
    {
        return (ProbabilityObjectArray<K, V>)this.Where((r) => !r.Key?.Equals(key) ?? false);
    }

    /// <summary>
    /// Return the data field of an element of the ProbabilityObjectArray
    /// </summary>
    /// <param name="key">The key of the element whose data shall be retrieved</param>
    /// <returns>Stored data object</returns>
    public V? Data(K key)
    {
        var element = this.FirstOrDefault(r => r.Key?.Equals(key) ?? false);
        return element == null ? default : element.Data;
    }

    /// <summary>
    /// Get the relative probability of an element by its key
    /// 
    /// Example:
    ///  po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
    ///  po.maxProbability() // returns 5
    /// </summary>
    /// <param name="key">Key of element whose relative probability shall be retrieved</param>
    /// <returns>The relative probability</returns>
    public double? Probability(K key)
    {
        var element = this.FirstOrDefault(r => r.Key.Equals(key));
        return element?.RelativeProbability;
    }

    /**
     * Get the maximum relative probability out of a ProbabilityObjectArray
     *
     * Example:
     *  po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
     *  po.maxProbability() // returns 5
     *
     * @return      {number}                                                the maximum value of all relative probabilities in this ProbabilityObjectArray
     */
    public double MaxProbability()
    {
        return this.Max(x => x.RelativeProbability).Value;
    }

    /// <summary>
    /// Get the minimum relative probability out of a ProbabilityObjectArray
    ///  * Example:
    ///  po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
    ///  po.minProbability() // returns 1
    /// </summary>
    /// <returns>the minimum value of all relative probabilities in this ProbabilityObjectArray</returns>
    public double MinProbability()
    {
        return this.Min(x => x.RelativeProbability.Value);
    }

    /**
     * Draw random element of the ProbabilityObject N times to return an array of N keys.
     * Drawing can be with or without replacement
     * @param count The number of times we want to draw
     * @param removeAfterDraw Draw with or without replacement from the input dict (true = dont remove after drawing)
     * @param lockList list keys which shall be replaced even if drawing without replacement
     * @returns Array consisting of N random keys for this ProbabilityObjectArray
     */
    public List<K> Draw(int drawCount = 1, bool removeAfterDraw = true, List<K>? neverRemoveWhitelist = null)
    {
        neverRemoveWhitelist ??= [];
        if (Count == 0) return [];

        var totals = this.Aggregate(
            new { probArray = new List<double>(), keyArray = new List<K>() },
            (acc, x) =>
            {
                acc.probArray.Add(x.RelativeProbability.Value);
                acc.keyArray.Add(x.Key);
                return acc;
            }
        );

        var probCumsum = CumulativeProbability(totals.probArray);

        var drawnKeys = new List<K>();
        for (var i = 0; i < drawCount; i++)
        {
            var rand = Random.Shared.NextDouble();
            var randomIndex = (int)probCumsum.FindIndex((x) => x > rand);
            // We cannot put Math.random() directly in the findIndex because then it draws anew for each of its iteration
            if (removeAfterDraw || neverRemoveWhitelist.Contains(totals.keyArray[randomIndex]))
            {
                // Add random item from possible value into return array
                drawnKeys.Add(totals.keyArray[randomIndex]);
            }
            else
            {
                // We draw without replacement -> remove the key and its probability from array
                var key = totals.keyArray[randomIndex];
                totals.keyArray.RemoveAt(randomIndex);
                _ = totals.probArray[randomIndex];
                totals.probArray.RemoveAt(randomIndex);
                drawnKeys.Add(key);
                probCumsum = CumulativeProbability(totals.probArray);
                // If we draw without replacement and the ProbabilityObjectArray is exhausted we need to break
                if (totals.keyArray.Count < 1) break;
            }
        }

        return drawnKeys;
    }
}

/// <summary>
/// A ProbabilityObject which is use as an element to the ProbabilityObjectArray array
/// It contains a key, the relative probability as well as optional data.
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="V"></typeparam>
public class ProbabilityObject<K, V>
{
    public ProbabilityObject()
    {
    }

    /**
 * constructor for the ProbabilityObject
 * @param       {string}                        key                         The key of the element
 * @param       {number}                        relativeProbability         The relative probability of this element
 * @param       {any}                           data                        Optional data attached to the element
 */
    public ProbabilityObject(K key, double? relativeProbability, V? data)
    {
        Key = key;
        RelativeProbability = relativeProbability;
        Data = data;
    }

    [JsonPropertyName("key")]
    public K? Key { get; set; }

    /// <summary>
    /// Weighting of key compared to other ProbabilityObjects
    /// </summary>
    [JsonPropertyName("relativeProbability")]
    public double? RelativeProbability { get; set; }

    [JsonPropertyName("data")]
    public V? Data { get; set; }
}
