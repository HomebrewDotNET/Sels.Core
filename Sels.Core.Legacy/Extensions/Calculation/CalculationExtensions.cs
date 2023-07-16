using System;
using System.Collections.Generic;
using System.Linq;

namespace Sels.Core.Extensions.Calculation
{
    /// <summary>
    /// Contains extension for doing math related actions.
    /// </summary>
    public static class CalculationExtensions
    {
        #region Multiply
        /// <summary>
        /// Multiplies <paramref name="value"/> by <paramref name="multiplyValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to multiply</param>
        /// <param name="multiplyValue">Value to multiply <paramref name="value"/> with</param>
        /// <param name="times">How many times to multiply <paramref name="value"/> with <paramref name="multiplyValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal MultiplyBy(this decimal value, decimal multiplyValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            while (times > 0)
            {
                value *= multiplyValue;

                times--;
            }

            return value;
        }

        /// <summary>
        /// Multiplies <paramref name="value"/> by <paramref name="multiplyValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to multiply</param>
        /// <param name="multiplyValue">Value to multiply <paramref name="value"/> with</param>
        /// <param name="times">How many times to multiply <paramref name="value"/> with <paramref name="multiplyValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal MultiplyBy(this long value, long multiplyValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            while (times > 0)
            {
                value *= multiplyValue;

                times--;
            }

            return value;
        }
        #endregion

        #region DivideBy
        /// <summary>
        /// Divides <paramref name="value"/> by <paramref name="divideValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to divide</param>
        /// <param name="divideValue">Value to divide <paramref name="value"/> with</param>
        /// <param name="times">How many times to divide <paramref name="value"/> with <paramref name="divideValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal DivideBy(this decimal value, decimal divideValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            while (times > 0)
            {
                value /= divideValue;

                times--;
            }

            return value;
        }

        /// <summary>
        /// Divides <paramref name="value"/> by <paramref name="divideValue"/> <paramref name="times"/> times.
        /// </summary>
        /// <param name="value">Value to divide</param>
        /// <param name="divideValue">Value to divide <paramref name="value"/> with</param>
        /// <param name="times">How many times to divide <paramref name="value"/> with <paramref name="divideValue"/></param>
        /// <returns>Calculated value</returns>
        public static decimal DivideBy(this long value, long divideValue, int times = 1)
        {
            times.ValidateArgumentLargerOrEqual(nameof(times), 1);

            decimal newValue = value;

            while (times > 0)
            {
                newValue /= divideValue;

                times--;
            }

            return newValue;
        }
        #endregion

        #region Median
        /// <summary>
        /// Returns the median in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <typeparam name="TValue">The type of the median value</typeparam>
        /// <param name="source">The source to get the median from</param>
        /// <param name="selector">Delegate that selects the value to get the median from</param>
        /// <returns>The median in <paramref name="source"/></returns>
        public static TValue Median<T, TValue>(this IEnumerable<T> source, Func<T, TValue> selector) where TValue : IComparable<TValue>
        {
            source.ValidateArgumentNotNullOrEmpty(nameof(source));
            selector.ValidateArgument(nameof(selector));

            // Sort by ascending first
            var sorted = source.Select(x => selector(x)).OrderBy(x => x).ToArray();
            // Only one element so return that
            if (sorted.Length == 1) return sorted[0];
            var middleIndex = sorted.Length / 2;

            // Check if array length is even, if it is we have to calculate the median
            if (sorted.Length % 2 == 0)
            {
                return ((dynamic)sorted[middleIndex] + (dynamic)sorted[middleIndex - 1]) / 2;
            }
            else
            {
                return sorted[middleIndex];
            }
        }

        /// <summary>
        /// Returns the median in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The type of the element</typeparam>
        /// <param name="source">The source to get the median from</param>
        /// <returns>The median in <paramref name="source"/></returns>
        public static T Median<T>(this IEnumerable<T> source) where T : IComparable<T> => source.Median(x => x);
        #endregion
    }
}
