using Sels.Core.Extensions.General.Generic;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
