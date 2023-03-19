using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Data.Contracts.Repository
{
    /// <summary>
    /// Repository that allows for crud operations on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity the perform crud operations on</typeparam>
    /// <typeparam name="TId">The type of the primary id of <typeparamref name="TEntity"/></typeparam>
    public interface IDataRepository<TEntity, TId>
    {
        /// <summary>
        /// Opens a new connection that can be used to perform actions on this repository.
        /// </summary>
        /// <param name="startTransaction">If a transaction must be created as well. Can be set to false when only reading data</param>
        /// <param name="isolationLevel">The isolation for the transaction if <paramref name="startTransaction"/> is set to true</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>A connection used to perform actions on the current service</returns>
        Task<IDataRepositoryConnection> OpenNewConnectionAsync(bool startTransaction = true, IsolationLevel isolationLevel = default, CancellationToken token = default);

        #region Create
        /// <summary>
        /// Persist <paramref name="entity"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entity">The entity to persist</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The created entity with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity> CreateAsync(IDataRepositoryConnection connection, TEntity entity, CancellationToken token = default);
        /// <summary>
        /// Persists all <paramref name="entities"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entities">The entities to persist</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The created entities with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity[]> CreateAsync(IDataRepositoryConnection connection, IEnumerable<TEntity> entities, CancellationToken token = default);
        #endregion
        #region Read
        /// <summary>
        /// Checks if an <typeparamref name="TEntity"/> with <paramref name="id"/> exists.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="id">The id of the entity to check</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>True if a <typeparamref name="TEntity"/> with <paramref name="id"/> exists, otherwise false</returns>
        Task<bool> ExistsAsync(IDataRepositoryConnection connection, TId id, CancellationToken token = default);
        /// <summary>
        /// Gets <typeparamref name="TEntity"/> with <paramref name="id"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="id">The id of the entity to fetch</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Entity with <paramref name="id"/> or null if it doesn't exist.</returns>
        Task<TEntity> GetAsync(IDataRepositoryConnection connection, TId id, CancellationToken token = default);
        /// <summary>
        /// Gets all <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All persisted <typeparamref name="TEntity"/></returns>
        Task<TEntity[]> GetAllAsync(IDataRepositoryConnection connection, CancellationToken token = default);
        #endregion
        #region Update
        /// <summary>
        /// Persists any changes to <paramref name="entity"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entity">The entity to update</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The updated entity with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity> UpdateAsync(IDataRepositoryConnection connection, TEntity entity, CancellationToken token = default);
        /// <summary>
        /// Persists any changes to all <paramref name="entities"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entities">The entities to update</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The updates entities with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity[]> UpdateAsync(IDataRepositoryConnection connection, IEnumerable<TEntity> entities, CancellationToken token = default);
        #endregion
        #region Delete
        /// <summary>
        /// Deletes <typeparamref name="TEntity"/> with id <paramref name="id"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="id">Id of the entity to delete</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The deleted entity</returns>
        Task<TEntity> DeleteAsync(IDataRepositoryConnection connection, TId id, CancellationToken token = default);
        /// <summary>
        /// Deletes all <typeparamref name="TEntity"/> with <paramref name="ids"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="ids">Ids of the entities to delete</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The deleted entities</returns>
        Task<TEntity[]> DeleteAsync(IDataRepositoryConnection connection, IEnumerable<TId> ids, CancellationToken token = default);
        #endregion
    }

    /// <summary>
    /// Represents an opened connection that can be used to perform actions on a <see cref="IDataRepository{TEntity, TId}"/>.
    /// </summary>
    public interface IDataRepositoryConnection : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// The opened database connection.
        /// </summary>
        IDbConnection Connection { get; }
        /// <summary>
        /// Optional transaction if one was created when creating the current connection.
        /// </summary>
        IDbTransaction Transaction { get; }
        /// <summary>
        /// Commits <see cref="Transaction"/>.
        /// </summary>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task to await the result</returns>
        Task CommitAsync(CancellationToken token = default);
        /// <summary>
        /// Creates a new transaction for the current connection.
        /// </summary>
        /// <param name="isolationLevel">The isolation level for the transaction</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Task to await the result</returns>
        Task CreateTransactionAsync(IsolationLevel isolationLevel = default, CancellationToken token = default);
    }
}
