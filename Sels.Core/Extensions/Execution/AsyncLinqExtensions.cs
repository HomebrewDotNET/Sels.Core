using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using static Sels.Core.Delegates.Async;

namespace Sels.Core.Extensions.Execution
{
    /// <summary>
    /// Extends the Linq extension methods with async variants
    /// </summary>
    public static class AsyncLinqExtensions
    {
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
