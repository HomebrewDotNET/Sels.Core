using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Sels.Core.Delegates;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains extension methods for comparing values.
    /// </summary>
    public static class ComparisonExtensions
    {
        #region In
        /// <summary>
        /// Checks if <paramref name="value"/> is in <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T">The type to compare</typeparam>
        /// <param name="value">The value to compare to <paramref name="values"/></param>
        /// <param name="values">The valid values for <paramref name="value"/></param>
        /// <returns>True if <paramref name="value"/> is equal to any of <paramref name="values"/>, otherwise false</returns>
        public static bool In<T>(this T value, params T[] values)
        {
            return value.In((x,y) => x.Equals(y),values);
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is in <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T">The type to compare</typeparam>
        /// <param name="value">The value to compare to <paramref name="values"/></param>
        /// <param name="comparer">To comparer to use to check for equality</param>
        /// <param name="values">The valid values for <paramref name="value"/></param>
        /// <returns>True if <paramref name="value"/> is equal to any of <paramref name="values"/>, otherwise false</returns>
        public static bool In<T>(this T value, IEqualityComparer<T> comparer, params T[] values)
        {
            comparer.ValidateArgument(nameof(comparer));
            if (!values.HasValue()) return false;

            if (value == null) return values.Any(x => x == null);
            return values.Contains(value, comparer);
        }
        /// <summary>
        /// Checks if <paramref name="value"/> is in <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T">The type to compare</typeparam>
        /// <param name="value">The value to compare to <paramref name="values"/></param>
        /// <param name="comparator">The delegate to use to check for equality</param>
        /// <param name="values">The valid values for <paramref name="value"/></param>
        /// <returns>True if <paramref name="value"/> is equal to any of <paramref name="values"/>, otherwise false</returns>
        public static bool In<T>(this T value, Comparator<T> comparator, params T[] values)
        {
            comparator.ValidateArgument(nameof(comparator));
            if (!values.HasValue()) return false;

            if (value == null) return values.Any(x => x == null);
            return values.Any(x => comparator(value, x));
        }
        #endregion
    }
}
