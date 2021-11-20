using Sels.Core.Extensions.Conversion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Extensions.Execution
{
    /// <summary>
    /// Provides additional extension methods for executing async code based on the source object.
    /// </summary>
    public static class AsyncExecutionExtensions
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
            action.ValidateArgument(nameof(action));
            // Parse to array to avoid triggering the enumerator multiple times.
            source = source.ToArrayOrDefault();

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
    }
}
