using Sels.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sels.Core.Extensions
{
    /// <summary>
    /// Contains extension for working with the various .NET collections.
    /// </summary>
    public static class CollectionExtensions
    {
        private static Random _random = new Random();

        #region IEnumerable
        /// <summary>
        /// Yield return any object in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Collection to get items from</param>
        /// <returns>Objects in <paramref name="source"/></returns>
        public static IEnumerable<object> Enumerate(this IEnumerable source)
        {
            if (source != null)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Creates a new list from the elements in <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="collection">Collection to get elements from</param>
        /// <returns>List containing all elements in <paramref name="collection"/></returns>
        public static List<T> Copy<T>(this IEnumerable<T> collection)
        {
            return new List<T>(collection);
        }

        /// <summary>
        /// Checks if all elements are unique in <paramref name="collection"/> by comparing with <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="collection">Collection to check</param>
        /// <returns>Boolean indicating if all elements in <paramref name="collection"/> are unique</returns>
        public static bool AreAllUnique<T>(this IEnumerable<T> collection)
        {
            if (collection.HasValue())
            {
                foreach(var item in collection)
                {
                    var occuranceAmount = 0;
                    foreach(var itemToCompare in collection)
                    {
                        if (item.Equals(itemToCompare))
                        {
                            occuranceAmount++;
                        }

                        // Has to be 2 because an item counts itself at least once
                        if(occuranceAmount > 1)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if all element values selected by <paramref name="valueSelector"/> are unique in <paramref name="collection"/> by comparing with <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <typeparam name="T">Collection element type</typeparam>
        /// <param name="collection">Collection to check</param>
        /// <param name="valueSelector">Func that selects the value from <typeparamref name="T"/> to compare</param>
        /// <returns>Boolean indicating if all elements in <paramref name="collection"/> are unique</returns>
        public static bool AreAllUnique<T>(this IEnumerable<T> collection, Func<T, object> valueSelector)
        {
            valueSelector.ValidateArgument(nameof(valueSelector));

            if (collection.HasValue())
            {
                foreach (var item in collection)
                {
                    var itemValue = valueSelector(item);

                    var occuranceAmount = 0;
                    foreach (var itemToCompare in collection)
                    {
                        var itemToCompareValue = valueSelector(itemToCompare);

                        if (itemValue.Equals(itemToCompareValue))
                        {
                            occuranceAmount++;
                        }

                        // Has to be 2 because an item counts itself at least once
                        if (occuranceAmount > 1)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        #region Random
        public static int GetRandomIndex<T>(this IEnumerable<T> value)
        {
            return _random.Next(0, value.Count());
        }

        public static T GetAtIndex<T>(this IEnumerable<T> value, int index)
        {
            var currentIndex = 0;
            foreach (var item in value)
            {
                if (currentIndex == index)
                {
                    return item;
                }

                currentIndex++;
            }

            return default;
        }

        public static T GetRandomItem<T>(this IEnumerable<T> value)
        {
            var randomIndex = value.GetRandomIndex();
            return value.GetAtIndex(randomIndex);
        }
        #endregion
        #endregion

        #region Dictionary
        public static void Merge<TKey, TItem>(this Dictionary<TKey, List<TItem>> dictionary, Dictionary<TKey, List<TItem>> dictionaryToMerge)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            dictionaryToMerge.ValidateArgument(nameof(dictionaryToMerge));

            foreach(var pair in dictionaryToMerge)
            {
                dictionary.AddValues(pair.Key, pair.Value);
            }
        }

        #region AddValue
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        [Obsolete]
        public static void AddValueToCollection<TKey, TItem>(this Dictionary<TKey, IEnumerable<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key))
            {
                var newList = new List<TItem>(dictionary[key]);
                newList.Add(item);
                dictionary[key] = newList;
            }
            else
            {
                var newList = new List<TItem>();
                newList.Add(item);
                dictionary.Add(key, newList);
            }
        }

        /// <summary>
        /// Adds <paramref name="item"/> to the list of <paramref name="key"/> if <paramref name="key"/> exists in <paramref name="dictionary"/>, otherwise create new list and add <paramref name="item"/> to it.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Collection type</typeparam>
        /// <param name="dictionary">Dictionary to add item to</param>
        /// <param name="key">Key for list</param>
        /// <param name="item">Item to add</param>
        public static void AddValueToList<TKey, TItem>(this Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(item);
            }
            else
            {
                var newList = new List<TItem>();
                newList.Add(item);
                dictionary.Add(key, newList);
            }
        }

        [Obsolete]
        public static void AddValues<TKey, TItem>(this Dictionary<TKey, IEnumerable<TItem>> dictionary, TKey key, IEnumerable<TItem> items)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            items.ValidateArgument(nameof(items));

            if (dictionary.ContainsKey(key))
            {
                var newList = new List<TItem>(dictionary[key]);
                newList.AddRange(items);
                dictionary[key] = newList;
            }
            else
            {
                dictionary.Add(key, items);
            }
        }

        /// <summary>
        /// Adds <paramref name="items"/> to the list of <paramref name="key"/> if <paramref name="key"/> exists in <paramref name="dictionary"/>, otherwise create new list and add <paramref name="items"/> to it.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TItem">Collection type</typeparam>
        /// <param name="dictionary">Dictionary to add item to</param>
        /// <param name="key">Key for list</param>
        /// <param name="items">Items to add</param>
        public static void AddValues<TKey, TItem>(this Dictionary<TKey, List<TItem>> dictionary, TKey key, IEnumerable<TItem> items)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            items.ValidateArgument(nameof(items));

            if (dictionary.ContainsKey(key))
            {
                dictionary[key].AddRange(items);
            }
            else
            {
                dictionary.Add(key, new List<TItem>(items));
            }
        }
        #endregion

        #region TryGetOrSet
        public static TValue TryGetOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            return dictionary.TryGetOrSet(key, () => value);
        }

        public static TValue TryGetOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFunc)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                var value = valueFunc();
                dictionary.Add(key, value);
                return value;
            }
        }

        public static TValue TryGetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));

            return dictionary.TryGetOrSet(key, default(TValue));
        }

        #endregion

        #region ContainsItem
        public static bool ContainsItem<TKey, TItem>(this Dictionary<TKey, IEnumerable<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            item.ValidateArgument(nameof(item));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key].Contains(item);
            }

            return false;
        }

        public static bool ContainsItem<TKey, TItem>(this Dictionary<TKey, List<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            item.ValidateArgument(nameof(item));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key].Contains(item);
            }

            return false;
        }

        public static bool ContainsItem<TKey, TItem>(this Dictionary<TKey, Collection<TItem>> dictionary, TKey key, TItem item)
        {
            dictionary.ValidateArgument(nameof(dictionary));
            key.ValidateArgument(nameof(key));
            item.ValidateArgument(nameof(item));

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key].Contains(item);
            }

            return false;
        }
        #endregion
        #endregion

        #region Grid
        public static int GetColumnLength<T>(this IEnumerable<IEnumerable<T>> table)
        {
            var biggestLength = 0;
            foreach (var row in table)
            {
                var rowCount = row.Count();

                if (rowCount > biggestLength)
                {
                    biggestLength = rowCount;
                }
            }

            return biggestLength;
        }

        public static int GetColumnLength<T>(this List<List<T>> table)
        {
            var biggestLength = 0;
            foreach (var row in table)
            {
                var rowCount = row.Count;

                if (rowCount > biggestLength)
                {
                    biggestLength = rowCount;
                }
            }

            return biggestLength;
        }        
        #endregion
    }
}
