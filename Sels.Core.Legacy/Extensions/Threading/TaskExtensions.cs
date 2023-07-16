using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Extensions.Threading
{
    /// <summary>
    /// Contains extension methods for the <see cref="Task"/> domain.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Transfers the result from <paramref name="task"/> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to set</param>
        /// <param name="task">The task to get the result from</param>
        /// <returns>Task containing the execution state</returns>
        public static async Task SetFromAsync(this TaskCompletionSource<object> source, Task task)
        {
            source.ValidateArgument(nameof(source));
            _ = task.ValidateArgument(nameof(task));

            try
            {
                // Can throw
                await task;

                source.SetResult(null);
            }
            catch(AggregateException aggEx) when (aggEx.InnerExceptions.Count == 1)
            {
                source.SetException(aggEx.InnerExceptions.First());
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }
        }

        /// <summary>
        /// Transfers the result from <paramref name="task"/> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to set</param>
        /// <param name="task">The task to get the result from</param>
        /// <typeparam name="T">The type of the result to transfer</typeparam>
        /// <returns>Task containing the execution state</returns>
        public static async Task SetFromAsync<T>(this TaskCompletionSource<T> source, Task<T> task)
        {
            source.ValidateArgument(nameof(source));
            _ = task.ValidateArgument(nameof(task));

            try
            {
                // Can throw
                source.SetResult(await task);
            }
            catch (AggregateException aggEx) when (aggEx.InnerExceptions.Count == 1)
            {
                source.SetException(aggEx.InnerExceptions.First());
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }
        }

        /// <summary>
        /// Transfers the result from <paramref name="task"/> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to set</param>
        /// <param name="task">The task to get the result from</param>
        /// <returns>Task containing the execution state</returns>
        public static void SetFrom(this TaskCompletionSource<object> source, Task task)
        {
            source.ValidateArgument(nameof(source));
            _ = task.ValidateArgument(nameof(task));

            try
            {
                // Can throw
                task.GetAwaiter().GetResult();

                source.SetResult(null);
            }
            catch (AggregateException aggEx) when (aggEx.InnerExceptions.Count == 1)
            {
                source.SetException(aggEx.InnerExceptions.First());
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }
        }

        /// <summary>
        /// Transfers the result from <paramref name="task"/> to <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to set</param>
        /// <param name="task">The task to get the result from</param>
        /// <typeparam name="T">The type of the result to transfer</typeparam>
        /// <returns>Task containing the execution state</returns>
        public static void SetFrom<T>(this TaskCompletionSource<T> source, Task<T> task)
        {
            source.ValidateArgument(nameof(source));
            _ = task.ValidateArgument(nameof(task));

            try
            {
                // Can throw
                source.SetResult(task.Result);
            }
            catch (AggregateException aggEx) when (aggEx.InnerExceptions.Count == 1)
            {
                source.SetException(aggEx.InnerExceptions.First());
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }
        }

        /// <summary>
        /// Creates a task result using <paramref name="result"/>.
        /// </summary>
        /// <typeparam name="T">Type of the result</typeparam>
        /// <param name="result">The result to wrap</param>
        /// <returns>Task wrapped around <paramref name="result"/></returns>
        public static Task<T> ToTaskResult<T>(this T result)
        {
            return Task.FromResult(result);
        }
    }
}
