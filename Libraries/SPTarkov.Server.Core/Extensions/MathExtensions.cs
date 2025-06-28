namespace SPTarkov.Server.Core.Extensions
{
    public static class MathExtensions
    {
        /// <summary>
        ///     Helper to create the cumulative sum of all enumerable elements
        ///     [1, 2, 3, 4].CumulativeSum() = [1, 3, 6, 10]
        /// </summary>
        /// <param name="values">The enumerable with numbers of which to calculate the cumulative sum</param>
        /// <returns>cumulative sum of values</returns>
        public static IEnumerable<double> CumulativeSum(this IEnumerable<double> values)
        {
            double sum = 0;
            foreach (var value in values)
            {
                sum += value;
                yield return sum;
            }
        }

        /// <summary>
        ///     Helper to create the cumulative sum of all enumerable elements
        ///     [1, 2, 3, 4].CumulativeSum() = [1, 3, 6, 10]
        /// </summary>
        /// <param name="values">The enumerable with numbers of which to calculate the cumulative sum</param>
        /// <returns>cumulative sum of values</returns>
        public static IEnumerable<float> CumulativeSum(this IEnumerable<float> values)
        {
            float sum = 0;
            foreach (var value in values)
            {
                sum += value;
                yield return sum;
            }
        }

        /// <summary>
        ///     Helper to create the product of each element times factor
        /// </summary>
        /// <param name="values">The enumerable of numbers which shall be multiplied by the factor</param>
        /// <param name="factor">Number to multiply each element by</param>
        /// <returns>An enumerable of elements all multiplied by the factor</returns>
        public static IEnumerable<double> Product(this IEnumerable<double> values, double factor)
        {
            return values.Select(v => v * factor);
        }

        /// <summary>
        ///     Helper to create the product of each element times factor
        /// </summary>
        /// <param name="values">The enumerable of numbers which shall be multiplied by the factor</param>
        /// <param name="factor">Number to multiply each element by</param>
        /// <returns>An enumerable of elements all multiplied by the factor</returns>
        public static IEnumerable<float> Product(this IEnumerable<float> values, float factor)
        {
            return values.Select(v => v * factor);
        }
    }
}
