using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Extensions.Execution
{
    /// <summary>
    /// Provides additional extension methods for executing code based on the source object.
    /// </summary>
    public static class ExecutionExtensions
    {
        #region Execution
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<T> action)
        {
            action.ValidateArgument(nameof(action));
            // Parse to array to avoid triggering the enumerator multiple times.
            source = source.ToArrayOrDefault();

            if (source.HasValue())
            {
                foreach (var item in source)
                {
                    action(item);
                }
            }

            return source;
        }

        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Action to execute for each element</param>
        /// <param name="exceptionHandler">Delegate that handles exceptions thrown by <paramref name="action"/> before the exception is rethrown</param>
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> Execute<T>(this IEnumerable<T> source, Action<T> action, Action<T, Exception> exceptionHandler)
        {
            action.ValidateArgument(nameof(action));
            exceptionHandler.ValidateArgument(nameof(exceptionHandler));
            // Parse to array to avoid triggering the enumerator multiple times.
            source = source.ToArrayOrDefault();

            if (source.HasValue())
            {
                foreach (var item in source)
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

            return source;
        }

        public static IEnumerable<T> ForceExecute<T>(this IEnumerable<T> items, Action<T> action)
        {
            // Parse to array to avoid triggering the enumerator multiple times.
            items = items.ToArrayOrDefault();

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
            // Parse to array to avoid triggering the enumerator multiple times.
            items = items.ToArrayOrDefault();

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

        #region IfContains
        public static void IfContains<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Action<TValue> action)
        {
            action.ValidateArgument(nameof(action));

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
            action.ValidateArgument(nameof(action));

            if (boolean)
            {
                action();
            }
        }

        public static void IfFalse(this bool boolean, Action action)
        {
            action.ValidateArgument(nameof(action));

            if (!boolean)
            {
                action();
            }
        }
        #endregion

        #region IfHasValue
        public static void IfHasValue<T>(this T value, Action action)
        {
            if (value.HasValue())
            {
                action();
            }
        }

        public static void IfHasValue<T>(this T value, Predicate<T> requiredValueChecker, Action action)
        {
            if (requiredValueChecker(value))
            {
                action();
            }
        }

        public static T IfHasValue<T>(this T value, Func<T> func)
        {
            if (value.HasValue())
            {
                return func();
            }

            return value;
        }

        public static T IfHasValue<T>(this T value, Predicate<T> requiredValueChecker, Func<T> func)
        {
            if (requiredValueChecker(value))
            {
                return func();
            }

            return value;
        }
        #endregion

        #region IfHasNoValue
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

        #region Trim
        /// <summary>
        /// Returns an enumerator that trims the values in <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Strings to trim</param>
        /// <returns>An enumerator that trims each element</returns>
        public static IEnumerable<string> Trim(this IEnumerable<string> source)
        {
            source.ValidateArgument(nameof(source));

            return source.Select(x => x.Trim());
        }
        #endregion
    }
}
