using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.DistributedLocking.Abstractions.Models
{
    /// <inheritdoc cref="ILockQueryResult"/>
    public class LockQueryResult : ILockQueryResult
    {
        /// <inheritdoc/>
        public ILockInfo[] Results { get; }
        /// <inheritdoc/>
        public int MaxPages { get; } = 1;

        /// <summary>
        /// Creates a query result using the query parameters.
        /// </summary>
        /// <param name="results">The query results</param>
        /// <param name="pageSize">The page size that was used</param>
        /// <param name="matchingResults">How many total results match the used query parameters</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LockQueryResult(IEnumerable<ILockInfo> results, int pageSize, int matchingResults) : this(results)
        {
            if(pageSize <= 0) throw new ArgumentException($"{nameof(pageSize)} must be larger than 0");
            if (matchingResults > 0) MaxPages = (matchingResults -1) / pageSize + 1;
        }

        /// <summary>
        /// Creates a query result.
        /// </summary>
        /// <param name="results">The query results</param>
        /// <exception cref="ArgumentNullException"></exception>
        public LockQueryResult(IEnumerable<ILockInfo> results)
        {
            Results = results?.ToArray() ?? throw new ArgumentNullException(nameof(results));
        }
    }
}
