namespace SPTarkov.Server.Core.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Add a value by key to a dictionary, if the key doesn't exist, create it
        /// </summary>
        /// <typeparam name="T">Dictionary key type</typeparam>
        /// <param name="dict">Dictionary to add/update</param>
        /// <param name="key">Key to update by</param>
        /// <param name="value">Value to add to key</param>
        public static void AddOrUpdate<T>(this IDictionary<T, double> dict, T key, double value)
            where T : notnull
        {
            if (!dict.TryAdd(key, value))
            {
                dict[key] += value;
            }
        }

        /// <summary>
        /// Add a value by key to a dictionary, if the key doesn't exist, create it
        /// </summary>
        /// <typeparam name="T">Dictionary key type</typeparam>
        /// <param name="dict">Dictionary to add/update</param>
        /// <param name="key">Key to update by</param>
        /// <param name="value">Value to add to key</param>
        public static void AddOrUpdate<T>(this IDictionary<T, int> dict, T key, int value)
            where T : notnull
        {
            if (!dict.TryAdd(key, value))
            {
                dict[key] += value;
            }
        }

        public static void RemoveItems<K, V>(this IDictionary<K, V> collection, ISet<K> idsToRemove)
        {
            foreach (var key in idsToRemove)
            {
                collection.Remove(key);
            }
        }
    }
}
