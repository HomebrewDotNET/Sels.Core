using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Data.Contracts.Repository
{
    /// <summary>
    /// Represents a transaction for a data repository that can be committed or rollbacked.
    /// </summary>
    public interface IRepositoryTransaction : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task cotnaining the execution state</returns>
        Task CommitAsync(CancellationToken token = default);
        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        void Commit();
    }
}
