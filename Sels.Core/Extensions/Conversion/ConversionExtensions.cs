using Sels.Core.Extensions.Calculation;
using Sels.Core.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Extensions.Conversion
{
    /// <summary>
    /// Contains extension methods for converting objects to other types.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Casts <paramref name="source"/> to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="source">Object to cast</param>
        /// <returns>Casted object</returns>
        public static T Cast<T>(this object source)
        {
            return (T)source;
        }

        /// <summary>
        /// Casts <paramref name="source"/> to type <typeparamref name="T"/> if it can be casted, otherwise return default of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="source">Object to cast</param>
        /// <returns>Casted object</returns>
        public static T CastOrDefault<T>(this object source)
        {
            if (source != null && source is T casted)
            {
                return casted;
            }

            return default;
        }

        #region ChangeType
        /// <summary>
        /// Attempts to convert <paramref name="source"/> to an instance of type <typeparamref name="TNew"/>.
        /// </summary>
        /// <typeparam name="TNew">The type to convert to</typeparam>
        /// <param name="source">The object to convert</param>
        /// <returns>The converted value</returns>
        public static TNew ChangeType<TNew>(this object source)
        {
            source.ValidateArgument(nameof(source));

            return source.ChangeType(typeof(TNew)).Cast<TNew>();
        }

        /// <summary>
        /// Attempts to convert <paramref name="source"/> to an instance of type <paramref name="newType"/>.
        /// </summary>
        /// <param name="source">The object to convert</param>
        /// <param name="newType">The type to convert to</param>
        /// <returns>The converted value</returns>
        public static object ChangeType(this object source, Type newType)
        {
            source.ValidateArgument(nameof(source));
            newType.ValidateArgument(nameof(newType));

            return Convert.ChangeType(source, Nullable.GetUnderlyingType(newType) ?? newType);
        }
        #endregion

        #region ToArray
        /// <summary>
        /// Tranforms <paramref name="items"/> into an array. If <paramref name="items"/> is null an empty array will be created
        /// </summary>
        /// <typeparam name="T">Element type of <paramref name="items"/></typeparam>
        /// <param name="items">Collection to turn into an array</param>
        /// <returns>A new array of element type <typeparamref name="T"/></returns>
        public static T[] ToArrayOrDefault<T>(this IEnumerable<T> items)
        {
            if (items.HasValue())
            {
                return items.ToArray();
            }

            return new T[0];
        }
        #endregion

        #region AsArray
        /// <summary>
        /// Creates a new array containing <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="value">Value to add to array</param>
        /// <returns>Array containing <paramref name="value"/></returns>
        public static T[] AsArray<T>(this T value)
        {
            value.ValidateArgument(nameof(value));

            return new T[] { value };
        }

        /// <summary>
        /// Creates a new array containing <paramref name="value"/> if it is not null, otherwise create an empty array.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="value">Value to add to array</param>
        /// <returns>Array containing <paramref name="value"/> or empty array</returns>
        public static T[] AsArrayOrDefault<T>(this T value)
        {
            if (value.HasValue())
            {
                return new T[] { value };
            }

            return new T[0];
        }
        #endregion

        #region AsList
        /// <summary>
        /// Creates a new list containing <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="value">Value to add to list</param>
        /// <returns>List containing <paramref name="value"/></returns>
        public static List<T> AsList<T>(this T value)
        {
            value.ValidateArgument(nameof(value));

            return new List<T>() { value };
        }

        /// <summary>
        /// Creates a new list containing <paramref name="value"/> if it is not null, otherwise create an empty array.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="value">Value to add to list</param>
        /// <returns>List containing <paramref name="value"/> or empty list</returns>
        public static List<T> AsListOrDefault<T>(this T value)
        {
            if (value.HasValue())
            {
                return new List<T>() { value };
            }

            return new List<T>();
        }
        #endregion

        #region AsEnumerable
        /// <summary>
        /// Creates an enumerator returning <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="value"/></typeparam>
        /// <param name="value">Value to return</param>
        /// <returns>Enumerator that returns <paramref name="value"/></returns>
        public static IEnumerable<T> AsEnumerable<T>(this T value)
        {
            value.ValidateArgument(nameof(value));

            yield return value;
        }
        #endregion

        #region Numeric
        /// <summary>
        /// Converts <paramref name="number"/> to <see cref="uint"/>.
        /// </summary>
        /// <param name="number">The number to convert</param>
        /// <returns><paramref name="number"/> as <see cref="uint"/></returns>
        public static uint ToUInt32(this int number)
        {
            return Convert.ToUInt32(number);
        }
        /// <summary>
        /// Converts <paramref name="number"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="number">The number to convert</param>
        /// <returns><paramref name="number"/> as <see cref="int"/></returns>
        public static int ToInt(this uint number)
        {
            return Convert.ToInt32(number);
        }
        #endregion

        #region GroupAsDictionary
        /// <summary>
        /// Creates new dictionary by grouping the items in <paramref name="source"/>. <paramref name="keySelector"/> will select the key for each item and <paramref name="valueSelector"/> will select the value for each item.
        /// </summary>
        /// <typeparam name="TKey">Key type for dictionary</typeparam>
        /// <typeparam name="TValue">Value type for dictionary</typeparam>
        /// <typeparam name="T">Collection type of <paramref name="source"/></typeparam>
        /// <param name="source">Items to group</param>
        /// <param name="keySelector">Func that selects key for each item</param>
        /// <param name="valueSelector">Func that selects value for each item</param>
        /// <returns>Dictionary with grouped items from <paramref name="source"/></returns>
        public static Dictionary<TKey, List<TValue>> GroupAsDictionary<TKey, TValue, T>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
        {
            keySelector.ValidateArgument(nameof(keySelector));
            valueSelector.ValidateArgument(nameof(keySelector));

            Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();

            if (source.HasValue())
            {
                foreach(var item in source)
                {
                    var key = keySelector(item);
                    var value = valueSelector(item);

                    dictionary.AddValueToList(key, value); 
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Creates new dictionary by grouping the items in <paramref name="source"/>. <paramref name="keySelector"/> will select the key for each item.
        /// </summary>
        /// <typeparam name="TKey">Key type for dictionary</typeparam>
        /// <typeparam name="T">Collection type of <paramref name="source"/></typeparam>
        /// <param name="source">Items to group</param>
        /// <param name="keySelector">Func that selects key for each item</param>
        /// <returns>Dictionary with grouped items from <paramref name="source"/></returns>
        public static Dictionary<TKey, List<T>> GroupAsDictionary<TKey, T>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            keySelector.ValidateArgument(nameof(keySelector));

            return source.GroupAsDictionary(keySelector, x => x);
        }
        #endregion
    }
}
