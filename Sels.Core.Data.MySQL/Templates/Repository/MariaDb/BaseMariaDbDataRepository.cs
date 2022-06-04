﻿using Dapper;
using Microsoft.Extensions.Logging;
using Sels.Core.Data.Contracts.Repository;
using Sels.Core.Data.MySQL.MariaDb;
using Sels.Core.Data.MySQL.Models.Repository;
using Sels.Core.Data.SQL.Extensions.Dapper;
using Sels.Core.Data.SQL.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Fluent;
using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;
using System.Data;

namespace Sels.Core.Data.MySQL.Templates.Repository.MariaDb
{
    /// <summary>
    /// Template for creating a new <see cref="IDataRepository{TEntity, TId}"/> using MariaDb as the backing database.
    /// Requires mariaDb 10.5.0 or later due to use of the RETURNING keyword.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity the perform crud operations on</typeparam>
    /// <typeparam name="TId">The type of the primary id of <typeparamref name="TEntity"/></typeparam>
    public abstract class BaseMariaDbDataRepository<TEntity, TId> : IDataRepository<TEntity, TId>
    {
        // Fields
        private readonly IEnumerable<ILogger>? _loggers;
        private readonly string _connectionString;
        private readonly string _name;

        // Properties
        private Lazy<Func<TEntity, TId>> IdGetter { get; }
        /// <summary>
        /// The property of the id column on <typeparamref name="TEntity"/>.
        /// </summary>
        protected Lazy<PropertyInfo> IdProperty { get; }
        /// <summary>
        /// The properties that will be excluded as columns from the query.
        /// </summary>
        protected Lazy<string[]?> ExcludedProperties { get; }

        /// <inheritdoc cref="BaseMariaDbDataRepository{TEntity, TId}"/>
        /// <param name="connectionString">The connection string to use to open connections</param>
        /// <param name="loggers">Optional loggers for tracing</param>
        public BaseMariaDbDataRepository(string connectionString, IEnumerable<ILogger>? loggers = null)
        {
            _connectionString = connectionString.ValidateArgumentNotNullOrWhitespace(nameof(connectionString));
            _loggers = loggers;

            // Initialize
            _name = GetType().Name;
            IdProperty = new Lazy<PropertyInfo>(() => GetIdProperty() ?? throw new InvalidOperationException($"{nameof(GetIdProperty)} returned null"), true);
            ExcludedProperties = new Lazy<string[]>(() => Helper.Collection.Enumerate(IdProperty.Value.Name, GetExcludedProperties().ToArrayOrDefault()).ToArrayOrDefault(), true);
            IdGetter = new Lazy<Func<TEntity, TId>>(() =>
            {
                // Compile property info into delegate for faster execution.
                var parameterExpression = LinqExpression.Parameter(typeof(TEntity), "x");
                System.Linq.Expressions.Expression memberExpression = LinqExpression.Property(parameterExpression, IdProperty.Value);
                if (IdProperty.Value.PropertyType != typeof(TId)) memberExpression = LinqExpression.Convert(memberExpression, typeof(TId));

                return LinqExpression.Lambda<Func<TEntity, TId>>(memberExpression, parameterExpression).Compile();
            }, true);
        }

        /// <inheritdoc/>
        public async Task<IDataRepositoryConnection> OpenNewConnectionAsync(bool startTransaction = true, IsolationLevel isolationLevel = default, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                _loggers.Debug($"Opening new connection from <{_name}>");
                var connection = new MySqlDataRepositoryConnection(_connectionString, startTransaction, isolationLevel);
                await connection.OpenAsync(token);
                return connection;
            }

        }

