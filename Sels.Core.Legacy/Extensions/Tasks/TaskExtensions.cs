using System.Threading.Tasks;

namespace Sels.Core.Extensions.Tasks
{
    /// <summary>
    /// Contains extenion methods for wrkong with <see cref="Task"/>.
    /// </summary>
    public static  class TaskExtensions
    {
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
