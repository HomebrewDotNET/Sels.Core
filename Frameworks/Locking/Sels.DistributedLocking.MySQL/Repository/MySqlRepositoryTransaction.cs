using MySqlConnector;
using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.MySQL.Repository
{
    internal class MySqlRepositoryTransaction : IRepositoryTransaction
    {
        // Fields
        private readonly string _connectionString;

        // Properties
        public MySqlConnection Connection { get; private set; }
        public MySqlTransaction Transaction { get; private set; }

        public MySqlRepositoryTransaction(string connectionString)
        {
            _connectionString = connectionString.ValidateArgumentNotNullOrWhitespace(nameof(connectionString));
        }

        internal async Task OpenAsync(CancellationToken token)
        {
            var mySqlConnection = new MySqlConnection(_connectionString);
            await mySqlConnection.OpenAsync(token);
            Connection = mySqlConnection;
            Transaction = await mySqlConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);
        }

        /// <inheritdoc/>
        public void Commit()
        {
            if (Transaction == null) throw new InvalidOperationException($"No transaction started");

            Transaction.Commit();
        }
        /// <inheritdoc/>
        public async Task CommitAsync(CancellationToken token = default)
        {
            if (Transaction == null) throw new InvalidOperationException($"No transaction started");

            await Transaction.CommitAsync(token);
        }
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (Transaction != null) await Transaction.DisposeAsync();
            if (Connection != null) await Connection.DisposeAsync();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (Transaction != null) Transaction.Dispose();
            if (Connection != null) Connection.Dispose();
        }
    }
}
