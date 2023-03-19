using MySqlConnector;
using Sels.Core.Data.Contracts.Repository;
using System.Data;

namespace Sels.Core.Data.MySQL.Models.Repository
{
    /// <summary>
    /// A <see cref="IDataRepositoryConnection"/> implemented using a MySql connection.
    /// </summary>
    public class MySqlDataRepositoryConnection : IDataRepositoryConnection
    {
        // Fields
        private readonly MySqlConnection _connection;
        private MySqlTransaction? _transaction;
        private readonly bool _beginTransaction;
        private readonly IsolationLevel _isolationLevel;

        /// <inheritdoc cref="MySqlDataRepositoryConnection"/>
        /// <param name="connectionString">The connection string to open the connection with</param>
        /// <param name="beginTransaction">If a transaction should be created for this connection</param>
        /// <param name="isolationLevel">The isolation level for the transaction if <paramref name="beginTransaction"/> is set to true</param>
        public MySqlDataRepositoryConnection(string connectionString, bool beginTransaction, IsolationLevel isolationLevel = default)
        {
            _connection = new MySqlConnection(connectionString);
            _beginTransaction = beginTransaction;
            _isolationLevel = isolationLevel;
        }

        /// <inheritdoc/>
        public IDbConnection Connection => _connection;
        /// <inheritdoc/>
        public IDbTransaction? Transaction => _transaction;

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task to await the request</returns>
        public async Task OpenAsync(CancellationToken token = default)
        {
            await _connection.OpenAsync(token).ConfigureAwait(false);
            if (_beginTransaction) await CreateTransactionAsync(_isolationLevel, token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task CreateTransactionAsync(IsolationLevel isolationLevel = 0, CancellationToken token = default)
        {
            if (_transaction != null) throw new InvalidOperationException("Transaction already created");
            _transaction = await _connection.BeginTransactionAsync(token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task CommitAsync(CancellationToken token = default)
        {
            if (_transaction == null) throw new InvalidOperationException("No transaction to commit");

            await _transaction.CommitAsync(token);
            _transaction = null;
        }
        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }
        /// <summary>
        /// Closes the connection.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}
