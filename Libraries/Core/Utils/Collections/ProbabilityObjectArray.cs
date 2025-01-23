using Core.Utils.Cloners;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;

namespace Core.Utils.Collections;

/**
 * Array of ProbabilityObjectArray which allow to randomly draw of the contained objects
 * based on the relative probability of each of its elements.
 * The probabilities of the contained element is not required to be normalized.
 *
 * Example:
 *   po = new ProbabilityObjectArray(
 *          new ProbabilityObject("a", 5),
 *          new ProbabilityObject("b", 1),
 *          new ProbabilityObject("c", 1)
 *   );
 *   res = po.draw(10000);
 *   // count the elements which should be distributed according to the relative probabilities
 *   res.filter(x => x==="b").reduce((sum, x) => sum + 1 , 0)
 */
public class ProbabilityObjectArray<T, K, V> : List<T> where T : ProbabilityObject<K,V>
{
    private MathUtil _mathUtil;
    private ICloner _cloner;

    public ProbabilityObjectArray(
        MathUtil mathUtil,
        ICloner cloner,
        ICollection<T> items
    ) : base(items)
    {
        _mathUtil = mathUtil;
        _cloner = cloner;
    }

    /**
     * Calculates the normalized cumulative probability of the ProbabilityObjectArray's elements normalized to 1
     * @param       {array}                         probValues              The relative probability values of which to calculate the normalized cumulative sum
     * @returns     {array}                                                 Cumulative Sum normalized to 1
     */
    public List<double> CumulativeProbability(List<double> probValues)
    {
        var sum = _mathUtil.ListSum(probValues);
        var probCumsum = _mathUtil.ListCumSum(probValues);
        probCumsum = _mathUtil.ListProduct(probCumsum, 1D / sum);
        return probCumsum;
    }

    public ProbabilityObjectArray<T, K, V> Filter(Predicate<ProbabilityObject<K, V>> predicate)
    {
        var filtered = new ProbabilityObjectArray<T, K, V>(_mathUtil, _cloner, new List<T>());
        foreach (var probabilityObject in this)
        {
            if (predicate.Invoke(probabilityObject))
                filtered.Add(probabilityObject);
        }
        return filtered;
    }
    
    /**
     * Clone this ProbabilitObjectArray
     * @returns     {ProbabilityObjectArray}                                Deep Copy of this ProbabilityObjectArray
     */
    public ProbabilityObjectArray<T, K, V> Clone()
    {
        var clone = _cloner.Clone(this);
        var probabliltyObjects = new ProbabilityObjectArray<T, K, V>(
            _mathUtil,
            _cloner,
            new List<T>()
        );
        probabliltyObjects.AddRange(clone);
        return probabliltyObjects;
    }

    /**
     * Drop an element from the ProbabilityObjectArray
     *
     * @param       {string}                        key                     The key of the element to drop
     * @returns     {ProbabilityObjectArray}                                ProbabilityObjectArray without the dropped element
     */
    public ProbabilityObjectArray<T, K, V> Drop(K key)
    {
        return (ProbabilityObjectArray<T, K, V>)this.Where((r) => !r.Key?.Equals(key) ?? false);
    }

    /**
     * Return the data field of a element of the ProbabilityObjectArray
     * @param       {string}                        key                     The key of the element whose data shall be retrieved
     * @returns     {object}                                                The data object
     */
    public V? Data(K key)
    {
        var element = this.FirstOrDefault(r => r.Key?.Equals(key) ?? false);
        return element == null ? default : element.Data;
    }

    /**
     * Get the relative probability of an element by its key
     *
     * Example:
     *  po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
     *  po.maxProbability() // returns 5
     *
     * @param       {string}                        key                     The key of the element whose relative probability shall be retrieved
     * @return      {number}                                                The relative probability
     */
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
        return this.Select(x => x.RelativeProbability).Max();
    }

    /**
     * Get the minimum relative probability out of a ProbabilityObjectArray
     *
     * Example:
     *  po = new ProbabilityObjectArray(new ProbabilityObject("a", 5), new ProbabilityObject("b", 1))
     *  po.minProbability() // returns 1
     *
     * @return      {number}                                                the minimum value of all relative probabilities in this ProbabilityObjectArray
     */
    public double MinProbability()
    {
        return this.Select(x => x.RelativeProbability).Min();
    }

    /**
     * Draw random element of the ProbabilityObject N times to return an array of N keys.
     * Drawing can be with or without replacement
     * @param count The number of times we want to draw
     * @param replacement Draw with or without replacement from the input dict (true = dont remove after drawing)
     * @param lockList list keys which shall be replaced even if drawing without replacement
     * @returns Array consisting of N random keys for this ProbabilityObjectArray
     */
    public List<K> Draw(int count = 1, bool replacement = true, List<K>? lockList = null)
    {
        lockList ??= [];
        if (Count == 0)
        {
            return [];
        }

        var totals = this.Aggregate(
            new { probArray = new List<double>(), keyArray = new List<K>() },
            (acc, x) =>
            {
                acc.probArray.Add(x.RelativeProbability);
                acc.keyArray.Add(x.Key);
                return acc;
            }
        );

        var probCumsum = CumulativeProbability(totals.probArray);

        var drawnKeys = new List<K>();
        for (var i = 0; i < count; i++)
        {
            var rand = Random.Shared.NextDouble();
            var randomIndex = (int)probCumsum.FindIndex((x) => x > rand);
            // We cannot put Math.random() directly in the findIndex because then it draws anew for each of its iteration
            if (replacement || lockList.Contains(totals.keyArray[randomIndex]))
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
                if (totals.keyArray.Count < 1)
                {
                    break;
                }
            }
        }

        return drawnKeys;
    }
}

/**
 * A ProbabilityObject which is use as an element to the ProbabilityObjectArray array
 * It contains a key, the relative probability as well as optional data.
 */
public class ProbabilityObject<K, V>
{
    public ProbabilityObject()
    { }

    [JsonPropertyName("key")]
    public K Key { get; set; }

    [JsonPropertyName("relativeProbability")]
    public double RelativeProbability { get; set; }

    [JsonPropertyName("data")]
    public V? Data { get; set; }

    /**
     * varructor for the ProbabilityObject
     * @param       {string}                        key                         The key of the element
     * @param       {number}                        relativeProbability         The relative probability of this element
     * @param       {any}                           data                        Optional data attached to the element
     */
    public ProbabilityObject(K key, double relativeProbability, V? data)
    {
        Key = key;
        RelativeProbability = relativeProbability;
        Data = data;
    }
}
