using MySqlConnector;
using Sels.Core.Data.Contracts.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Models.Data
{
    /// <summary>
    /// A <see cref="IDataServiceConnection"/> implemented using a MySql connection.
    /// </summary>
    public class MySqlDataServiceConnection : IDataServiceConnection
    {
        // Fields
        private readonly MySqlConnection _connection;
        private readonly bool _beginTransaction;

        /// <inheritdoc cref="MySqlDataServiceConnection"/>
        /// <param name="connectionString">The connection string to open the connection with</param>
        /// <param name="beginTransaction">If a transaction should be created for this connection</param>
        public MySqlDataServiceConnection(string connectionString, bool beginTransaction)
        {
            _connection = new MySqlConnection(connectionString);
            _beginTransaction = beginTransaction;
        }

        /// <inheritdoc/>
        public IDbConnection Connection => _connection;
        /// <inheritdoc/>
        public IDbTransaction? Transaction { get; private set; }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task to await the request</returns>
        public async Task OpenAsync(CancellationToken token = default)
        {
            await _connection.OpenAsync(token).ConfigureAwait(false);
            if (_beginTransaction) Transaction = await _connection.BeginTransactionAsync(token).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Commit()
        {
            if (Transaction != null) Transaction.Commit();
        }
        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
