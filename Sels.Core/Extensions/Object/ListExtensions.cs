using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using static Sels.Core.Delegates;

namespace System.Collections.Generic
{
    /// <summary>
    /// Contains extensions for System.Collections.Generic.List
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Inserts <paramref name="value"/> in <paramref name="list"/> before the first item matching <paramref name="predicate"/>. If no matching item is found it added to <paramref name="list"/>. 
        /// </summary>
        /// <typeparam name="T">Element type of list</typeparam>
        /// <param name="list">List to add element in</param>
        /// <param name="predicate">Predicate for finding object to insert before</param>
        /// <param name="value">Object to insert</param>
        public static void InsertBefore<T>(this List<T> list, Predicate<T> predicate, T value)
        {
            list.ValidateArgument(nameof(list));
            predicate.ValidateArgument(nameof(predicate));

            var index = 0;
            foreach(var item in list)
            {
                if (predicate(item))
                {
                    list.Insert(index, value);
                    return;
                }
                index++;
            }

            list.Add(value);
        }

        /// <summary>
        /// Updates the first object in <paramref name="list"/> matching the <paramref name="comparator"/> and replaces it with <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">Item type of <paramref name="list"/></typeparam>
        /// <param name="list">List to update value in</param>
        /// <param name="comparator">Comparator that tells which object to update</param>
        /// <param name="value">Value to update</param>
        /// <returns>Boolean indicating if <paramref name="list"/> was updated</returns>
        public static bool UpdateFirst<T>(this List<T> list, Comparator<T> comparator, T value)
        {
            return list.UpdateItemInEnumerable(comparator, value, true);
        }

        /// <summary>
        /// Updates the all objects in <paramref name="list"/> matching the <paramref name="comparator"/> and replaces it with <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">Item type of <paramref name="list"/></typeparam>
        /// <param name="list">List to update values in</param>
        /// <param name="comparator">Comparator that tells which objects to update</param>
        /// <param name="value">Value to update</param>
        /// <returns>Boolean indicating if <paramref name="list"/> was updated</returns>
        public static bool UpdateAll<T>(this List<T> list, Comparator<T> comparator, T value)
        {
            return list.UpdateItemInEnumerable(comparator, value, false);
        }

        /// <summary>
        /// Deletes the first object in <paramref name="list"/> matching the <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="T">Item type of <paramref name="list"/></typeparam>
        /// <param name="list">List to delete value in</param>
        /// <param name="comparator">Comparator that tells which object to delete</param>
        /// <param name="value">Value to delete</param>
        /// <returns>Boolean indicating if item in <paramref name="list"/> was deleted</returns>
        public static bool DeleteFirst<T>(this List<T> list, Comparator<T> comparator, T value)
        {
            return list.DeleteItemInEnumerable(comparator, value, true);
        }

        /// <summary>
        /// Deletes the all objects in <paramref name="list"/> matching the <paramref name="comparator"/>.
        /// </summary>
        /// <typeparam name="T">Item type of <paramref name="list"/></typeparam>
        /// <param name="list">List to delete values in</param>
        /// <param name="comparator">Comparator that tells which objects to delete</param>
        /// <param name="value">Value to delete</param>
        /// <returns>Boolean indicating if items in <paramref name="list"/> were deleted</returns>
        public static bool DeleteAll<T>(this List<T> list, Comparator<T> comparator, T value)
        {
            return list.DeleteItemInEnumerable(comparator, value, false);
        }

        #region Privates
        public static bool UpdateItemInEnumerable<T>(this List<T> list, Comparator<T> comparator, T value, bool onlyFirst = false)
        {
            var isUpdated = false;

            if (list.HasValue())
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];

                    if (item.HasValue() && value.HasValue() && comparator(value, item))
                    {
                        list[i] = value;

                        isUpdated = true;

                        if (onlyFirst)
                        {
                            return isUpdated;
                        }
                    }
                }
            }

            return isUpdated;
        }

        public static bool DeleteItemInEnumerable<T>(this List<T> list, Comparator<T> comparator, T value, bool onlyFirst = false)
        {
            var hasDeleted = false;

            if (list.HasValue())
            {
                foreach (var item in list)
                {
                    if (item.HasValue() && value.HasValue() && comparator(value, item))
                    {
                        list.Remove(item);
                        hasDeleted = true;

                        if (onlyFirst)
                        {
                            break;
                        }
                    }
                }
            }

            return hasDeleted;
        }
        #endregion
    }
}
