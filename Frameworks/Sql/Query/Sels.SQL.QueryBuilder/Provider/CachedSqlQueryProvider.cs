using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Builder.Compilation;
using Sels.Core.Extensions.Logging.Advanced;
using System;
using System.Collections.Generic;
using System.Text;
using Sels.Core.Extensions.Logging;

namespace Sels.SQL.QueryBuilder.Provider
{
    /// <inheritdoc cref="ICachedSqlQueryProvider"/>
    public class CachedSqlQueryProvider : SqlQueryProvider, ICachedSqlQueryProvider
    {
        // Fields
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly ILogger _logger;
        private ExpressionCompileOptions _compileOptions;

        /// <inheritdoc cref="CachedSqlQueryProvider"/>
        /// <param name="cache">The cache to use</param>
        /// <param name="cacheOptions">The options for the cached queries</param>
        /// <param name="compiler">The compiler to use for the statement builders</param>
        /// <param name="compileOptions">The default compile options when returning a IQueryBuilder</param>
        /// <param name="logger">Optional logger for tracing</param>
        public CachedSqlQueryProvider(IMemoryCache cache, MemoryCacheEntryOptions cacheOptions, ISqlCompiler compiler, ExpressionCompileOptions compileOptions = ExpressionCompileOptions.None, ILogger<CachedSqlQueryProvider> logger = null) : base(compiler)
        {
            _cache = cache.ValidateArgument(nameof(cache));
            _cacheOptions = cacheOptions.ValidateArgument(nameof(cacheOptions));
            _compileOptions = compileOptions;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string GetQuery(string queryName, Func<ISqlQueryProvider, IQueryBuilder> queryBuilder)
        {
            queryName.ValidateArgumentNotNullOrWhitespace(nameof(queryName));
            queryBuilder.ValidateArgument(nameof(queryBuilder));

            var cacheKey = $"{typeof(CachedSqlQueryProvider).FullName}.{queryName}";

            if(_cache.TryGetValue<string>(cacheKey, out var query))
            {
                _logger.Log($"Retrieved query <{queryName}> of length <{query.Length}> from cache");
                return query;
            }
            else
            {
                _logger.Debug($"Query with name <{queryName}> is not cached. Generating");
                var builder = new StringBuilder();
                using(_logger.CreateTimedLogger(LogLevel.Information, $"Generating query <{queryName}>", x => $"Generated query <{queryName}> of length <{builder.Length}> in <{x.PrintTotalMs()}>"))
                {
                    var configuredQueryBuilder = queryBuilder(this);
                    if (configuredQueryBuilder == null) new InvalidOperationException($"{nameof(queryBuilder)} returned null");
                    configuredQueryBuilder.Build(builder, _compileOptions);
                }

                _logger.Debug($"Caching query <{queryName}> of length <{builder.Length}>");
                query = builder.ToString();             
                _cache.Set(cacheKey, query, _cacheOptions);
                return query;
            }
        }

        /// <inheritdoc/>
        public string GetQuery(string queryName, Func<ISqlQueryProvider, string> queryBuilder)
        {
            queryName.ValidateArgumentNotNullOrWhitespace(nameof(queryName));
            queryBuilder.ValidateArgument(nameof(queryBuilder));

            var cacheKey = $"{typeof(CachedSqlQueryProvider).FullName}.{queryName}";

            if (_cache.TryGetValue<string>(cacheKey, out var query))
            {
                _logger.Log($"Retrieved query <{queryName}> of length <{query.Length}> from cache");
                return query;
            }
            else
            {
                _logger.Debug($"Query with name <{queryName}> is not cached. Generating");
                using (_logger.CreateTimedLogger(LogLevel.Information, $"Generating query <{queryName}>", x => $"Generated query <{queryName}> of length <{query?.Length}> in <{x.PrintTotalMs()}>"))
                {
                    query = queryBuilder(this);
                    if (!query.HasValue()) new InvalidOperationException($"{nameof(queryBuilder)} returned an empty query string");
                }

                _logger.Debug($"Caching query <{queryName}> of length <{query.Length}>");
                _cache.Set(cacheKey, query, _cacheOptions);
                return query;
            }
        }
    }
}
