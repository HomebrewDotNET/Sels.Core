using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Extensions.Linq.Async
{
    /// <summary>
    /// Extends the Linq extension methods with async variants
    /// </summary>
    public static class AsyncLinqExtensions
    {
        #region Execute
        /// <summary>
        /// Executes <paramref name="action"/> for each element in <paramref name="source"/> in parallel.
        /// </summary>
        /// <typeparam name="T">Type of element</typeparam>
        /// <param name="source">Enumerator that return the elements</param>
        /// <param name="action">Async action to execute for each element</param>
        /// <returns><paramref name="source"/></returns>
        public static IEnumerable<T> ExecuteAsync<T>(this IEnumerable<T> source, AsyncAction<T> action)
        {
            source.ValidateArgument(nameof(source));
            action.ValidateArgument(nameof(action));

            var tasks = new List<Task>();

            if (source.HasValue())
            {
                foreach (var item in source)
                {
                    tasks.Add(action(item));
                }
            }

            Task.WaitAll(tasks.ToArray());

            return source;
        }
        #endregion

        #region Any
        /// <inheritdoc cref="Enumerable.Any{TSource}(IEnumerable{TSource}, System.Func{TSource, bool})"/>
        public static async Task<bool> AnyAsync<T>(this IEnumerable<T> source, AsyncPredicate<T> predicate)
        {
            source.ValidateArgument(nameof(source));
            predicate.ValidateArgument(nameof(predicate));

            foreach(var item in source)
            {
                if(await predicate(item))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
