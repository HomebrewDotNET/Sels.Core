using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sels.Core.Data.Contracts.Service
{
    /// <summary>
    /// Service that allows for crud operations on <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity the perform crud operations on</typeparam>
    /// <typeparam name="TId">The type of the primary id of <typeparamref name="TEntity"/></typeparam>
    public interface IDataService<TEntity, TId>
    {
        /// <summary>
        /// Opens a new connection that can be used to perform actions on this service.
        /// </summary>
        /// <param name="startTransaction">If a transaction must be created as well. Can be set to false when only reading data</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>A connection used to perform actions on the current service</returns>
        Task<IDataServiceConnection> OpenNewConnectionAsync(bool startTransaction = true, CancellationToken token = default);

        #region Create
        /// <summary>
        /// Persist <paramref name="entity"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entity">The entity to persist</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The created entity with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity> CreateAsync(IDataServiceConnection connection, TEntity entity, CancellationToken token = default);
        /// <summary>
        /// Persists all <paramref name="entities"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entities">The entities to persist</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The created entities with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity[]> CreateAsync(IDataServiceConnection connection, IEnumerable<TEntity> entities, CancellationToken token = default);
        #endregion
        #region Read
        /// <summary>
        /// Checks if an <typeparamref name="TEntity"/> with <paramref name="id"/> exists.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="id">The id of the entity to check</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>True if a <typeparamref name="TEntity"/> with <paramref name="id"/> exists, otherwise false</returns>
        Task<bool> ExistsAsync(IDataServiceConnection connection, TId id, CancellationToken token = default);
        /// <summary>
        /// Gets <typeparamref name="TEntity"/> with <paramref name="id"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="id">The id of the entity to fetch</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>Entity with <paramref name="id"/> or null if it doesn't exist.</returns>
        Task<TEntity> GetAsync(IDataServiceConnection connection, TId id, CancellationToken token = default);
        /// <summary>
        /// Gets all <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>All persisted <typeparamref name="TEntity"/></returns>
        Task<TEntity[]> GetAllAsync(IDataServiceConnection connection, CancellationToken token = default);
        #endregion
        #region Update
        /// <summary>
        /// Persists any changes to <paramref name="entity"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entity">The entity to update</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The updated entity with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity> UpdateAsync(IDataServiceConnection connection, TEntity entity, CancellationToken token = default);
        /// <summary>
        /// Persists any changes to all <paramref name="entities"/> to the database.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="entities">The entities to update</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The updates entities with any changes done by the database (Default values, incrementing ids, ...)</returns>
        Task<TEntity[]> UpdateAsync(IDataServiceConnection connection, IEnumerable<TEntity> entities, CancellationToken token = default);
        #endregion
        #region Delete
        /// <summary>
        /// Deletes <typeparamref name="TEntity"/> with id <paramref name="id"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="id">Id of the entity to delete</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The deleted entity</returns>
        Task<TEntity> DeleteAsync(IDataServiceConnection connection, TId id, CancellationToken token = default);
        /// <summary>
        /// Deletes all <typeparamref name="TEntity"/> with <paramref name="ids"/>.
        /// </summary>
        /// <param name="connection">The connection to perform the request with</param>
        /// <param name="ids">Ids of the entities to delete</param>
        /// <param name="token">Optional token to cancel the request</param>
        /// <returns>The deleted entities</returns>
        Task<TEntity[]> DeleteAsync(IDataServiceConnection connection, IEnumerable<TId> ids, CancellationToken token = default);
        #endregion
    }

    /// <summary>
    /// Represents an opened connection that can be used to perform actions on a <see cref="IDataService{TEntity, TId}"/>.
    /// </summary>
    public interface IDataServiceConnection : IDisposable
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
        /// Commits <see cref="Transaction"/> if it is not null.
        /// </summary>
        void Commit();
    }
}
