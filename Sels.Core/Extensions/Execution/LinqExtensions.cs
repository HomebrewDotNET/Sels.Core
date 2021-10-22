using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Extensions.Linq
{
    /// <summary>
    /// Extra extension methods that follow the same setup as Linq.
    /// </summary>
    public static class LinqExtensions
    {
        public static IEnumerable<T> WherePredicate<T>(this IEnumerable<T> items, Predicate<T> predicate)
        {
            // Parse to array to avoid triggering the enumerator multiple times.
            items = items.ToArrayOrDefault();

            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    if (predicate(item))
                    {
                        yield return item;
                    }
                }
            }
        }     

        #region Select
        public static TSelect[] SelectOrDefault<T, TSelect>(this T[] items, Func<T, TSelect> select)
        {
            if (items.HasValue())
            {
                var list = new List<TSelect>();

                foreach (var item in items)
                {
                    list.Add(select(item));
                }

                return list.ToArray();
            }
            else
            {
                return default;
            }
        }

        public static IEnumerable<TSelect> SelectOrDefault<T, TSelect>(this IEnumerable<T> items, Func<T, TSelect> select)
        {
            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    yield return select(item);
                }
            }
        }
        #endregion

        #region ForceSelect
        /// <summary>
        /// Projects each element of a sequence into a new form. Exception are caught so execution doesn't stop.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TSelect">The type of the value returned by selector.</typeparam>
        /// <param name="source"> A sequence of values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <param name="exceptionHandler">Optional action for handling exceptions. First argument is the element that caused the exception en the second arg is the thrown exception</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 whose elements are the result of invoking the transform function on each element of source.</returns>
        public static IEnumerable<TSelect> ForceSelect<TSource, TSelect>(this IEnumerable<TSource> source, Func<TSource, TSelect> selector, Action<TSource, Exception> exceptionHandler = null)
        {
            if (source.HasValue())
            {
                foreach(var item in source)
                {

                    TSelect selectedItem;
                    try
                    {
                        selectedItem = selector(item);
                    }
                    catch(Exception ex)
                    {
                        try
                        {
                            if (exceptionHandler.HasValue())
                            {
                                exceptionHandler(item, ex);
                            }
                        }
                        catch { }

                        continue;
                    }

                    yield return selectedItem;
                }
            }
        }
        #endregion

        #region Modify 
        /// <summary>
        /// Modifies item in array using modifier if condition is true
        /// </summary>
        public static T[] ModifyItemIf<T>(this T[] source, Predicate<T> condition, Func<T,T> modifier)
        {
            condition.ValidateArgument(nameof(condition));
            modifier.ValidateArgument(nameof(modifier));

            if (source.HasValue())
            {
                for(int i = 0; i < source.Length; i++)
                {
                    var item = source[i];

                    if (condition(item))
                    {
                        source[i] = modifier(item);
                    }
                }
            }

            return source;
        }

        /// <summary>
        /// Modifies item in collection using modifier if condition is true
        /// </summary>
        public static IEnumerable<T> ModifyItemIf<T>(this IEnumerable<T> source, Predicate<T> condition, Func<T, T> modifier)
        {
            condition.ValidateArgument(nameof(condition));
            modifier.ValidateArgument(nameof(modifier));

            if (source.HasValue())
            {
                foreach(var item in source)
                {
                    if (condition(item))
                    {
                        yield return modifier(item);
                    }
                    else
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <summary>
        /// Returns modified object from modifier if confition is true, otherwise return item.
        /// </summary>
        public static T ModifyIf<T>(this T item, Predicate<T> condition, Func<T, T> modifier)
        {
            condition.ValidateArgument(nameof(condition));
            modifier.ValidateArgument(nameof(modifier));

            if (condition(item))
            {
                return modifier(item);
            }

            return item;
        }
        #endregion

        #region Enumerate
        /// <summary>
        /// Yield return any object in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Collection to get items from</param>
        /// <returns>Objects in <paramref name="source"/></returns>
        public static IEnumerable<object> Enumerate(this IEnumerable source)
        {
            if(source != null)
            {
                foreach(var item in source)
                {
                    yield return item;
                }
            }
        }
        #endregion
    }
}
