using Sels.Core.Extensions.General.Generic;
using Sels.Core.Extensions.General.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Extensions.Execution.Linq
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> WherePredicate<T>(this IEnumerable<T> items, Predicate<T> predicate)
        {
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

        #region Execution
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    action(item);
                }
            }

            return items;
        }

        public static IEnumerable<T> Execute<T>(this IEnumerable<T> items, Action<T> action, Action<T, Exception> exceptionHandler)
        {
            if (items.HasValue())
            {

                foreach (var item in items)
                {
                    try
                    {
                        action(item);
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler(item, ex);
                        throw;
                    }
                }
            }

            return items;
        }

        public static async Task<IEnumerable<T>> ExecuteAsync<T>(this IEnumerable<T> items, Func<T, Task> action)
        {
            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    await action(item);
                }
            }

            return items;
        }

        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    action.ForceExecute(item);
                }
            }

            return items;
        }

        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> items, Action<T> action, Action<T, Exception> exceptionHandler)
        {
            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    try
                    {
                        action(item);
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler.ForceExecute(item, ex);
                    }

                }
            }

            return items;
        }

        public static IEnumerable<TOut> ForceSelect<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> function)
        {
            List<TOut> newItems = new List<TOut>();

            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    try
                    {
                        newItems.Add(function(item));
                    }
                    catch { }
                }
            }

            return newItems;
        }

        public static IEnumerable<TOut> ForceSelect<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> function, Action<TIn, Exception> exceptionHandler)
        {
            List<TOut> newItems = new List<TOut>();

            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    try
                    {
                        newItems.Add(function(item));
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler.ForceExecute(item, ex);
                    }
                }
            }

            return newItems;
        }

        #region  ForceExecuteAction
        public static void ForceExecute<T>(this Action<T> action, T item)
        {
            try
            {
                action(item);
            }
            catch { }
        }

        public static void ForceExecute<TOne, TTwo>(this Action<TOne, TTwo> action, TOne itemOne, TTwo itemTwo)
        {
            try
            {
                action(itemOne, itemTwo);
            }
            catch { }
        }

        public static void ForceExecute<T>(this Action<T> action, T item, Action<T, Exception> exceptionHandler)
        {
            try
            {
                action(item);
            }
            catch (Exception ex)
            {
                exceptionHandler.ForceExecute(item, ex);
            }
        }

        public static void ForceExecute<TOne, TTwo>(this Action<TOne, TTwo> action, TOne itemOne, TTwo itemTwo, Action<TOne, TTwo, Exception> exceptionHandler)
        {
            try
            {
                action(itemOne, itemTwo);
            }
            catch (Exception ex)
            {
                try
                {
                    exceptionHandler(itemOne, itemTwo, ex);
                }
                catch { }
            }
        }
        #endregion

        #region ExecuteOrDefault
        public static void ExecuteOrDefault(this Action action)
        {
            if (action.HasValue())
            {
                action();
            }
        }
        public static void ExecuteOrDefault<T>(this Action<T> action, T item)
        {
            if (action.HasValue())
            {
                action(item);
            }
        }
        #endregion

        #region ExecuteOrDefault
        public static void ForceExecuteOrDefault(this Action action)
        {
            try
            {
                if (action.HasValue())
                {
                    action();
                }
            }
            catch { }
        }
        public static void ForceExecuteOrDefault<T>(this Action<T> action, T item)
        {
            try
            {
                if (action.HasValue())
                {
                    action(item);
                }
            }
            catch { }
        }
        #endregion

        #endregion

        #region ValueChecker
        public static void IfHasNoValue<T>(this T value, Action action)
        {
            if (!value.HasValue())
            {
                action();
            }
        }

        public static void IfHasNoValue<T>(this T value, Predicate<T> requiredValueChecker, Action action)
        {
            if (!requiredValueChecker(value))
            {
                action();
            }
        }

        public static T IfHasNoValue<T>(this T value, Func<T> func)
        {
            if (!value.HasValue())
            {
                return func();
            }

            return value;
        }

        public static T IfHasNoValue<T>(this T value, Predicate<T> requiredValueChecker, Func<T> func)
        {
            if (!requiredValueChecker(value))
            {
                return func();
            }

            return value;
        }
        #endregion

        #region IfContains
        public static void IfContains<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Action<TValue> action)
        {
            action.ValidateVariable(nameof(action));

            if (dictionary.HasValue() && key != null)
            {
                if (dictionary.ContainsKey(key))
                {
                    action(dictionary[key]);
                }
            }
        }
        #endregion

        #region IfBool
        public static void IfTrue(this bool boolean, Action action)
        {
            action.ValidateVariable(nameof(action));

            if (boolean)
            {
                action();
            }
        }

        public static void IfFalse(this bool boolean, Action action)
        {
            action.ValidateVariable(nameof(action));

            if (!boolean)
            {
                action();
            }
        }
        #endregion

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

        public static IEnumerable<TItem> SelectCollection<T, TItem>(this IEnumerable<T> items, Func<T, IEnumerable<TItem>> select)
        {
            if (items.HasValue())
            {
                foreach (var item in items)
                {
                    foreach (var collectionItem in select(item))
                    {
                        yield return collectionItem;
                    }
                }
            }
        }

        #endregion
    }
}