        #region Create
        /// <inheritdoc/>
        public virtual async Task<TEntity> CreateAsync(IDataRepositoryConnection connection, TEntity entity, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                entity.ValidateArgument(nameof(entity));
                var excludedProperties = ExcludedProperties.Value;
                _loggers.Debug($"Creating new entity of type <{typeof(TEntity)}>");

                // Create query
                var query = MySql.Insert<TEntity>().Into().ColumnsOf(excludedProperties)
                                    .ParametersFrom(excludedProperties: excludedProperties)
                                    .Return(x => x.All())
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Inserting <{typeof(TEntity)}> using query: {query}");

                // Create parameters
                var parameters = entity.AsParameters(excludedProperties);

                // Execute query
                entity = await connection.Connection.QuerySingleAsync<TEntity>(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token));
                var id = IdGetter.Value(entity);
                _loggers.Debug($"Inserted <{typeof(TEntity)}> with id <{id}>");
                return entity;
            }
        }
        /// <inheritdoc/>
        public virtual async Task<TEntity[]> CreateAsync(IDataRepositoryConnection connection, IEnumerable<TEntity> entities, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                entities.ValidateArgumentNotNullOrEmpty(nameof(entities));
                var excludedProperties = ExcludedProperties.Value;
                var entityCount = entities.GetCount();
                _loggers.Debug($"Creating {entityCount} new entities of type <{typeof(TEntity)}>");

                // Create query and parameters
                var parameters = new DynamicParameters();
                var query = MySql.Insert<TEntity>().Into().ColumnsOf(excludedProperties)
                                    .ForEach(entities, (b, i, e) => {
                                        parameters.AddParametersUsing(e, x => $"{x.Name}{i}", excludedProperties);
                                        return b.ParametersFrom(i, excludedProperties);
                                    })
                                    .Return(x => x.All())
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Inserting <{typeof(TEntity)}> using query: {query}");

                // Execute query
                var inserted = (await connection.Connection.QueryAsync<TEntity>(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token))).ToArray();
                var ids = entities.Select(x => IdGetter.Value(x));
                _loggers.Debug($"Inserted {inserted.Length} <{typeof(TEntity)}> with ids <{ids.JoinString(',')}>");
                return inserted;
            }
        }
        #endregion

        #region Read
        /// <inheritdoc/>
        public virtual async Task<bool> ExistsAsync(IDataRepositoryConnection connection, TId id, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                id.ValidateArgument(nameof(id));
                _loggers.Debug($"Checking if an entity of type <{typeof(TEntity)}> exists with id <{id}>");
                var property = IdProperty.Value;

                // Create query and parameters
                var parameters = new DynamicParameters().AddParameter(property.Name, id);
                var query = MySql.Select<TEntity>().Value(1).From()
                                    .Where(x => x.Column(typeof(TEntity), property.Name).EqualTo.Parameter(property.Name))
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Checking existance of <{typeof(TEntity)}> with id <{id}> using query: {query}");

                // Execute query
                var exists = (await connection.Connection.QuerySingleAsync<int>(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token))) == 1;
                _loggers.Debug($"Entity <{typeof(TEntity)}> with id <{id}> {(exists ? "exists" : "does not exist")}");
                return exists;
            }
        }
        /// <inheritdoc/>
        public virtual async Task<TEntity[]> GetAllAsync(IDataRepositoryConnection connection, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                _loggers.Debug($"Fetching all entities of type <{typeof(TEntity)}>");

                // Create query
                var query = MySql.Select<TEntity>().All().From()
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Fetching all entities of type <{typeof(TEntity)}> using query: {query}");

                // Execute query
                var entities = (await connection.Connection.QueryAsync<TEntity>(new CommandDefinition(query, transaction: connection.Transaction, cancellationToken: token))).ToArray();
                _loggers.Debug($"Fetched <{entities.Length}> entities of type <{typeof(TEntity)}>");
                return entities;
            }
        }
        /// <inheritdoc/>
        public virtual async Task<TEntity> GetAsync(IDataRepositoryConnection connection, TId id, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                id.ValidateArgument(nameof(id));
                var property = IdProperty.Value;
                _loggers.Debug($"Fetching entity of type <{typeof(TEntity)}> with id <{id}>");

                // Create query and parameters
                var parameters = new DynamicParameters().AddParameter(property.Name, id);
                var query = MySql.Select<TEntity>().All().From()
                                    .Where(x => x.Column(typeof(TEntity), property.Name).EqualTo.Parameter(property.Name))
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Fetching entity of type <{typeof(TEntity)}> with id <{id}> using query: {query}");

                // Execute query
                var entity = await connection.Connection.QuerySingleAsync<TEntity>(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token));
                if(entity != null)
                {
                    _loggers.Debug($"Fetched entity of type <{typeof(TEntity)}> with id <{id}>");
                }
                else
                {
                    _loggers.Warning($"Could not fetch entity of type <{typeof(TEntity)}> with id <{id}> because it does not exist");
                }
                
                return entity;
            }
        }
        #endregion

        #region Update
        /// <inheritdoc/>
        public virtual async Task<TEntity> UpdateAsync(IDataRepositoryConnection connection, TEntity entity, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                entity.ValidateArgument(nameof(entity));
                var excludedProperties = ExcludedProperties.Value;
                var id = IdGetter.Value(entity);
                var idName = IdProperty.Value.Name;
                _loggers.Debug($"Updating entity of type <{typeof(TEntity)}> with id <{id}>");

                // Create query
                var query = MySql.Update<TEntity>().Table()
                                    .SetFrom(excludedProperties: excludedProperties)
                                    .Where(w => w.Column(typeof(TEntity), idName).EqualTo.Parameter(idName))
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Updating <{typeof(TEntity)}> with id <{id}> using query: {query}");

                // Create parameters
                var parameters = entity.AsParameters(excludedProperties.Where(x => !x.Equals(idName, StringComparison.OrdinalIgnoreCase)).ToArrayOrDefault());

                // Execute query
                await connection.Connection.ExecuteAsync(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token));
                _loggers.Debug($"Updated entity of type <{typeof(TEntity)}> with id <{id}>");
                return await GetAsync(connection, id, token);
            }
        }
        /// <inheritdoc/>
        public virtual async Task<TEntity[]> UpdateAsync(IDataRepositoryConnection connection, IEnumerable<TEntity> entities, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                entities.ValidateArgument(nameof(entities));
                var entityCount = entities.GetCount();
                var idGetter = IdGetter.Value;
                _loggers.Debug($"Updating <{entityCount}> entity of type <{typeof(TEntity)}>");

                // Trigger tasks in parallel and get result                
                var updatedEntities = await Task.WhenAll(entities.Select(x => UpdateAsync(connection, x, token)));

                _loggers.Debug($"Updated <{typeof(TEntity)}> with ids <{updatedEntities.Select(x => idGetter(x)).JoinString(',')}>");
                return entities.ToArray();
            }
        }
        #endregion

        #region Delete
        /// <inheritdoc/>
        public virtual async Task<TEntity> DeleteAsync(IDataRepositoryConnection connection, TId id, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                var idName = IdProperty.Value.Name;
                _loggers.Debug($"Deleting entity of type <{typeof(TEntity)}> with id <{id}>");

                // Create query
                var query = MySql.Delete<TEntity>().From()
                                    .Where(w => w.Column(typeof(TEntity), idName).EqualTo.Parameter(idName))
                                    .Return(x => x.All())
                                    .AliasFor<TEntity>(string.Empty).Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Deleting <{typeof(TEntity)}> with id <{id}> using query: {query}");

                // Create parameters
                var parameters = new DynamicParameters().AddParameter(IdProperty.Value.Name, id);

                // Execute query
                var deleted = await connection.Connection.QuerySingleAsync<TEntity>(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token));
                _loggers.Debug($"Deleted <{typeof(TEntity)}> with id <{id}>");
                return deleted;
            }
        }
        /// <inheritdoc/>
        public virtual async Task<TEntity[]> DeleteAsync(IDataRepositoryConnection connection, IEnumerable<TId> ids, CancellationToken token = default)
        {
            using (_loggers.TraceMethod(this))
            {
                connection.ValidateArgument(nameof(connection));
                ids.ValidateArgument(nameof(ids));
                var idGetter = IdGetter.Value;
                var idName = IdProperty.Value.Name;
                var idCount = ids.GetCount();
                _loggers.Debug($"Deleting <{idCount}> entity of type <{typeof(TEntity)}>");

                // Create query
                var query = MySql.Delete<TEntity>().From()
                                    .Where(w => w.Column(typeof(TEntity), idName).In.Values(ids.Select<TId, object>((x, i) => $"{idName}{i}".AsParameterExpression())))
                                    .Return(x => x.All())
                                    .Build(ExpressionCompileOptions.Format);
                _loggers.Trace($"Deleting <{idCount}> <{typeof(TEntity)}> using query: {query}");

                // Create parameters
                var parameters = new DynamicParameters();
                ids.Execute((i, x) => parameters.AddParameter(idName + i, x));

                // Execute query
                var deleted = (await connection.Connection.QueryAsync<TEntity>(new CommandDefinition(query, parameters, connection.Transaction, cancellationToken: token))).ToArray();
                _loggers.Debug($"Deleted <{typeof(TEntity)}> with ids <{deleted.Select(x => idGetter(x)).JoinString(',')}>");
                return deleted;
            }
        }
        #endregion

        // Virtuals
        /// <summary>
        /// Gets the names of the properties on <typeparamref name="TEntity"/> not to include in queries.
        /// </summary>
        /// <returns>Enumerator returning the names of the properties to ignore or null if no properties need to be ignored</returns>
        protected virtual IEnumerable<string>? GetExcludedProperties()
        {
            foreach(var property in typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetIndexParameters().Length != 0) yield return property.Name;

                var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if(!type.IsPrimitive && !type.IsString() && !type.Is<DateTime>() && !type.Is<DateTimeOffset>() && !type.Is<Guid>()) yield return property.Name;
            }
        }

        // Abstractions
        /// <summary>
        /// Returns the property info of the id property on <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>The property info of the id property on <typeparamref name="TEntity"/></returns>
        protected abstract PropertyInfo GetIdProperty();
    }
}
